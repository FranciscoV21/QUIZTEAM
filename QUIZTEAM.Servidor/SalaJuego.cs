using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QUIZTEAM.Servidor
{
    public class JugadorEnSala
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public int Puntos { get; set; }
        public int Correctas { get; set; }
        public int Incorrectas { get; set; }
        public bool EsLider { get; set; }
        public TcpClient Cliente { get; set; }
    }

    public class SalaJuego
    {
        public string Id { get; }
        private readonly ConcurrentDictionary<string, JugadorEnSala> _jugadores
            = new ConcurrentDictionary<string, JugadorEnSala>();

        public SalaJuego(string id) { Id = id; }

        public JugadorEnSala Unirse(TcpClient cliente)
        {
            string playerId = System.Guid.NewGuid().ToString("N").Substring(0, 8);
            int numero = _jugadores.Count + 1;
            bool esLider = _jugadores.IsEmpty;

            var jugador = new JugadorEnSala
            {
                Id = playerId,
                Nombre = $"Jugador {numero}",
                Puntos = 0,
                Correctas = 0,
                Incorrectas = 0,
                EsLider = esLider,
                Cliente = cliente
            };

            _jugadores[playerId] = jugador;
            return jugador;
        }

        public void Salir(string playerId)
        {
            _jugadores.TryRemove(playerId, out _);
        }

        public bool EstaVacia() => _jugadores.IsEmpty;
        public int CuentaJugadores() => _jugadores.Count;

        public List<PlayerScore> ActualizarPuntaje(string playerId, int puntos)
        {
            if (_jugadores.TryGetValue(playerId, out var j))
            {
                j.Puntos += puntos;
                if (puntos > 0) j.Correctas++;
                else j.Incorrectas++;
            }
            return ObtenerRanking();
        }

        public List<PlayerScore> ObtenerRanking()
        {
            return _jugadores.Values
                .OrderByDescending(j => j.Puntos)
                .Select(j => new PlayerScore
                {
                    nombre = j.Nombre,
                    puntos = j.Puntos,
                    correctas = j.Correctas,
                    incorrectas = j.Incorrectas
                })
                .ToList();
        }

        public async Task BroadcastAsync(object mensaje)
        {
            string json = JsonSerializer.Serialize(mensaje) + "\n";
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            foreach (var j in _jugadores.Values)
            {
                try
                {
                    if (j.Cliente.Connected)
                        await j.Cliente.GetStream().WriteAsync(bytes, 0, bytes.Length);
                }
                catch { }
            }
        }

        public async Task EnviarA(string playerId, object mensaje)
        {
            if (!_jugadores.TryGetValue(playerId, out var j)) return;
            string json = JsonSerializer.Serialize(mensaje) + "\n";
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            try
            {
                if (j.Cliente.Connected)
                    await j.Cliente.GetStream().WriteAsync(bytes, 0, bytes.Length);
            }
            catch { }
        }
    }
}