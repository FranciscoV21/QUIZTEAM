using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QUIZTEAM
{
    public partial class Juego : Form
    {
        // Variables de Estado y Datos
        private string _categoria;
        private List<Pregunta> _preguntas = new List<Pregunta>();
        private int _indiceActual = 0, _correctas = 0, _incorrectas = 0, _seleccion = -1;
        private bool _respondida = false;
        private bool _podioMostrado = false;
        private bool _esLider = false;
        private bool _soloModo = false;
        private int _jugadoresEnSala = 1;

        // UI y Gráficos
        private Rectangle[] _zonasOpciones = new Rectangle[4];
        private Rectangle _zonaSiguiente, _zonaSalir;
        private List<Image> _imagenesOpciones = new List<Image>();
        private static readonly HttpClient client = new HttpClient();

        // Comunicación (WebSockets y API)
        private ClientWebSocket _ws;
        private string _playerId = "";
        private string _playerNombre = "";
        private List<PlayerScore> _rankingActual = new List<PlayerScore>();
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public Juego(string categoria, ClientWebSocket ws, string playerId, string playerNombre, bool esLider, bool soloModo)
        {
            _categoria = categoria;
            _ws = ws;
            _playerId = playerId;
            _playerNombre = playerNombre;
            _esLider = esLider;
            _soloModo = soloModo;

            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(26, 26, 46);

            // Suscribir eventos básicos
            this.Load += async (s, e) => await InicializarJuego();
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }

        private async Task InicializarJuego()
        {
            // Escuchar mensajes del servidor en segundo plano
            _ = Task.Run(EscucharWebSocket);

            await CargarPreguntasAPI();
            CalcularZonas();
            CargarImagenesActual();
            this.Invalidate();
        }

        private async Task EscucharWebSocket()
        {
            var buffer = new byte[8192];
            while (_ws.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
            {
                try
                {
                    using (var ms = new MemoryStream())
                    {
                        WebSocketReceiveResult result;
                        do
                        {
                            result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            ms.Write(buffer, 0, result.Count);
                        } while (!result.EndOfMessage);

                        if (result.MessageType == WebSocketMessageType.Close) break;

                        string json = Encoding.UTF8.GetString(ms.ToArray());
                        var msg = JsonSerializer.Deserialize<JsonElement>(json);
                        string type = msg.GetProperty("type").GetString();

                        if (type == "sala_update")
                        {
                            _jugadoresEnSala = msg.GetProperty("jugadores").GetInt32();
                            ActualizarUI();
                        }
                        else if (type == "score" || type == "final")
                        {
                            var ranking = new List<PlayerScore>();
                            foreach (var item in msg.GetProperty("ranking").EnumerateArray())
                            {
                                ranking.Add(new PlayerScore
                                {
                                    nombre = item.GetProperty("nombre").GetString(),
                                    puntos = item.GetProperty("puntos").GetInt32()
                                });
                            }
                            _rankingActual = ranking;

                            if (type == "final")
                            {
                                _cts.Cancel();
                                MostrarPodio();
                            }
                            else
                            {
                                ActualizarUI();
                            }
                        }
                    }
                }
                catch { break; }
            }
        }

        private void MostrarPodio()
        {
            if (_podioMostrado) return;
            _podioMostrado = true;

            this.Invoke((Action)(() =>
            {
                if (this.IsDisposed) return;

                // Si por red no llegó el ranking, armamos uno local con tus puntos
                if (_rankingActual == null || _rankingActual.Count == 0)
                {
                    _rankingActual = new List<PlayerScore> {
                        new PlayerScore { nombre = _playerNombre, puntos = _correctas * 10 }
                    };
                }

                // PASO DIRECTO DE INFORMACIÓN LOCAL AL FORMULARIO DE RESULTADOS
                var formRes = new Resultado(_categoria, _correctas, _preguntas.Count, _rankingActual);
                formRes.Show();
                this.Hide();
            }));
        }

        private async Task CargarPreguntasAPI()
        {
            try
            {
                string url = $"{Config.ApiUrl}/preguntas?categoria={Uri.EscapeDataString(_categoria)}";
                string response = await client.GetStringAsync(url);
                _preguntas = JsonSerializer.Deserialize<List<Pregunta>>(response);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando preguntas: {ex.Message}");
                this.Close();
            }
        }

        private void CalcularZonas()
        {
            int W = this.ClientSize.Width, H = this.ClientSize.Height;
            if (W <= 0 || H <= 0) return;

            int ox = 40, gx = 28, gy = 20, headerH = 230, btnH = 60;
            int oh = (H - headerH - btnH - gy) / 2;
            int ow = (W - ox * 2 - gx) / 2;

            _zonasOpciones[0] = new Rectangle(ox, headerH, ow, oh);
            _zonasOpciones[1] = new Rectangle(ox + ow + gx, headerH, ow, oh);
            _zonasOpciones[2] = new Rectangle(ox, headerH + oh + gy, ow, oh);
            _zonasOpciones[3] = new Rectangle(ox + ow + gx, headerH + oh + gy, ow, oh);

            _zonaSiguiente = new Rectangle((W - 300) / 2, H - 70, 300, 50);
            _zonaSalir = new Rectangle(20, H - 50, 120, 35);
        }

        private void CargarImagenesActual()
        {
            foreach (var img in _imagenesOpciones) img?.Dispose();
            _imagenesOpciones.Clear();
            if (_indiceActual >= _preguntas.Count) return;

            var p = _preguntas[_indiceActual];
            if (p.tipo == "imagen" && p.imagenesOpciones != null)
            {
                foreach (var ruta in p.imagenesOpciones)
                {
                    if (string.IsNullOrEmpty(ruta)) { _imagenesOpciones.Add(null); continue; }
                    string path = Path.Combine(Application.StartupPath, "Imagenes", ruta);
                    try { _imagenesOpciones.Add(File.Exists(path) ? Image.FromFile(path) : null); }
                    catch { _imagenesOpciones.Add(null); }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_preguntas == null || _preguntas.Count == 0 || _indiceActual >= _preguntas.Count) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var p = _preguntas[_indiceActual];

            // 1. HEADER (Info y Progreso)
            string info = $"QUIZTEAM > {_categoria} | {_playerNombre} | {_indiceActual + 1}/{_preguntas.Count}";
            g.DrawString(info, new Font("Segoe UI", 10, FontStyle.Bold), Brushes.Crimson, 40, 25);

            int barW = this.ClientSize.Width - 80;
            DrawRoundRect(g, new Rectangle(40, 50, barW, 10), 5, Color.FromArgb(45, 45, 70), Color.Transparent);
            int progreso = (int)(barW * (_indiceActual + 1.0) / _preguntas.Count);
            DrawRoundRect(g, new Rectangle(40, 50, progreso, 10), 5, Color.FromArgb(233, 69, 96), Color.Transparent);

            // 2. PREGUNTA
            Rectangle rectPregunta = new Rectangle(40, 80, barW, 120);
            DrawRoundRect(g, rectPregunta, 15, Color.FromArgb(22, 33, 62), Color.FromArgb(233, 69, 96));
            using (Font f = new Font("Georgia", 18, FontStyle.Bold))
                g.DrawString(p.texto, f, Brushes.White, rectPregunta, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            // 3. OPCIONES
            if (p.tipo == "imagen") DibujarOpcionesImagen(g, p);
            else DibujarOpcionesTexto(g, p);

            // 4. BOTÓN SIGUIENTE
            if (_respondida)
            {
                DrawRoundRect(g, _zonaSiguiente, 25, Color.FromArgb(233, 69, 96), Color.Transparent);
                using (Font f = new Font("Segoe UI", 12, FontStyle.Bold))
                    g.DrawString(_indiceActual == _preguntas.Count - 1 ? "FINALIZAR" : "SIGUIENTE", f, Brushes.White, _zonaSiguiente, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }

            DibujarRankingLateral(g);
        }

        private void DibujarOpcionesTexto(Graphics g, Pregunta p)
        {
            for (int i = 0; i < 4; i++)
            {
                var r = _zonasOpciones[i];
                Color fill = Color.FromArgb(15, 52, 96), borde = Color.FromArgb(51, 51, 85);

                if (_respondida)
                {
                    if (i == p.correcta - 1) { fill = Color.DarkGreen; borde = Color.Lime; }
                    else if (i == _seleccion) { fill = Color.DarkRed; borde = Color.Red; }
                }
                else if (i == _seleccion) borde = Color.Cyan;

                DrawRoundRect(g, r, 15, fill, borde);
                using (Font f = new Font("Segoe UI", 13))
                    g.DrawString(p.opciones[i], f, Brushes.White, r, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
        }

        private void DibujarOpcionesImagen(Graphics g, Pregunta p)
        {
            for (int i = 0; i < 4; i++)
            {
                var r = _zonasOpciones[i];
                Color borde = (i == _seleccion) ? Color.Cyan : Color.FromArgb(51, 51, 85);
                if (_respondida && i == p.correcta - 1) borde = Color.Lime;

                DrawRoundRect(g, r, 15, Color.FromArgb(15, 33, 62), borde);
                if (i < _imagenesOpciones.Count && _imagenesOpciones[i] != null)
                {
                    g.DrawImage(_imagenesOpciones[i], new Rectangle(r.X + 10, r.Y + 10, r.Width - 20, r.Height - 20));
                }
            }
        }

        private void DibujarRankingLateral(Graphics g)
        {
            if (_rankingActual == null) return;
            int x = this.ClientSize.Width - 200;
            g.DrawString("TOP LIVE", new Font("Segoe UI", 9, FontStyle.Bold), Brushes.Gray, x, 220);
            for (int i = 0; i < Math.Min(_rankingActual.Count, 5); i++)
            {
                string txt = $"{i + 1}. {_rankingActual[i].nombre}: {_rankingActual[i].puntos}";
                g.DrawString(txt, new Font("Consolas", 9), Brushes.Silver, x, 245 + (i * 20));
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (_respondida && _zonaSiguiente.Contains(e.Location))
            {
                if (_indiceActual == _preguntas.Count - 1) _ = FinalizarJuego();
                else
                {
                    _indiceActual++; _seleccion = -1; _respondida = false;
                    CargarImagenesActual(); this.Invalidate();
                }
                return;
            }

            if (!_respondida)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (_zonasOpciones[i].Contains(e.Location))
                    {
                        _seleccion = i; _respondida = true;
                        bool correcto = (i == _preguntas[_indiceActual].correcta - 1);
                        if (correcto) _correctas++;
                        _ = EnviarWs(new { type = "respuesta", puntos = correcto ? 10 : 0 });
                        this.Invalidate(); break;
                    }
                }
            }
        }

        private async Task FinalizarJuego()
        {
            await EnviarWs(new { type = "end" });
            try { await Task.Delay(3000, _cts.Token); MostrarPodio(); }
            catch { }
        }

        private async Task EnviarWs(object obj)
        {
            if (_ws.State != WebSocketState.Open) return;
            try
            {
                string json = JsonSerializer.Serialize(obj);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch { }
        }

        public static void DrawRoundRect(Graphics g, Rectangle r, int radio, Color fill, Color borde)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                int d = radio * 2;
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                if (fill != Color.Transparent) g.FillPath(new SolidBrush(fill), path);
                if (borde != Color.Transparent) g.DrawPath(new Pen(borde, 2), path);
            }
        }

        private void ActualizarUI() { if (!this.IsDisposed) this.Invoke((Action)(() => this.Invalidate())); }
    }
}