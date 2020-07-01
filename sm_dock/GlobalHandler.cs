using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

namespace sm_dock
{
    internal static partial class GlobalHandler
    {
        //Helpers
        public static int width;
        public static int height;
        public static int left;
        public static int top;
        public static int right;
        public static int bottom;
        //Half-size
        public static int icon_hsize;
        //Size with padding
        public static int icon_nsize;

        //Error message on load
        public static int load_lvl;

        public static int LoadIcons(Dock dock)
        {
            load_lvl = 0;
            //The icon cache must be consistent with the ini file, otherwise errors will occour
            List<int> img_addrs = new List<int>();
            int img_count;
            using (StreamReader sr = new StreamReader(CFG_FILE, DEF_ENC))
            {
                LoadData(sr);
                load_lvl = 1;
                icon_hsize = icon_size / 2;
                icon_nsize = icon_size + 2 * icon_pad;
                using (BinaryReader br = new BinaryReader(new FileStream(CACHE_FILE, FileMode.Open, FileAccess.Read), DEF_ENC))
                {
                    //Reading file pointers
                    img_count = br.ReadInt32();
                    for (int i = 0; i < img_count; i++)
                    {
                        img_addrs.Add(br.ReadInt32());
                    }
                    int img_start = img_addrs[0];
                    //Dummy address to skip the file length check
                    img_count--;
                    //Reading PNG images
                    for (int i = 0; i < img_count; i++)
                    {
                        int img_len = img_addrs[i + 1] - img_start;
                        img_start += img_len;
                        byte[] img_data = br.ReadBytes(img_len);
                        Bitmap img = new Bitmap(new MemoryStream(img_data));
                        string[] icon_data = sr.ReadLine().Split(CFG_DELIM);
                        IconControl icon = new IconControl(
                            icon_data[ICON_TEXT],
                            icon_data[ICON_FILENAME],
                            icon_data[ICON_STARTARG],
                            icon_data[ICON_WORKDIR],
                            win_style[ParseInt(icon_data[ICON_WINDOW])],
                            img);
                        if (dock_pos < 2)
                        {
                            icon.SetBounds(
                                i * icon_nsize,
                                0,
                                icon_nsize,
                                icon_nsize + icon_line);
                        }
                        else
                        {
                            icon.SetBounds(
                                0,
                                i * icon_nsize,
                                icon_nsize + icon_line,
                                icon_nsize);
                        }
                        dock.Controls.Add(icon);
                    }
                }
            }
            return img_count;
        }
    }
}