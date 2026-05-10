using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace QUIZTEAM.Servidor
{
    public class Servidor
    {
        private readonly int _puerto;
        private readonly ConcurrentDictionary<string, SalaJuego> _salas
            = new ConcurrentDictionary<string, SalaJuego>();

        public Servidor(int puerto) { _puerto = puerto; }

        public async Task IniciarAsync()
        {
            var listener = new TcpListener(IPAddress.Any, _puerto);
            listener.Start();
            Console.WriteLine($"[Servidor] Escuchando en puerto {_puerto}...");
            Console.WriteLine("[Servidor] Ctrl+C para detener.\n");

            while (true)
            {
                TcpClient cliente = await listener.AcceptTcpClientAsync();
                Console.WriteLine($"[+] Cliente conectado: {((IPEndPoint)cliente.Client.RemoteEndPoint).Address}");

                // Cada cliente en su propio Task — no bloqueamos el loop
                _ = Task.Run(async () =>
                {
                    var manejador = new ManejadorCliente(cliente, _salas);
                    await manejador.ManejarAsync();
                });
            }
        }
    }
}