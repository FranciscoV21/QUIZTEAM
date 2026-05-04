using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace QUIZTEAM
{
    public static class Config
    {
        public static string ServerIP = "192.168.1.151";
        public static int ServerPort = 8000;
        public static string ApiUrl => $"http://{ServerIP}:{ServerPort}";
        public static string WsUrl => $"ws://{ServerIP}:{ServerPort}";
        public static string RoomId = "sala1";

        private static ClientWebSocket _ws = null;

        public static async Task<ClientWebSocket> NuevoWebSocket()
        {
            if (_ws != null)
            {
                try
                {
                    if (_ws.State == WebSocketState.Open)
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                catch { }
                _ws.Dispose();
                _ws = null;
                await Task.Delay(600);
            }
            _ws = new ClientWebSocket();
            return _ws;
        }
    }
}