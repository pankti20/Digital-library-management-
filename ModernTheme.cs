using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DigitalLibrary
{
    public class ModernButton : Button
    {
        private int borderRadius = 5;
        public int BorderRadius
        {
            get => borderRadius;
            set { borderRadius = value; Invalidate(); }
        }

        public ModernButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = UITheme.PrimaryColor;
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.Cursor = Cursors.Hand;
            this.Resize += (s, e) => Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rectSurface = this.ClientRectangle;
            Rectangle rectBorder = Rectangle.Inflate(rectSurface, -1, -1);
            int smoothSize = 2;

            if (borderRadius > 2)
            {
                using (GraphicsPath pathSurface = GetFigurePath(rectSurface, borderRadius))
                using (GraphicsPath pathBorder = GetFigurePath(rectBorder, borderRadius - 1))
                using (Pen penSurface = new Pen(this.Parent?.BackColor ?? Color.White, smoothSize))
                {
                    this.Region = new Region(pathSurface);
                    pevent.Graphics.DrawPath(penSurface, pathSurface);
                }
            }
            else
            {
                this.Region = new Region(rectSurface);
            }

            TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font, rectSurface, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private GraphicsPath GetFigurePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float curveSize = radius * 2F;

            path.StartFigure();
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public static class UITheme
    {
        public static Color PrimaryColor = Color.FromArgb(41, 128, 185);
        public static Color SecondaryColor = Color.FromArgb(52, 152, 219);
        public static Color SuccessColor = Color.FromArgb(39, 174, 96);
        public static Color DangerColor = Color.FromArgb(231, 76, 60);
        public static Color BackgroundColor = Color.FromArgb(245, 247, 250);
        public static Color FormBackColor = Color.White;
        public static Color TextColor = Color.FromArgb(44, 62, 80);
        public static Font MainFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static Font TitleFont = new Font("Segoe UI", 16F, FontStyle.Bold);

        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = FormBackColor;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.DefaultCellStyle.SelectionBackColor = SecondaryColor;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.BackColor = FormBackColor;
            dgv.DefaultCellStyle.ForeColor = TextColor;
            dgv.DefaultCellStyle.Font = MainFont;
            dgv.DefaultCellStyle.Padding = new Padding(5);
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowTemplate.Height = 40;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = PrimaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgv.ColumnHeadersHeight = 45;
            
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(235, 245, 251);
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
        }

        public static void StyleForm(Form form)
        {
            form.BackColor = BackgroundColor;
            form.Font = MainFont;
            form.ForeColor = TextColor;
        }
    }
}
