using System;
using System.Drawing;
using System.Windows.Forms;

namespace sm_dock
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed class Dock : Form
    {
        //Avoid focus stealing
        protected override bool ShowWithoutActivation => true;

        //Load error levels
        private static string[] err_lvl =
        {
            "Please reconfigure the Dock!",
            "Please generate a new icon cache!"
        };

        //Autohide timer
        private Timer tmr;
        //Autohide delay timer
        private Timer vis_tmr;
        //Autohide trigger rectangle
        private Rectangle autoh_rect;

        public Dock()
        {
            try
            {
                int img_count = 0;
                img_count = GlobalHandler.LoadIcons(this);
                if (img_count == 0)
                {
                    GlobalHandler.ErrorMsg("No icons are added to the dock. Exiting.");
                    Environment.Exit(0);
                }
                SetUpDock(img_count);
            }
            catch (Exception ex)
            {
                GlobalHandler.ErrorMsg("Error while initializing:\n" + ex.Message +
                    "\n" + err_lvl[GlobalHandler.load_lvl]);
                Environment.Exit(0);
            }
        }

        private void SetUpDock(int img_count)
        {
            SuspendLayout();
            Text = GlobalHandler.PROG_NAME;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            int sc_size;
            int size = img_count * GlobalHandler.icon_nsize;
            int bottom;
            int frm_size = GlobalHandler.icon_nsize + GlobalHandler.icon_line;
            Screen curr_sc = Screen.PrimaryScreen;
            if (GlobalHandler.dock_pos < 2)
            {
                sc_size = curr_sc.Bounds.Width;
                bottom = GlobalHandler.dock_pos == 0 ? 0 : curr_sc.Bounds.Height - frm_size;
                SetBounds(
                    curr_sc.Bounds.X + sc_size / 2 - size / 2 + GlobalHandler.dock_offs,
                    curr_sc.Bounds.Y + bottom,
                    size,
                    frm_size);
                autoh_rect = Bounds;
                if (GlobalHandler.dock_pos == 1) autoh_rect.Y = autoh_rect.Bottom - GlobalHandler.dock_autoh_sn;
                autoh_rect.Height = GlobalHandler.dock_autoh_sn;
            }
            else
            {
                sc_size = curr_sc.Bounds.Height;
                bottom = GlobalHandler.dock_pos == 2 ? 0 : curr_sc.Bounds.Width - frm_size;
                SetBounds(
                    curr_sc.Bounds.X + bottom,
                    curr_sc.Bounds.Y + sc_size / 2 - size / 2 + GlobalHandler.dock_offs,
                    frm_size,
                    size);
                autoh_rect = Bounds;
                if (GlobalHandler.dock_pos == 3) autoh_rect.X = autoh_rect.Right - GlobalHandler.dock_autoh_sn;
                autoh_rect.Width = GlobalHandler.dock_autoh_sn;
            }
            ShowInTaskbar = false;
            BackColor = GlobalHandler.icon_col[GlobalHandler.IC_STATE_NORMAL].Color;
            ResumeLayout();
            //Helpers
            GlobalHandler.width = Width;
            GlobalHandler.height = Height;
            GlobalHandler.left = Left;
            GlobalHandler.top = Top;
            GlobalHandler.right = Right;
            GlobalHandler.bottom = Bottom;
            //Autohide timer
            if (GlobalHandler.dock_autoh)
            {
                tmr = new Timer();
                tmr.Interval = GlobalHandler.dock_autoh_iv;
                tmr.Enabled = true;
                tmr.Tick += tmr_Tick;
                vis_tmr = new Timer();
                vis_tmr.Interval = GlobalHandler.dock_autoh_de;
                vis_tmr.Tick += vis_tmr_Tick;
            }
        }

        private void tmr_Tick(object sender, EventArgs e)
        {
            bool click = !GlobalHandler.dock_autoh_cl || MouseButtons.HasFlag(MouseButtons.Left);
            Point pt = MousePosition;
            if (Visible)
            {
                vis_tmr.Enabled = !Bounds.Contains(pt);
            }
            else
            {
                vis_tmr.Enabled = autoh_rect.Contains(pt) && click;
            }
        }

        private void vis_tmr_Tick(object sender, EventArgs e)
        {
            vis_tmr.Enabled = false;
            Visible = !Visible;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams par = base.CreateParams;
                par.ExStyle |= GlobalHandler.window_style;
                return par;
            }
        }
    }
}