using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Diagnostics;

namespace sm_dock
{
    internal static partial class GlobalHandler
    {
        //Delimiter
        const char CFG_DELIM = (char)1;

        //First line indexes
        //0 - Normal bgcolor
        //1 - Hover bgcolor
        //2 - Click bgcolor
        //3 - Text fgcolor
        //4 - Text bgcolor
        //5 - Font name
        //6 - Font size
        //7 - Font style
        //8 - Icon size
        //9 - Padding
        //10 - Line size (state)
        //11 - Smoothing quality
        //12 - Position
        //13 - Position offset
        //14 - Show names
        //15 - Topmost
        //16 - Autohide
        //17 - Autohide with mouse click
        //18 - Autohide sensitivity
        //19 - Autohide interval
        const int SET_DATA_LEN = 20;
        const int SET_N_BGCOLOR = 0;
        const int SET_H_BGCOLOR = 1;
        const int SET_C_BGCOLOR = 2;
        const int SET_TXT_FGCOLOR = 3;
        const int SET_TXT_BGCOLOR = 4;
        const int SET_TXT_FONT = 5;
        const int SET_TXT_FONT_SZ = 6;
        const int SET_TXT_FONT_ST = 7;
        const int SET_ICONSIZE = 8;
        const int SET_PADDING = 9;
        const int SET_LINESIZE = 10;
        const int SET_SMOOTH = 11;
        const int SET_POSITION = 12;
        const int SET_POS_OFFS = 13;
        const int SET_NAMES = 14;
        const int SET_TOP = 15;
        const int SET_AUTOH = 16;
        const int SET_AUTOH_CL = 17;
        const int SET_AUTOH_SN = 18;
        const int SET_AUTOH_IV = 19;

        //Icon data indexes
        //0 - Text
        //1 - File name
        //2 - Icon - File name
        //3 - Icon - Number
        //4 - Start arguments
        //5 - Working directory
        //6 - Start window style
        const int ICON_DATA_LEN = 7;
        const int ICON_TEXT = 0;
        const int ICON_FILENAME = 1;
        const int ICON_ICONFILE = 2;
        const int ICON_ICONNUM = 3;
        const int ICON_STARTARG = 4;
        const int ICON_WORKDIR = 5;
        const int ICON_WINDOW = 6;

        //Icon color states
        public const int IC_STATES = 3;
        public const int IC_STATE_NORMAL = 0;
        public const int IC_STATE_HOVER = 1;
        public const int IC_STATE_CLICK = 2;

        //For FileStream
        const int BUFFER_SIZE = 4096;

        //Program name
        public const string PROG_NAME = "Simple Dock";

        //Files
        public const string CFG_FILE = "sm_dock.ini";
        public const string CACHE_FILE = "icons.img";
        public const string TMP_CACHE_FILE = "icons.img.tmp";

        //Settings
        public static int icon_size;
        public static int icon_pad;
        public static int icon_line;
        public static InterpolationMode icon_sm;
        public static int icon_sm_idx;
        public static int dock_pos;
        public static int dock_offs;
        public static Color txt_fg;
        public static Color txt_bg;
        public static Font txt_fnt;
        public static SolidBrush[] icon_col = new SolidBrush[IC_STATES];
        public static bool dock_nam;
        public static bool dock_top;
        public static bool dock_autoh;
        public static bool dock_autoh_cl;
        public static int dock_autoh_sn;
        public static int dock_autoh_iv;
        public static int window_style;

        //Smoothing levels
        public static InterpolationMode[] icon_interp =
        {
            InterpolationMode.NearestNeighbor,
            InterpolationMode.Low,
            InterpolationMode.Bilinear,
            InterpolationMode.HighQualityBilinear
        };


        //Start window state
        public static ProcessWindowStyle[] win_style =
        {
            ProcessWindowStyle.Normal,
            ProcessWindowStyle.Maximized,
            ProcessWindowStyle.Minimized
        };

        //Encoding
        private static Encoding DEF_ENC = Encoding.Default;

        private static void LoadData(StreamReader sr)
        {
            string[] cols = sr.ReadLine().Split(CFG_DELIM);
            icon_col[IC_STATE_NORMAL] = new SolidBrush(Color.FromArgb(ParseInt(cols[SET_N_BGCOLOR], true)));
            icon_col[IC_STATE_HOVER] = new SolidBrush(Color.FromArgb(ParseInt(cols[SET_H_BGCOLOR], true)));
            icon_col[IC_STATE_CLICK] = new SolidBrush(Color.FromArgb(ParseInt(cols[SET_C_BGCOLOR], true)));
            txt_fg = Color.FromArgb(ParseInt(cols[SET_TXT_FGCOLOR], true));
            txt_bg = Color.FromArgb(ParseInt(cols[SET_TXT_BGCOLOR], true));
            txt_fnt = new Font(new FontFamily(cols[SET_TXT_FONT]),
                ParseFloat(cols[SET_TXT_FONT_SZ], 10),
                (FontStyle)ParseInt(cols[SET_TXT_FONT_ST]));
            icon_size = ParseInt(cols[SET_ICONSIZE]);
            icon_pad = ParseInt(cols[SET_PADDING]);
            icon_line = ParseInt(cols[SET_LINESIZE]);
            icon_sm_idx = ParseInt(cols[SET_SMOOTH]);
            icon_sm = icon_interp[icon_sm_idx];
            dock_pos = ParseInt(cols[SET_POSITION]);
            dock_offs = ParseInt(cols[SET_POS_OFFS]);
            dock_nam = ParseBool(cols[SET_NAMES]);
            dock_top = ParseBool(cols[SET_TOP]);
            window_style = 0x80;
            if (dock_top) window_style |= 0x8;
            dock_autoh = ParseBool(cols[SET_AUTOH]);
            dock_autoh_cl = ParseBool(cols[SET_AUTOH_CL]);
            dock_autoh_sn = ParseInt(cols[SET_AUTOH_SN]);
            dock_autoh_iv = ParseInt(cols[SET_AUTOH_IV]);
        }

        private static float ParseFloat(string input, float def)
        {
            float res;
            if (!float.TryParse(input, out res)) res = def;
            return res;
        }

        private static int ParseInt(string input, bool hex = false)
        {
            int res;
            if (hex) int.TryParse(input, NumberStyles.HexNumber, null, out res);
            else int.TryParse(input, out res);
            return res;
        }

        private static bool ParseBool(string input)
        {
            return input != "0";
        }

        private static string BoolToStr(bool input)
        {
            return input ? "1" : "0";
        }

        public static void ErrorMsg(string msg)
        {
            MessageBox.Show(msg, PROG_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}