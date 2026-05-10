using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QUIZTEAM
{
    public class ConexionServidor : IDisposable
    {
        private TcpClient _tcp;
        private NetworkStream _stream;
        private StreamReader _reader;

        // Evento que dispara cuando llega un mensaje del servidor
        public event Action<JsonElement> OnMensaje;

        // ── Conectar ─────────────────────────────────────────────────
        public async Task ConectarAsync()
        {
            _tcp = new TcpClient();
            await _tcp.ConnectAsync(Config.ServerIP, Config.ServerPort);
            _stream = _tcp.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8);

            // Escuchar en background
            _ = Task.Run(EscucharAsync);
        }

        // ── Escuchar mensajes entrantes ──────────────────────────────
        private async Task EscucharAsync()
        {
            try
            {
                string linea;
                while ((linea = await _reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    try
                    {
                        var msg = JsonSerializer.Deserialize<JsonElement>(linea);
                        OnMensaje?.Invoke(msg);
                    }
                    catch { /* JSON malformado, ignorar */ }
                }
            }
            catch { /* servidor cerró conexión */ }
        }

        // ── Enviar mensaje ───────────────────────────────────────────
        public async Task EnviarAsync(object mensaje)
        {
            if (_tcp == null || !_tcp.Connected) return;
            try
            {
                string json = JsonSerializer.Serialize(mensaje) + "\n";
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await _stream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch { }
        }

        public bool Conectado => _tcp?.Connected == true;

        public void Dispose()
        {
            _reader?.Dispose();
            _stream?.Dispose();
            _tcp?.Close();
        }
    }
}