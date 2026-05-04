using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace QUIZTEAM
{
    public partial class Form1 : Form
    {
        private Image _imgBienvenida;
        private Rectangle _zonaComenzon;
        private Rectangle _zonaSalir;
        private Rectangle _rectImagen;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(26, 26, 46);
            this.KeyPreview = true;

            RecalcularZonas();
        }

        private void RecalcularZonas()
        {
            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height;
            int centroX = W / 2;

            _rectImagen = new Rectangle(centroX - W / 3, (int)(H * 0.08), W * 2 / 3, (int)(H * 0.45));
            _zonaComenzon = new Rectangle(centroX - 230, (int)(H * 0.65), 460, 52);
            _zonaSalir = new Rectangle(centroX - 110, (int)(H * 0.75), 220, 40);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecalcularZonas();
            this.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string ruta = Path.Combine(Application.StartupPath, "Imagenes", "bienvenida.png");
            if (File.Exists(ruta))
                _imgBienvenida = Image.FromFile(ruta);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height;

            g.Clear(Color.FromArgb(26, 26, 46));

            // Imagen de bienvenida
            DrawRoundRect(g, _rectImagen, 12, Color.FromArgb(15, 33, 62), Color.FromArgb(233, 69, 96));
            if (_imgBienvenida != null)
                g.DrawImage(_imgBienvenida, _rectImagen);

            // Título
            using (Font fTitulo = new Font("Georgia", 26, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(234, 234, 234)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("BIENVENIDO A NUESTRO QUIZ", fTitulo, br,
                    new RectangleF(0, H * 0.57f, W, 50), sf);
            }

            // Línea decorativa
            using (Pen p = new Pen(Color.FromArgb(233, 69, 96), 1.5f))
                g.DrawLine(p, W * 0.25f, H * 0.63f, W * 0.75f, H * 0.63f);

            // Botón Comenzar
            DrawBoton(g, _zonaComenzon, "CLICK AQUÍ PARA COMENZAR",
                Color.FromArgb(15, 52, 96), Color.FromArgb(233, 69, 96),
                Color.FromArgb(234, 234, 234));

            // Botón Salir
            DrawBoton(g, _zonaSalir, "SALIR",
                Color.Transparent, Color.FromArgb(85, 85, 85),
                Color.FromArgb(136, 146, 164));
        }

        private void DrawBoton(Graphics g, Rectangle r, string texto,
            Color fill, Color borde, Color colorTexto)
        {
            DrawRoundRect(g, r, 22, fill, borde);
            using (Font f = new Font("Georgia", 13, FontStyle.Bold))
            using (SolidBrush brT = new SolidBrush(colorTexto))
            {
                var sf = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(texto, f, brT, r, sf);
            }
        }

        private void DrawRoundRect(Graphics g, Rectangle r, int radio, Color fill, Color borde)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            GraphicsPath path = new GraphicsPath();
            int d = radio * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseAllFigures();

            if (fill != Color.Transparent)
                using (SolidBrush br = new SolidBrush(fill)) g.FillPath(br, path);
            using (Pen p = new Pen(borde, 1.5f)) g.DrawPath(p, path);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_zonaComenzon.Contains(e.Location))
            {
                var cats = new Categorias();
                cats.FormClosed += (s, args) => this.Show();
                cats.Show();
                this.Hide();
            }
            else if (_zonaSalir.Contains(e.Location))
            {
                Application.Exit();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.Cursor = (_zonaComenzon.Contains(e.Location) || _zonaSalir.Contains(e.Location))
                ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape) Application.Exit();
        }

        private void ImagBienvenida_Click(object sender, EventArgs e) { }
    }
}