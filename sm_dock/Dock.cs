using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace sm_dock
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed class Dock : Form
    {
        //Avoid focus stealing
        protected override bool ShowWithoutActivation => true;

        private Timer tmr;
        private Rectangle autoh_rect;

        public Dock()
        {
            try
            {
                int img_count = 0;
                img_count = GlobalHandler.LoadIcons(this);
                if(img_count == 0)
                {
                    GlobalHandler.ErrorMsg("No icons are added to the dock. Exiting.");
                    Environment.Exit(0);
                }
                SetUpDock(img_count);
            }
            catch (Exception ex)
            {
                GlobalHandler.ErrorMsg("Error while initializing:\n" + ex.Message + "\nPlease generate a new icon cache!");
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
            int size = GlobalHandler.icon_pad + img_count * (GlobalHandler.icon_size + GlobalHandler.icon_pad);
            int bottom;
            int frm_size = GlobalHandler.icon_size + GlobalHandler.icon_line;
            if (GlobalHandler.dock_pos < 2)
            {
                sc_size = Screen.PrimaryScreen.Bounds.Width;
                bottom = GlobalHandler.dock_pos == 0 ? 0 : Screen.PrimaryScreen.Bounds.Height - frm_size;
                SetBounds(
                    sc_size / 2 - size / 2 + GlobalHandler.dock_offs,
                    bottom,
                    size,
                    frm_size);
                autoh_rect = Bounds;
                if (GlobalHandler.dock_pos == 1) autoh_rect.Y = autoh_rect.Bottom;
                autoh_rect.Height = 2;
            }
            else
            {
                sc_size = Screen.PrimaryScreen.Bounds.Height;
                bottom = GlobalHandler.dock_pos == 2 ? 0 : Screen.PrimaryScreen.Bounds.Width - frm_size;
                SetBounds(
                    bottom,
                    sc_size / 2 - size / 2 + GlobalHandler.dock_offs,
                    frm_size,
                    size);
                autoh_rect = Bounds;
                if (GlobalHandler.dock_pos == 3) autoh_rect.X = autoh_rect.Right;
                autoh_rect.Width = 2;
            }
            ShowInTaskbar = false;
            BackColor = GlobalHandler.icon_col[0].Color;
            TopMost = GlobalHandler.dock_top;
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
                tmr.Tick += Tmr_Tick;
            }
        }

        private void Tmr_Tick(object sender, EventArgs e)
        {
            bool click = !GlobalHandler.dock_autoh_cl || MouseButtons.HasFlag(MouseButtons.Left);
            Point pt = MousePosition;
            if (Visible)
            {
                if (!Bounds.Contains(pt))
                {
                    Visible = false;
                }
            }
            else
            {
                if (autoh_rect.Contains(pt) && click)
                {
                    Visible = true;
                }
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams par = base.CreateParams;
                par.ExStyle |= 0x80;
                return par;
            }
        }
    }
}