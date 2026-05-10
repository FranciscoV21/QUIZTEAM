using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QUIZTEAM
{
    public class Espera : Form
    {
        private string _categoria;
        private ConexionServidor _conn;
        private string _playerId = "", _playerNombre = "";
        private bool _esLider = false, _navegando = false;
        private int _jugadoresEnSala = 0;

        private Rectangle _zonaSolo, _zonaSalir;
        private System.Windows.Forms.Timer _timer;

        public Espera(string categoria)
        {
            _categoria = categoria;
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(26, 26, 46);
            this.KeyPreview = true;

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 500;
            _timer.Tick += (s, e) => this.Invalidate();
            _timer.Start();

            this.Load += async (s, e) => await Conectar();
        }

        private async Task Conectar()
        {
            try
            {
                _conn = new ConexionServidor();
                _conn.OnMensaje += ProcesarMensaje;
                await _conn.ConectarAsync();

                await _conn.EnviarAsync(new
                {
                    type = "unirse",
                    room_id = Config.RoomId
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
                this.Close();
            }
        }

        private void ProcesarMensaje(JsonElement msg)
        {
            if (!msg.TryGetProperty("type", out var tp)) return;
            string type = tp.GetString();

            if (type == "connected")
            {
                _playerId = msg.GetProperty("player_id").GetString();
                _playerNombre = msg.GetProperty("nombre").GetString();
                _esLider = msg.GetProperty("es_lider").GetBoolean();
                ActualizarUI();
            }
            else if (type == "sala_update")
            {
                _jugadoresEnSala = msg.GetProperty("jugadores").GetInt32();
                ActualizarUI();
            }
            else if (type == "start_game")
            {
                if (msg.TryGetProperty("categoria", out var cp))
                    _categoria = cp.GetString();

                if (!this.IsDisposed)
                    this.Invoke((Action)IniciarJuegoLocal);
            }
        }

        private void IniciarJuegoLocal()
        {
            if (_navegando) return;
            _navegando = true;
            _timer.Stop();

            var juego = new Juego(_categoria, _conn, _playerId, _playerNombre, _esLider);
            juego.Show();
            this.Hide();
        }

        private void IniciarJuego()
        {
            if (_navegando) return;
            _ = _conn.EnviarAsync(new { type = "start_game", categoria = _categoria });
        }

        private void ActualizarUI()
        {
            if (!this.IsDisposed)
                this.Invoke((Action)(() => this.Invalidate()));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int W = this.ClientSize.Width, H = this.ClientSize.Height;
            g.Clear(Color.FromArgb(26, 26, 46));

            using (Font f = new Font("Georgia", 26, FontStyle.Bold))
                DibujarTextoCentrado(g, "SALA DE ESPERA", f, Brushes.White, H * 0.1f);

            using (Font f = new Font("Consolas", 14))
                DibujarTextoCentrado(g, $"Categoría: {_categoria}", f, Brushes.Crimson, H * 0.2f);

            DibujarIconosJugadores(g, W, H);

            _zonaSolo = new Rectangle((W - 300) / 2, (int)(H * 0.8f), 300, 50);
            Color cBtn = _esLider ? Color.FromArgb(233, 69, 96) : Color.FromArgb(45, 45, 70);
            Juego.DrawRoundRect(g, _zonaSolo, 20, cBtn, Color.White);

            string txtBtn = _esLider
                ? (_jugadoresEnSala > 1 ? "INICIAR PARTIDA" : "ESPERANDO RIVAL...")
                : "EL LÍDER INICIARÁ...";

            using (Font f = new Font("Georgia", 12, FontStyle.Bold))
                DibujarTextoEnRect(g, txtBtn, f, Brushes.White, _zonaSolo);

            _zonaSalir = new Rectangle(30, H - 60, 120, 40);
            Juego.DrawRoundRect(g, _zonaSalir, 15, Color.Transparent, Color.Gray);
            using (Font f = new Font("Segoe UI", 9))
                DibujarTextoEnRect(g, "ESC - SALIR", f, Brushes.Gray, _zonaSalir);
        }

        private void DibujarIconosJugadores(Graphics g, int W, int H)
        {
            int size = 80, gap = 30;
            int totalW = (_jugadoresEnSala * size) + ((_jugadoresEnSala - 1) * gap);
            int startX = (W - totalW) / 2;
            int y = H / 2 - 40;

            for (int i = 0; i < _jugadoresEnSala; i++)
            {
                Rectangle r = new Rectangle(startX + (i * (size + gap)), y, size, size);
                g.FillEllipse(new SolidBrush(Color.FromArgb(39, 174, 96)), r);
                g.DrawEllipse(new Pen(Color.White, 2), r);
                using (Font f = new Font("Impact", 20))
                    DibujarTextoEnRect(g, (i + 1).ToString(), f, Brushes.White, r);
            }

            using (Font f = new Font("Georgia", 14, FontStyle.Italic))
                DibujarTextoCentrado(g, $"{_jugadoresEnSala} jugadores en sala", f, Brushes.Silver, H * 0.65f);
        }

        private void DibujarTextoCentrado(Graphics g, string t, Font f, Brush b, float y)
        {
            SizeF s = g.MeasureString(t, f);
            g.DrawString(t, f, b, (this.ClientSize.Width - s.Width) / 2, y);
        }

        private void DibujarTextoEnRect(Graphics g, string t, Font f, Brush b, Rectangle r)
        {
            var sf = new StringFormat
            { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(t, f, b, r, sf);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (_zonaSalir.Contains(e.Location)) Regresar();
            if (_zonaSolo.Contains(e.Location) && _esLider) IniciarJuego();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Regresar();
            if (e.KeyCode == Keys.Enter && _esLider) IniciarJuego();
        }

        private void Regresar()
        {
            _timer.Stop();
            _conn?.Dispose();
            new Categorias().Show();
            this.Hide();
        }
    }
}