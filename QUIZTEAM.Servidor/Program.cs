using System;
using System.Threading.Tasks;

namespace QUIZTEAM.Servidor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "QUIZTEAM Servidor";
            Console.WriteLine("╔══════════════════════════════╗");
            Console.WriteLine("║     QUIZTEAM  SERVIDOR       ║");
            Console.WriteLine("╚══════════════════════════════╝\n");

            int puerto = 9000;
            if (args.Length > 0 && int.TryParse(args[0], out int p))
                puerto = p;

            var servidor = new Servidor(puerto);
            await servidor.IniciarAsync();
        }
    }
}