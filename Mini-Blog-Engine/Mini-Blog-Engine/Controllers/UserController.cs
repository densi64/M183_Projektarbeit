using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mini_Blog_Engine.Models;

namespace Mini_Blog_Engine.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Dashboard()
        {
            var currentUser = (string)Session["username"];
            var con = SQL_String.GetConnection();
            con.Open();
            var cmd = new SqlCommand
            {
                CommandText = $"SELECT [Id], [Role] FROM [dbo].[User] WHERE [Username] = '{currentUser}' AND [DeletedOn] IS NULL",
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

            if (currentUserRole == "User")
            {
                con = SQL_String.GetConnection();

                cmd = new SqlCommand
                {
                    CommandText = "SELECT [Title], [Description], [content], [Id] FROM [dbo].[Post] WHERE [UserId] = " + id,
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
    }
}