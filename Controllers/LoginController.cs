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
                string connectionString = Configuration["BDs:cetelem"];

                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conexion;
                    cmd.CommandText = "SELECT ID_USR,USR_NAME,USR_NICK,USR_PASS,USR_ACTIVE FROM login_user where USR_NICK = @pfclave and USR_PASS = @pfcpassword AND (ID_USR = 1 OR ID_USR = 20 OR ID_USR = 153);";

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

                            usuario = new login_user()
                            {
                                usr_nick = usr_nick,
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

                HttpContext.Session.SetString("conexion", "cetelem");

                return RedirectToAction("Index", "Home");
            }
        }
    }
}