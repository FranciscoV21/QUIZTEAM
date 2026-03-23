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
        private static string connStr = "Server=localhost;Database=quiz;Uid=root;Pwd=12345;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connStr);
        }
    }
}
