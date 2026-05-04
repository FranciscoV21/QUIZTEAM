using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QUIZTEAM
{
    public class Espera : Form
    {
        private string _categoria;
        private ClientWebSocket _ws;
        private string _playerId = "";
        private string _playerNombre = "";
        private bool _esLider = false;
        private int _jugadoresEnSala = 0;
        private bool _soloModo = false;
        private bool _navegando = false;

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

            // Timer para redibujar el contador
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
                _ws = await Config.NuevoWebSocket();
                string url = $"{Config.WsUrl}/ws/{Config.RoomId}";
                await _ws.ConnectAsync(new Uri(url), CancellationToken.None);

                var buffer = new byte[4096];
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var msg = JsonSerializer.Deserialize<JsonElement>(json);

                _playerId = msg.GetProperty("player_id").GetString();
                _playerNombre = msg.GetProperty("nombre").GetString();
                _esLider = msg.GetProperty("es_lider").GetBoolean();
                _jugadoresEnSala = 1;

                _ = Task.Run(EscucharWs);
                this.Invoke((Action)(() => this.Invalidate()));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error conectando: " + ex.Message);
                this.Close();
            }
        }

        private async Task EscucharWs()
        {
            var buffer = new byte[4096];
            while (_ws != null && _ws.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var msg = JsonSerializer.Deserialize<JsonElement>(json);
                    string type = msg.GetProperty("type").GetString();

                    if (type == "sala_update")
                    {
                        _jugadoresEnSala = msg.GetProperty("jugadores").GetInt32();
                        if (!this.IsDisposed)
                            this.Invoke((Action)(() => this.Invalidate()));
                    }
                }
                catch { break; }
            }
        }

        private async Task EnviarWs(object obj)
        {
            try
            {
                string json = JsonSerializer.Serialize(obj);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch { }
        }

        private void IniciarJuego()
        {
            if (_navegando) return;
            _navegando = true;
            _timer.Stop();

            // Enviar categoría al servidor
            _ = EnviarWs(new { type = "set_category", categoria = _categoria });

            // Abrir juego pasando el WS ya conectado
            var juego = new Juego(_categoria, _ws, _playerId, _playerNombre, _esLider, _soloModo);
            juego.Show();
            this.Hide();
            this.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.FromArgb(26, 26, 46));

            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height;
            int midX = W / 2;

            // Título
            using (Font f = new Font("Georgia", 28, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
                g.DrawString("SALA DE ESPERA", f, br,
                    new RectangleF(0, H * 0.08f, W, 50),
                    new StringFormat { Alignment = StringAlignment.Center });

            // Categoría
            using (Font f = new Font("Consolas", 14))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(233, 69, 96)))
                g.DrawString($"▶ Categoría: {_categoria}", f, br,
                    new RectangleF(0, H * 0.17f, W, 30),
                    new StringFormat { Alignment = StringAlignment.Center });

            // Jugadores conectados
            DibujarJugadores(g, W, H, midX);

            // Mensaje de espera o listo
            string msg = _jugadoresEnSala >= 2
                ? $"¡{_jugadoresEnSala} jugadores listos!  El líder puede iniciar."
                : "Esperando más jugadores...";

            Color msgColor = _jugadoresEnSala >= 2
                ? Color.FromArgb(39, 174, 96)
                : Color.FromArgb(136, 146, 164);

            using (Font f = new Font("Georgia", 13, FontStyle.Italic))
            using (SolidBrush br = new SolidBrush(msgColor))
                g.DrawString(msg, f, br,
                    new RectangleF(0, H * 0.72f, W, 30),
                    new StringFormat { Alignment = StringAlignment.Center });

            // Botón INICIAR SOLO (siempre visible)
            _zonaSolo = new Rectangle(midX - 160, (int)(H * 0.80f), 320, 50);
            Color colorSolo = _soloModo
                ? Color.FromArgb(233, 69, 96)
                : Color.FromArgb(50, 50, 80);
            Juego.DrawRoundRect(g, _zonaSolo, 20, colorSolo, Color.FromArgb(233, 69, 96));

            string textoSolo = _esLider
                ? (_jugadoresEnSala >= 2 ? "INICIAR PARTIDA  ▶" : "JUGAR SOLO  [ENTER]")
                : "Esperando al líder...";

            using (Font f = new Font("Georgia", 13, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
                g.DrawString(textoSolo, f, br, _zonaSolo,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            // Advertencia modo solo
            if (_esLider && _jugadoresEnSala < 2)
            {
                using (Font f = new Font("Consolas", 10))
                using (SolidBrush br = new SolidBrush(Color.FromArgb(200, 150, 50)))
                    g.DrawString("⚠  Estás jugando solo — el podio solo te mostrará a ti",
                        f, br,
                        new RectangleF(0, (int)(H * 0.88f), W, 24),
                        new StringFormat { Alignment = StringAlignment.Center });
            }

            // Botón salir
            _zonaSalir = new Rectangle(30, H - 52, 120, 38);
            Juego.DrawRoundRect(g, _zonaSalir, 17, Color.Transparent, Color.FromArgb(85, 85, 85));
            using (Font f = new Font("Georgia", 10))
            using (SolidBrush br = new SolidBrush(Color.Gray))
                g.DrawString("ESC - SALIR", f, br, _zonaSalir,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            // Nombre del jugador
            using (Font f = new Font("Consolas", 9))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(100, 100, 130)))
                g.DrawString($"Conectado como: {_playerNombre}{(_esLider ? " 👑" : "")}",
                    f, br, 30, 20);
        }

        private void DibujarJugadores(Graphics g, int W, int H, int midX)
        {
            int total = Math.Max(_jugadoresEnSala, 1);
            int iconSize = 70;
            int gap = 40;
            int startX = midX - (total * (iconSize + gap)) / 2;
            int iconY = (int)(H * 0.35f);

            for (int i = 0; i < total; i++)
            {
                int x = startX + i * (iconSize + gap);
                Color color = i < _jugadoresEnSala
                    ? Color.FromArgb(39, 174, 96)
                    : Color.FromArgb(60, 60, 80);

                // Círculo jugador
                using (SolidBrush br = new SolidBrush(color))
                    g.FillEllipse(br, x, iconY, iconSize, iconSize);

                // Número
                using (Font f = new Font("Impact", 22))
                using (SolidBrush br = new SolidBrush(Color.White))
                    g.DrawString($"{i + 1}", f, br,
                        new RectangleF(x, iconY, iconSize, iconSize),
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                // Etiqueta
                string label = i == 0 && _jugadoresEnSala >= 1 ? _playerNombre : $"Jugador {i + 1}";
                using (Font f = new Font("Consolas", 9))
                using (SolidBrush br = new SolidBrush(Color.Silver))
                    g.DrawString(label, f, br,
                        new RectangleF(x - 10, iconY + iconSize + 8, iconSize + 20, 20),
                        new StringFormat { Alignment = StringAlignment.Center });
            }

            // Contador grande
            using (Font f = new Font("Impact", 48))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(233, 69, 96)))
                g.DrawString($"{_jugadoresEnSala}", f, br,
                    new RectangleF(0, H * 0.55f, W, 60),
                    new StringFormat { Alignment = StringAlignment.Center });

            using (Font f = new Font("Georgia", 12))
            using (SolidBrush br = new SolidBrush(Color.Gray))
                g.DrawString("jugadores conectados", f, br,
                    new RectangleF(0, H * 0.63f, W, 24),
                    new StringFormat { Alignment = StringAlignment.Center });
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (_zonaSalir.Contains(e.Location))
            {
                _timer.Stop();
                _ = CerrarWs();
                var cats = new Categorias();
                cats.Show();
                this.Hide();
                this.Dispose();
                return;
            }

            if (_zonaSolo.Contains(e.Location) && _esLider && !_navegando)
                IniciarJuego();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter && _esLider && !_navegando)
                IniciarJuego();
            if (e.KeyCode == Keys.Escape)
            {
                _timer.Stop();
                _ = CerrarWs();
                var cats = new Categorias();
                cats.Show();
                this.Hide();
                this.Dispose();
            }
        }

        private async Task CerrarWs()
        {
            try
            {
                if (_ws != null && _ws.State == WebSocketState.Open)
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            catch { }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer?.Stop();
            base.OnFormClosed(e);
        }
    }
}