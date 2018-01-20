using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Mini_Blog_Engine.Models
{
    public class SQL_String
    {

        public static SqlConnection GetConnection()
        {
            SqlConnection con = new SqlConnection();
            con.ConnectionString =
                "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\DSari\\OneDrive\\Dokumente\\GitHub\\M183_Projektarbeit\\Mini-Blog-Engine\\Database\\M183_PROJECT.MDF\";Integrated Security=True;MultipleActiveResultSets=True;Connect Timeout=30;Application Name=EntityFramework";
            return con;
           
        }
    }
}