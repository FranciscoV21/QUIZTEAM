using MySql.Data.MySqlClient;

namespace TestDB
{
    public class DB
    {
        private static string connStr = "Server=gondola.proxy.rlwy.net;Port=42614;Database=railway;Uid=root;Pwd=MNzmTeajpSCLHElCQguamXzDqEkdhZqN;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connStr);
        }
    }
}