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

        public event Action<JsonElement> OnMensaje;

        public async Task ConectarAsync()
        {
            _tcp = new TcpClient();
            await _tcp.ConnectAsync(Config.ServerIP, Config.ServerPort);
            _stream = _tcp.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8);
            _ = Task.Run(EscucharAsync);
        }

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
                    catch { }
                }
            }
            catch { }
        }

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
            try { _reader?.Dispose(); } catch { }
            try { _stream?.Dispose(); } catch { }
            try { _tcp?.Close(); } catch { }
        }
    }
}