using System;
using System.Drawing;
using System.Windows.Forms;

namespace sm_dock
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed class DockLabel : Form
    {
        //Avoid focus stealing
        protected override bool ShowWithoutActivation => true;

        private Label lb;

        public DockLabel()
        {
            SuspendLayout();
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ForeColor = GlobalHandler.txt_fg;
            BackColor = GlobalHandler.txt_bg;
            lb = new Label();
            lb.Font = GlobalHandler.txt_fnt;
            lb.TextAlign = ContentAlignment.MiddleCenter;
            lb.AutoSize = true;
            SetAutoSizeMode(AutoSizeMode.GrowAndShrink);
            Controls.Add(lb);
            AutoSize = true;
            TopMost = GlobalHandler.dock_top;
            ResumeLayout();
            ShowInTaskbar = false;
        }

        public void SetText(Control ct, string input)
        {
            lb.Text = input;
            Point pt = ct.PointToScreen(Point.Empty);
            int x, y;
            if (GlobalHandler.dock_pos < 2)
            {
                x = pt.X + GlobalHandler.icon_hsize - Width / 2;
                if (GlobalHandler.dock_pos == 0) y = GlobalHandler.bottom;
                else y = GlobalHandler.top - Height;
            }
            else
            {
                if (GlobalHandler.dock_pos == 2) x = GlobalHandler.right;
                else x = GlobalHandler.left - Width;
                y = pt.Y + GlobalHandler.icon_hsize - Height / 2;
            }
            SetDesktopLocation(x, y);
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

        protected override void OnClosed(EventArgs e)
        {
            Application.Exit();
            base.OnClosed(e);
        }
    }
}