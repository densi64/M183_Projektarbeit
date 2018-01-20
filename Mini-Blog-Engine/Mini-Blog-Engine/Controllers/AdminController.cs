using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mini_Blog_Engine.Models;

namespace Mini_Blog_Engine.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Dashboard()
        {
            var currentUser = (string)Session["username"];
            var con = SQL_String.GetConnection();
            con.Open();
            var cmd = new SqlCommand
            {
                CommandText = $"SELECT [Id], [Role] FROM [dbo].[User] WHERE [Username] = '{currentUser}'",
                Connection = con
            };
            var reader = cmd.ExecuteReader();

            var currentUserRole = "";
            var id = -1;
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    id = reader.GetInt32(0);
                    currentUserRole = reader.GetString(1);
                }
            }

            if (currentUserRole == "Administrator")
            {
                con = SQL_String.GetConnection();

                cmd = new SqlCommand
                {
                    CommandText = "SELECT [Title], [Description], [content], [Id] FROM [dbo].[Post]",
                    Connection = con
                };

                reader = cmd.ExecuteReader();

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
                    ViewBag.Message = "Keine Einträge zu finden";
                }

                con.Close();
                return View(list);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Search()
        {
            var list = new List<Post>();
            var searchString = Request["searchString"];

            if (!searchString.All(char.IsLetterOrDigit))
                return RedirectToAction("Dashboard", "Admin");

            var con = SQL_String.GetConnection();
            con.Open();
            var cmd = new SqlCommand
            {
                CommandText = $"SELECT [Title], [Description], [content] FROM [dbo].[Post] WHERE [Title] LIKE '%{searchString}%' OR [Description] LIKE '%{searchString}%' OR [Content] LIKE '%{searchString}%'",
                Connection = con
            };
            var reader = cmd.ExecuteReader();
            
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    list.Add(new Post()
                    {
                        Title = reader.GetString(0),
                        Description = reader.GetString(1),
                        Content = reader.GetString(2)
                    });
                }
            }
            con.Close();
            return RedirectToAction("Dashboard", "Admin", list);
        }
    }
}