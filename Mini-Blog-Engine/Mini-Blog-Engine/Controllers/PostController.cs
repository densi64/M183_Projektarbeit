using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mini_Blog_Engine.Models;

namespace Mini_Blog_Engine.Controllers
{
    public class PostController : Controller
    {
		public ActionResult Index(int id)
        {
            var con = SQL_String.GetConnection();

            SqlCommand cmd = new SqlCommand
			{
				CommandText = "SELECT Commet FROM [dbo].[Comment] WHERE PostId = " + id,
				Connection = con
			};

			con.Open();

			var reader = cmd.ExecuteReader();

			var list = new List<string>();
			if (reader.HasRows)
			{
				while (reader.Read())
				{
					list.Add(reader.GetString(0));
				}
			}

			SqlCommand cmdPost = new SqlCommand
			{
				CommandText = "SELECT [Title], [Description], [content] FROM [dbo].[Post] WHERE Id = " + id,
				Connection = con
			};
			reader = cmdPost.ExecuteReader();

			var post = new Post();
			while (reader.Read())
			{
				post = new Post()
				{
					Title = reader.GetString(0),
					Description = reader.GetString(1),
					Content = reader.GetString(2),
					Id = id
				};
			}
			con.Close();

			post.Comments = list;
			return View(post);
		}

		[HttpPost]
		public ActionResult AddComment()
		{
			var comment = Request["comment"];
		    var id = Request["id"];

            if (!comment.All(char.IsLetterOrDigit))
		        return RedirectToAction("Index", "Home");

			if (!string.IsNullOrEmpty(comment) && comment.Length < 200)
			{
			    var con = SQL_String.GetConnection();
				SqlCommand cmd = new SqlCommand
				{
					CommandText = "INSERT INTO [dbo].[Comment] (commet, PostId) VALUES('" + comment + "', " + id + ")",
					Connection = con
				};
				con.Open();
				cmd.ExecuteReader();
				ViewBag.Message = "";
				con.Close();
			}
			else
			{
				ViewBag.Message = "Text ist zu lang oder ist leer!";
			}

		    return View();
		}
	}
}