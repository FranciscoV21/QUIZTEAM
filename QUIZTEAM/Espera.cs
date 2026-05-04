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
                // Limpiamos cualquier rastro de conexión previa antes de conectar
                if (_ws != null) _ws.Dispose();

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

                // El servidor nos dirá cuántos hay realmente en el primer 'sala_update'
                _jugadoresEnSala = 1;

                _ = Task.Run(EscucharWs);
                ActualizarUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
                this.Close();
            }
        }

        private async Task EscucharWs()
        {
            var buffer = new byte[8192]; // Buffer más grande para evitar cortes
            while (_ws != null && _ws.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close) break;

                    string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var msg = JsonSerializer.Deserialize<JsonElement>(json);

                    if (msg.TryGetProperty("type", out JsonElement typeProp))
                    {
                        string type = typeProp.GetString();

                        if (type == "sala_update")
                        {
                            // Actualizamos el conteo real que manda el servidor
                            _jugadoresEnSala = msg.GetProperty("jugadores").GetInt32();
                            ActualizarUI();
                        }
                        else if (type == "start_game")
                        {
                            if (msg.TryGetProperty("categoria", out JsonElement catProp))
                                _categoria = catProp.GetString();

                            this.Invoke((Action)(() => IniciarJuegoLocal()));
                        }
                    }
                }
                catch { break; }
            }
        }

        private void IniciarJuegoLocal()
        {
            if (_navegando) return;
            _navegando = true;
            _timer.Stop();

            // Pasamos la conexión activa al juego
            var juego = new Juego(_categoria, _ws, _playerId, _playerNombre, _esLider, _soloModo);
            juego.Show();
            this.Hide();
        }

        private void IniciarJuego()
        {
            if (_navegando) return;
            _ = EnviarWs(new { type = "start_game", categoria = _categoria });
        }

        private async Task EnviarWs(object obj)
        {
            if (_ws == null || _ws.State != WebSocketState.Open) return;
            try
            {
                string json = JsonSerializer.Serialize(obj);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height;

            // Dibujar fondo y textos
            g.Clear(Color.FromArgb(26, 26, 46));

            using (Font f = new Font("Georgia", 26, FontStyle.Bold))
                DibujarTextoCentrado(g, "SALA DE ESPERA", f, Brushes.White, H * 0.1f);

            using (Font f = new Font("Consolas", 14))
                DibujarTextoCentrado(g, $"Categoría: {_categoria}", f, Brushes.Crimson, H * 0.2f);

            // Iconos de jugadores
            DibujarIconosJugadores(g, W, H);

            // Botón principal
            _zonaSolo = new Rectangle((W - 300) / 2, (int)(H * 0.8f), 300, 50);
            Color cBtn = _esLider ? Color.FromArgb(233, 69, 96) : Color.FromArgb(45, 45, 70);
            Juego.DrawRoundRect(g, _zonaSolo, 20, cBtn, Color.White);

            string txtBtn = _esLider ? (_jugadoresEnSala > 1 ? "INICIAR PARTIDA" : "ESPERANDO RIVAL...") : "EL LÍDER INICIARÁ...";
            using (Font f = new Font("Georgia", 12, FontStyle.Bold))
                DibujarTextoEnRect(g, txtBtn, f, Brushes.White, _zonaSolo);

            // Botón Salir
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

        // Helpers de dibujo
        private void DibujarTextoCentrado(Graphics g, string t, Font f, Brush b, float y)
        {
            SizeF s = g.MeasureString(t, f);
            g.DrawString(t, f, b, (this.ClientSize.Width - s.Width) / 2, y);
        }

        private void DibujarTextoEnRect(Graphics g, string t, Font f, Brush b, Rectangle r)
        {
            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
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
            _ = CerrarWs();
            new Categorias().Show();
            this.Hide();
        }

        private async Task CerrarWs()
        {
            try { if (_ws?.State == WebSocketState.Open) await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "User exit", CancellationToken.None); } catch { }
        }

        private void ActualizarUI() { if (!this.IsDisposed) this.Invoke((Action)(() => this.Invalidate())); }
    }
}