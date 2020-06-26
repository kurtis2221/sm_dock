using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace sm_dock
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed class IconControl : Control
    {
        private string pr_name;
        private string pr_file;
        private string pr_args;
        private string pr_work;
        private ProcessWindowStyle pr_window;
        private Bitmap bg_img;
        private int state;
        private static DockLabel tt = new DockLabel();

        public IconControl(string pr_name, string pr_file, string pr_args, string pr_work,
            ProcessWindowStyle pr_window, Bitmap img) : base()
        {
            this.pr_name = pr_name;
            this.pr_file = pr_file;
            this.pr_args = pr_args;
            this.pr_work = pr_work;
            this.pr_window = pr_window;
            bg_img = img;
            state = GlobalHandler.IC_STATE_NORMAL;
            AllowDrop = true;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (GlobalHandler.dock_nam)
            {
                tt.Visible = true;
                tt.SetText(this, pr_name);
            }
            state = GlobalHandler.IC_STATE_HOVER;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (GlobalHandler.dock_nam) tt.Visible = false;
            state = GlobalHandler.IC_STATE_NORMAL;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            state = GlobalHandler.IC_STATE_CLICK;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            state = GlobalHandler.IC_STATE_NORMAL;
            Invalidate();
            StartProgram(string.Empty);
            base.OnMouseClick(e);
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
            base.OnDragEnter(e);
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            string[] args = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            StartProgram(string.Join(" ", args));
            base.OnDragDrop(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = GlobalHandler.icon_sm;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            int offsx = GlobalHandler.dock_pos == 3 ? GlobalHandler.icon_line : 0;
            int offsy = GlobalHandler.dock_pos == 1 ? GlobalHandler.icon_line : 0;
            g.DrawImage(bg_img, offsx, offsy, GlobalHandler.icon_size, GlobalHandler.icon_size);
            if (GlobalHandler.dock_pos < 2)
            {
                g.FillRectangle(
                    GlobalHandler.icon_col[state],
                    0,
                    GlobalHandler.dock_pos == 0 ? GlobalHandler.icon_size : 0,
                    GlobalHandler.icon_size,
                    GlobalHandler.icon_line);
            }
            else
            {
                g.FillRectangle(
                    GlobalHandler.icon_col[state],
                    GlobalHandler.dock_pos == 2 ? GlobalHandler.icon_size : 0,
                    0,
                    GlobalHandler.icon_line,
                    GlobalHandler.icon_size);
            }
            base.OnPaint(e);
        }

        private void StartProgram(string args)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = pr_file;
            psi.Arguments = pr_args + " " + args;
            psi.WindowStyle = pr_window;
            if (pr_work.Length > 0) psi.WorkingDirectory = pr_work;
            psi.UseShellExecute = true;
            Process.Start(psi);
        }
    }
}