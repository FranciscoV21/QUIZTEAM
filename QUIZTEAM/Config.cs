namespace QUIZTEAM
{
    public static class Config
    {
        public static string ServerIP = "192.168.1.222";
        public static int ServerPort = 8000;
        public static string ApiUrl => $"http://{ServerIP}:{ServerPort}";
        public static string WsUrl => $"ws://{ServerIP}:{ServerPort}";
        public static string RoomId = "sala1"; // sala fija para todos
    }
}