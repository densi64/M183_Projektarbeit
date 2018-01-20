using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Mini_Blog_Engine.Models;

namespace Mini_Blog_Engine.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
            var con = SQL_String.GetConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT [Title], [Description], [content], [Id] FROM [dbo].[Post] WHERE DeletedOn IS Null";
            cmd.Connection = con;
			con.Open();

			var reader = cmd.ExecuteReader();
			var list = new List<Post>();
			if (reader.HasRows)
			{
				while (reader.Read())
				{
					list.Add(new Post()
					{
						Title = reader.GetString(0),
						Description = reader.GetString(1),
						Content = reader.GetString(2),
						Id = reader.GetInt32(3)
					});
				}
			}
			else
			{
				ViewBag.Message = "Keine Einträge gefunden.";
			}

			con.Close();
			return View(list);
		}
	}
}
