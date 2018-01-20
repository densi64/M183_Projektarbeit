using System;
using Mini_Blog_Engine.Models;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Mini_Blog_Engine.Controllers
{
    public class LoginController : System.Web.Mvc.Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            int loginTryNumber = 0;
            var username = Request["username"];
            var password = Request["password"];

            var value = Request.Cookies.Get(username)?.Value;
            if (value != null)
            {
                int.TryParse(value, out loginTryNumber);
                loginTryNumber += 1;
                var cookie = new HttpCookie(username);
                cookie.Expires = DateTime.Now.AddMinutes(5);
                cookie.Value = loginTryNumber.ToString();
                Response.Cookies.Add(cookie);
            }
            else
            {
                var cookie = new HttpCookie(username);
                cookie.Expires = DateTime.Now.AddMinutes(5);
                cookie.Value = "1";
                Response.Cookies.Add(cookie);
            }

            var con = SQL_String.GetConnection();
            SqlCommand cmd;

            if (loginTryNumber > 3)
            {
                try
                {
                    cmd = new SqlCommand
                    {
                        CommandText = $"UPDATE [User] SET [Status] = 'blocked' WHERE [username] = '{username}'",
                        Connection = con
                    };

                    con.Open();
                    cmd.ExecuteReader();
                    ViewBag.Message = "Login failed";
                    con.Close();
                }
                catch (Exception)
                {
                    con.Close();
                }
                return View("Index");
            }

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
            {
                ViewBag.Message = "Login failed";
                return View("Index");
            }

            con = SQL_String.GetConnection();

            con.Open();
            cmd = new SqlCommand
            {
                CommandText = "SELECT [Id], [Mobilephonenumber] FROM [dbo].[User] WHERE [username] = '" + username + "'",
                Connection = con
            };

            var reader = cmd.ExecuteReader();

            var id = -1;
            string phone = "000";
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    id = reader.GetInt32(0);
                    phone = reader.GetString(1);
                }

                var generator = new Random();
                string token = generator.Next(0, 1000000).ToString("D6");

                cmd = new SqlCommand
                {
                    CommandText = "INSERT INTO [dbo].[Token] (Token, UserId, Expiry, DeletedOn) VALUES('" + token + "', " + id + ", '" + DateTime.Now.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:ss") + "', NULL)",
                    Connection = con
                };
                cmd.ExecuteReader();
                con.Close();

                try
                {
                    var request = (HttpWebRequest)WebRequest.Create("https://rest.nexmo.com/sms/json");
                
                    var postData = "api_key=56435b83";
                    postData += "&api_secret=3350e362dfb8ec70";
                    postData += "&to=" + phone;
                    postData += "&from=\"\"Blog\"\"";
                    postData += "&text=\"" + token + "\"";
                    var data = Encoding.ASCII.GetBytes(postData);

                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch (Exception e)
                {
                    WriteError("Error while sending: " + e, id);
                }
            }
            else
            {
                WriteError("Login error", id);
                return View("Index");
            }

            return View("TokenLogin", new UserLogin(){Username = username, Password = password});
        }

        private void WriteError(string message, int id)
        {
            var con = SQL_String.GetConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = $"INSERT INTO[UserLog] (UserId, Action) VALUES({id}, '{message}')";
            cmd.Connection = con;
            con.Open();
            cmd.ExecuteReader();

            ViewBag.Message = message;
        }

        [HttpPost]
        public ActionResult TokenLogin()
        {
            var token = Request["token"];
            var username = Request["username"];
            var passwordForm = Request["password"];


            if (!token.All(char.IsLetterOrDigit))
                return RedirectToAction("TokenLogin", "Login", new UserLogin() { Username = username, Password = passwordForm });
            var con = SQL_String.GetConnection();

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT [Id], [Password], [Role] FROM [dbo].[User] WHERE [username] = '" + username + "'";
            cmd.Connection = con;
            con.Open();

            var reader = cmd.ExecuteReader();
            int id = -1;
            var password = "";
            var generatedToken = "";
            var role = "";
            DateTime expiry = new DateTime();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    id = reader.GetInt32(0);
                    password = reader.GetString(1);
                    role = reader.GetString(2);
                }
            }

            cmd = new SqlCommand
            {
                CommandText = "SELECT [Token], [Expiry]  FROM Token WHERE DeletedOn IS NULL AND UserId =" + id,
                Connection = con
            };

            reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    generatedToken = reader.GetString(0);
                    expiry = reader.GetDateTime(1);
                }
            }

            if (password == passwordForm && token == generatedToken && expiry > DateTime.Now)
            {
                cmd = new SqlCommand
                {
                    CommandText = "UPDATE Token SET DeletedOn ='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE [UserId] =" + id,
                    Connection = con
                };
                cmd.ExecuteReader();

                cmd = new SqlCommand
                {
                    CommandText = "INSERT INTO [UserLog] (UserId, Action) VALUES(" + id + ", 'Login')",
                    Connection = con
                };
                cmd.ExecuteReader();

                Session["username"] = username;

                cmd = new SqlCommand
                {
                    CommandText = $"INSERT INTO [Userlogin] (UserId, IP, SessionId, CreatedOn) VALUES({id}, '{Request.UserHostAddress}', '{username}', GETDATE())",
                    Connection = con
                };
                cmd.ExecuteReader();

                con.Close();
                if (role == "admin")
                    return RedirectToAction("Dashboard", "Admin");
                else if (role == "user")
                    return RedirectToAction("Dashboard", "User");
            }
            else
                WriteError("Passwort oder Token ist falsch. Evtl. ist das Token bereits abgelaufen.", id);
            
            return View("TokenLogin", new UserLogin() { Username = username, Password = password });
        }
    }
}