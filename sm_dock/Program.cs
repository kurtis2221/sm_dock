using System;
using System.IO;
using System.Windows.Forms;

namespace sm_dock
{
    internal static class Program
    {
        [STAThread]
        static private void Main()
        {
            if (!File.Exists(GlobalHandler.CFG_FILE))
            {
                GlobalHandler.ErrorMsg(GlobalHandler.CACHE_FILE +
                    " file not found!\nCreate a configuration before running!");
                return;
            }
            if (!File.Exists(GlobalHandler.CACHE_FILE))
            {
                GlobalHandler.ErrorMsg(GlobalHandler.CACHE_FILE +
                    " file not found!\nGenerate an icon cache before running!");
                return;
            }
            Application.Run(new Dock());
        }
    }
}