using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Serfitex.Data;
using Serfitex.Models;
using System.Data;
using WebApp.Models;

namespace Serfitex.Controllers
{
    public class LoginController : Controller
    {
        private readonly NuevoBanorteContext _context;
        private readonly IConfiguration Configuration;

        public LoginController(NuevoBanorteContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }

        // GET: Usuarios
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index([Bind("username, Contrasena")] Login login)
        {
            login_user usuario = new login_user();

            if (ModelState.IsValid)
            {
                string connectionString = Configuration["BDs:SemiCC"];

                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conexion;
                    cmd.CommandText = "SELECT * FROM login_user where USR_NICK = @pfclave and USR_PASS = @pfcpassword;";

                    cmd.Parameters.AddWithValue("@pfclave", login.username);
                    cmd.Parameters["@pfclave"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@pfcpassword", login.Contrasena);
                    cmd.Parameters["@pfcpassword"].Direction = ParameterDirection.Input;

                    using (var cursor = cmd.ExecuteReader())
                    {
                        if (!cursor.HasRows)
                            ModelState.AddModelError("", "Usuario o contraseña no valida");

                        while (cursor.Read())
                        {
                            string usr_nick = Convert.ToString(cursor["usr_nick"]);
                            string fiperfil = Convert.ToString(cursor["fiperfil"]);

                            usuario = new login_user()
                            {
                                usr_nick = usr_nick,
                                fiperfil = fiperfil,
                            };
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View(login);
            }
            else
            {
                HttpContext.Session.SetString("username", login.username);
                HttpContext.Session.SetString("fiusr", usuario.usr_nick.ToString());
                HttpContext.Session.SetString("fiperfil", usuario.fiperfil.ToString());

                HttpContext.Session.SetString("conexion", "SemiCC");

                return RedirectToAction("Index", "Unidades");
            }
        }
    }
}