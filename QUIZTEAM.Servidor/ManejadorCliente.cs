using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QUIZTEAM.Servidor
{
    public class ManejadorCliente
    {
        private readonly TcpClient _cliente;
        private readonly ConcurrentDictionary<string, SalaJuego> _salas;

        private SalaJuego _salaActual;
        private string _playerId;
        private NetworkStream _stream;

        public ManejadorCliente(TcpClient cliente,
            ConcurrentDictionary<string, SalaJuego> salas)
        {
            _cliente = cliente;
            _salas = salas;
        }

        // ── Bucle principal ──────────────────────────────────────────
        public async Task ManejarAsync()
        {
            _stream = _cliente.GetStream();
            // DESPUÉS (C# 7.3)
            using (var reader = new StreamReader(_stream, Encoding.UTF8))
            {

                try
                {
                    string linea;
                    while ((linea = await reader.ReadLineAsync()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(linea)) continue;
                        await ProcesarMensaje(linea);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Cliente] Desconectado: {ex.Message}");
                }
                finally
                {
                    await DesconectarJugador();
                    _cliente.Close();
                }
            }
        }

        // ── Router de mensajes ───────────────────────────────────────
        private async Task ProcesarMensaje(string json)
        {
            JsonElement msg;
            try { msg = JsonSerializer.Deserialize<JsonElement>(json); }
            catch { return; }

            if (!msg.TryGetProperty("type", out var typeProp)) return;
            string type = typeProp.GetString();

            Console.WriteLine($"[{type}] {(_playerId ?? "?")}");

            switch (type)
            {
                case "get_categorias":
                    await HandleGetCategorias();
                    break;

                case "get_preguntas":
                    string cat = msg.GetProperty("categoria").GetString();
                    await HandleGetPreguntas(cat);
                    break;

                case "unirse":
                    string roomId = msg.GetProperty("room_id").GetString();
                    await HandleUnirse(roomId);
                    break;

                case "respuesta":
                    int puntos = msg.GetProperty("puntos").GetInt32();
                    await HandleRespuesta(puntos);
                    break;

                case "start_game":
                    string categoria = msg.GetProperty("categoria").GetString();
                    await HandleStartGame(categoria);
                    break;

                case "end":
                    string catFin = msg.GetProperty("categoria").GetString();
                    int correctas = msg.GetProperty("correctas").GetInt32();
                    int total = msg.GetProperty("total").GetInt32();
                    await HandleEnd(catFin, correctas, total);
                    break;
            }
        }

        // ── Handlers ─────────────────────────────────────────────────

        private async Task HandleGetCategorias()
        {
            var cats = BaseDatos.ObtenerCategorias();
            await Enviar(new { type = "categorias", data = cats });
        }

        private async Task HandleGetPreguntas(string categoria)
        {
            var preguntas = BaseDatos.ObtenerPreguntas(categoria);
            await Enviar(new { type = "preguntas", data = preguntas });
        }

        private async Task HandleUnirse(string roomId)
        {
            // Crear sala si no existe
            var sala = _salas.GetOrAdd(roomId, id => new SalaJuego(id));
            _salaActual = sala;

            var jugador = sala.Unirse(_cliente);
            _playerId = jugador.Id;

            // Confirmar conexión al jugador que acaba de entrar
            await Enviar(new
            {
                type = "connected",
                player_id = jugador.Id,
                nombre = jugador.Nombre,
                es_lider = jugador.EsLider
            });

            // Notificar a todos cuántos hay en sala
            await sala.BroadcastAsync(new
            {
                type = "sala_update",
                jugadores = sala.CuentaJugadores()
            });
        }

        private async Task HandleRespuesta(int puntos)
        {
            if (_salaActual == null || _playerId == null) return;

            var ranking = _salaActual.ActualizarPuntaje(_playerId, puntos);
            await _salaActual.BroadcastAsync(new
            {
                type = "score",
                ranking = ranking
            });
        }

        private async Task HandleStartGame(string categoria)
        {
            if (_salaActual == null) return;
            await _salaActual.BroadcastAsync(new
            {
                type = "start_game",
                categoria = categoria
            });
        }

        private async Task HandleEnd(string categoria, int correctas, int total)
        {
            // Guardar partida en DB
            try { BaseDatos.GuardarPartida(categoria, correctas, total); }
            catch (Exception ex) { Console.WriteLine($"[DB] Error guardando partida: {ex.Message}"); }

            if (_salaActual == null) return;

            var ranking = _salaActual.ObtenerRanking();
            await _salaActual.BroadcastAsync(new
            {
                type = "final",
                ranking = ranking
            });
        }

        // ── Desconexión ──────────────────────────────────────────────
        private async Task DesconectarJugador()
        {
            if (_salaActual == null || _playerId == null) return;

            _salaActual.Salir(_playerId);

            if (_salaActual.EstaVacia())
                _salas.TryRemove(_salaActual.Id, out _);
            else
                await _salaActual.BroadcastAsync(new
                {
                    type = "sala_update",
                    jugadores = _salaActual.CuentaJugadores()
                });
        }

        // ── Enviar al cliente actual ─────────────────────────────────
        private async Task Enviar(object mensaje)
        {
            try
            {
                string json = JsonSerializer.Serialize(mensaje) + "\n";
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await _stream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch { }
        }
    }
}