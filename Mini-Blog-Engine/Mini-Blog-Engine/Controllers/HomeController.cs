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


/*
 * Frage 1:  Warum haben Sie sich für gerade für den Hash Algorithmus (Usernamen & Passwort) entschieden?
 * 
 * Wir haben den sha256 Algorithmus verwendet, wenn wir mehr Zeit für das Projekt gehabt hätten.
 * Wir hätten uns für den SHA-3 Algorithmus entschieden, da dies der aktuellste und somit unserer Meinung nach der sicherste Algorithmus ist.
 *
 * 
 * 
 * Frage 2: In der User-Login-Tabelle ist noch ein Feld für die IP-Adresse Reserviert. Welche Attacke lässt sich dadurch verhindern?
 * 
 * Über die Session-ID eines Benutzers einloggen
 * 
 * 
 * 
 * Frage 3:  Erklären Sie, wie diese Attacke genau funktioniert und inwiefern die Gegenmassnahmen die Attacke vereitelt?
 * 
 * Der Hacker "klaut" die Session-ID eines bereits vorhandenen Benutzers. Die Gegenmassnahme dafür ist das Abfragen der IP-Adresse. Man kann somit überprüfen, 
 * ob es sich bei der Anmeldung um eine fremde IP-Adresse und somit auch um einen potenziellen Hacker handelt.
 *
 */
