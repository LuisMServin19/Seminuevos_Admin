using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using NuGet.Protocol;
using NuGet.Protocol.Plugins;
using Serfitex.Data;
using Serfitex.Models;
using System.Data;
using System.Diagnostics;
using WebApp.Models;

namespace Serfitex.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly NuevoBanorteContext _context;

        public HomeController(ILogger<HomeController> logger, NuevoBanorteContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            int idUsuario = 102;
            TAMenu menu1 = new TAMenu();
            string htmlMenu = "";

            if (ModelState.IsValid)
            {
                string connectionString = "Server=192.168.23.204;Port=3306;Database=nominet;Uid=nominet;password=pssnominet2017;";
                List<TAMenu> menus = new List<TAMenu>();

                try
                {
                    using (MySqlConnection conexion = new MySqlConnection(connectionString))
                    {
                        conexion.Open();

                        MySqlCommand cmd = new MySqlCommand();
                        cmd.Connection = conexion;
                        cmd.CommandText = "SPSObtener_menu";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                        cmd.Parameters["@idUsuario"].Direction = ParameterDirection.Input;


                        using (var cursor = cmd.ExecuteReader())
                        {
                            while (cursor.Read())
                            {
                                TAMenu menu = new TAMenu();


                                menu.fimenu = cursor.GetInt32("fimenu");
                                menu.fcdescmenu = cursor.GetString("fcdescmenu");
                                menu.fipadre = cursor.GetInt32("fipadre");
                                menu.fiposicion = cursor.GetInt32("fiposicion");
                                menu.fcicono = null;
                                menu.fbhabilitado = cursor.GetBoolean("fbhabilitado");
                                menu.fcurl = cursor.GetString("fcurl");
                               
                                menus.Add(menu);
                            }
                        }
                        menus = menus.OrderBy(m => m.fcdescmenu).ToList();
                        bool prod = false;
                        foreach (var item in menus)
                        {
                            if (item.fimenu == item.fipadre)
                            {
                                htmlMenu += "<li><a href=\"" + (prod ? item.fcurl : item.fcurl.Replace("/Indicadores", "")) + "\">" + item.fcdescmenu + "</a>";
                                htmlMenu += "<ul class=\"submenu\">";

                                foreach (var item2 in menus)
                                {
                                    if (item.fimenu != item2.fimenu)
                                    {
                                        if (item2.fipadre == item.fimenu)
                                        {
                                            htmlMenu = htmlMenu + "<li><a href=\"" + (prod ? item2.fcurl : (item2.fcurl.Replace("/Indicadores", ""))) + "\">" + item2.fcdescmenu + "</a></li>";
                                        }
                                    }
                                }

                                htmlMenu += "</ul>";
                                htmlMenu += "</li>";
                            }
                        }
                    }

                    Console.WriteLine("HTML Menu: " + htmlMenu);

                    HttpContext.Session.SetString("menu", htmlMenu);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult cambiarConexion(string cliente)
        {
            HttpContext.Session.SetString("conexion", cliente);

            return Ok(cliente);
        }
    }
}