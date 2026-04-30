namespace QUIZTEAM
{
    public static class Config
    {
        public static string ServerIP = "10.31.144.84";
        public static int ServerPort = 8000;
        public static string ApiUrl => $"http://{ServerIP}:{ServerPort}";
        public static string WsUrl => $"ws://{ServerIP}:{ServerPort}";
        public static string RoomId = "sala1"; // sala fija para todos
    }
}