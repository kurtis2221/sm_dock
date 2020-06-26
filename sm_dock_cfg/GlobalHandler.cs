using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace sm_dock
{
    internal static partial class GlobalHandler
    {
        [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

        public static List<IconData> icon_list = new List<IconData>();

        public static void CreateCache()
        {
            List<int> img_lens = new List<int>();
            int start;
            int offset;
            using (BinaryWriter bw = new BinaryWriter(new FileStream(CACHE_FILE, FileMode.Create, FileAccess.Write), DEF_ENC))
            {
                using (StreamReader sr = new StreamReader(CFG_FILE))
                {
                    //Store icons in a temporary file
                    using (BinaryWriter tmp_bw = new BinaryWriter(new FileStream(TMP_CACHE_FILE, FileMode.Create, FileAccess.Write), DEF_ENC))
                    {
                        sr.ReadLine();
                        while (sr.Peek() > -1)
                        {
                            start = (int)tmp_bw.BaseStream.Position;
                            string[] icon_data = sr.ReadLine().Split(CFG_DELIM);
                            //If invalid, skip
                            if (icon_data.Length < ICON_DATA_LEN) continue;
                            //Saving as PNG
                            int icon_nm = Convert.ToInt32(icon_data[ICON_ICONNUM]);
                            string icon_file = icon_data[ICON_ICONFILE];
                            string ext = Path.GetExtension(icon_file).ToLower();
                            Bitmap bmp;
                            if (ext == ".ico" || ext == ".exe" || ext == ".dll")
                            {
                                bmp = ExtractIcon(icon_file, icon_nm).ToBitmap();
                            }
                            else
                            {
                                bmp = new Bitmap(icon_file);
                            }
                            bmp.Save(tmp_bw.BaseStream, ImageFormat.Png);
                            offset = (int)tmp_bw.BaseStream.Position - start;
                            img_lens.Add(offset);
                        }
                    }
                    //Add 1 more image address to skip file length check
                    bw.Write(img_lens.Count + 1);
                    //Write file pointers, taking the dictionary size into account
                    offset = 4 + 4 + img_lens.Count * 4;
                    for (int i = 0; i < img_lens.Count; i++)
                    {
                        bw.Write(offset);
                        offset += img_lens[i];
                    }
                    bw.Write(offset);
                    //Merge the 2 files
                    using (FileStream fs = new FileStream(TMP_CACHE_FILE, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[BUFFER_SIZE];
                        int len;
                        while (true)
                        {
                            len = fs.Read(buffer, 0, BUFFER_SIZE);
                            if (len <= 0) break;
                            bw.Write(buffer, 0, len);
                        }
                    }
                    //Delete temporary file
                    File.Delete(TMP_CACHE_FILE);
                }
            }
        }

        public static void LoadConfig()
        {
            using (StreamReader sr = new StreamReader(CFG_FILE, DEF_ENC))
            {
                LoadData(sr);
                string[] cols;
                while (sr.Peek() > -1)
                {
                    cols = sr.ReadLine().Split(CFG_DELIM);
                    if (cols.Length < ICON_DATA_LEN) continue;
                    IconData ic = new IconData();
                    ic.text = cols[ICON_TEXT];
                    ic.filename = cols[ICON_FILENAME];
                    ic.icon = cols[ICON_ICONFILE];
                    ic.icon_nm = Convert.ToInt32(cols[ICON_ICONNUM]);
                    ic.startarg = cols[ICON_STARTARG];
                    ic.workdir = cols[ICON_WORKDIR];
                    ic.window = ParseInt(cols[ICON_WINDOW]);
                    icon_list.Add(ic);
                }
            }
        }

        public static void SaveConfig()
        {
            using (StreamWriter sw = new StreamWriter(CFG_FILE, false, DEF_ENC))
            {
                sw.WriteLine(icon_col[IC_STATE_NORMAL].Color.ToArgb().ToString("X") + CFG_DELIM +
                    icon_col[IC_STATE_HOVER].Color.ToArgb().ToString("X") + CFG_DELIM +
                    icon_col[IC_STATE_CLICK].Color.ToArgb().ToString("X") + CFG_DELIM +
                    txt_fg.ToArgb().ToString("X") + CFG_DELIM +
                    txt_bg.ToArgb().ToString("X") + CFG_DELIM +
                    txt_fnt.Name + CFG_DELIM +
                    txt_fnt.Size + CFG_DELIM +
                    (int)txt_fnt.Style + CFG_DELIM +
                    icon_size + CFG_DELIM +
                    icon_pad + CFG_DELIM +
                    icon_line + CFG_DELIM +
                    icon_sm_idx + CFG_DELIM +
                    dock_pos + CFG_DELIM +
                    dock_offs + CFG_DELIM +
                    BoolToStr(dock_nam) + CFG_DELIM +
                    BoolToStr(dock_top) + CFG_DELIM +
                    BoolToStr(dock_autoh) + CFG_DELIM +
                    BoolToStr(dock_autoh_cl) + CFG_DELIM +
                    dock_autoh_sn + CFG_DELIM +
                    dock_autoh_iv);
                //Icons
                foreach (IconData ic in icon_list)
                {
                    sw.WriteLine(ic.text + CFG_DELIM +
                        ic.filename + CFG_DELIM +
                        ic.icon + CFG_DELIM +
                        ic.icon_nm + CFG_DELIM +
                        ic.startarg + CFG_DELIM +
                        ic.workdir + CFG_DELIM +
                        ic.window);
                }
            }
        }

        private static Icon ExtractIcon(string file, int number)
        {
            IntPtr large;
            IntPtr small;
            ExtractIconEx(file, number, out large, out small, 1);
            try
            {
                return Icon.FromHandle(large);
            }
            catch
            {
                return null;
            }
        }
    }
}