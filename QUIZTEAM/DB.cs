using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUIZTEAM
{
    internal class DB
    {
        private static string connStr = "Server=gondola.proxy.rlwy.net;Port=42614;Database=railway;Uid=root;Pwd=MNzmTeajpSCLHElCQguamXzDqEkdhZqN;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connStr);
        }
    }
}
