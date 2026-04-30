namespace QUIZTEAM
{
    public static class Config
    {
        // Cambia solo aquí si el servidor cambia de IP
        public static string ServerIP = "10.31.144.84";
        public static int ServerPort = 8000;
        public static string ApiUrl => $"http://{ServerIP}:{ServerPort}";
    }
}