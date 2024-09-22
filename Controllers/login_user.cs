using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using NuevoBanorte.Models;
using NuGet.Protocol;
using System.Data;
using System.Text;
using WebApp.Models;

namespace Serfitex.Controllers
{
    public class login_userController : Controller
    {
        private readonly ILogger<login_userController> _logger;
        //private readonly IKTTheme _theme;
        private readonly IConfiguration Configuration;

        public login_userController(ILogger<login_userController> logger, /*IKTTheme theme*/ IConfiguration configuration)
        {
            _logger = logger;
            //_theme = theme;
            Configuration = configuration;
        }

        public IActionResult Index()
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string cliente = HttpContext.Session.GetString("conexion") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:" + cliente];


            List<login_user> registros = new List<login_user>();

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT ID_USR,usr_name,usr_active FROM login_user ORDER BY ID_USR DESC";
                cmd.CommandType = System.Data.CommandType.Text;

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        int ID_USR = Convert.ToInt32(cursor["ID_USR"]);
                        string usr_name = Convert.ToString(cursor["usr_name"]);
                        bool usr_active = Convert.ToBoolean(cursor["usr_active"]);

                        login_user registro = new login_user() { ID_USR = ID_USR, usr_name = usr_name, usr_active = usr_active };
                        registros.Add(registro);
                    }
                }
            }
            return View(registros);
        }

        // GET: login_user/Create
        public IActionResult Create()
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            return View();
        }

        // POST: login_user/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(login_user newBranch)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string cliente = HttpContext.Session.GetString("conexion") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (ModelState.IsValid)
            {
                string connectionString = Configuration["BDs:" + cliente];

                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    MySqlCommand getLastCodeCmd = new MySqlCommand("SELECT MAX(ID_USR) FROM login_user", conexion);
                    int lastCode = Convert.ToInt16(getLastCodeCmd.ExecuteScalar());
                    int newCode = lastCode + 1;

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conexion;
                    cmd.CommandText = "INSERT INTO login_user (ID_USR,ID_ROL,USR_NICK,USR_PASS,USR_NAME,USR_ACTIVE,USR_CDATE) VALUES (@ID_USR,4,@USR_NICK,@USR_PASS,@USR_NAME,1,@USR_CDATE)";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@ID_USR", newCode);
                    cmd.Parameters.AddWithValue("@USR_NICK", newBranch.usr_nick);
                    cmd.Parameters.AddWithValue("@USR_PASS", newBranch.usr_pass);
                    cmd.Parameters.AddWithValue("@USR_NAME", newBranch.usr_name);
                    cmd.Parameters.AddWithValue("@USR_ACTIVE", newBranch.Status);
                    cmd.Parameters.AddWithValue("@USR_CDATE", DateTime.Now);

                    cmd.ExecuteNonQuery();

                    Cambios cambios = new Cambios
                    {
                        Usuario = HttpContext.Session.GetString("username") ?? "",
                        Datos = newBranch.ToJson(),
                        FechaCambios = DateTime.Now
                    };

                    MySqlCommand logCmd = new MySqlCommand();
                    logCmd.Connection = conexion;
                    logCmd.CommandText = "INSERT INTO Cambios (Usuario, Datos, FechaCambios) VALUES (@Usuario, @Datos, @FechaCambios)";
                    logCmd.CommandType = System.Data.CommandType.Text;
                    logCmd.Parameters.AddWithValue("@Usuario", cambios.Usuario);
                    logCmd.Parameters.AddWithValue("@Datos", cambios.Datos);
                    logCmd.Parameters.AddWithValue("@FechaCambios", cambios.FechaCambios);

                    logCmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index");
            }

            return View(newBranch);
        }

        // GET: login_user/Edit/
        public IActionResult Edit(int? id)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string cliente = HttpContext.Session.GetString("conexion") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (id == null)
            {
                return NotFound();
            }

            string connectionString = Configuration["BDs:" + cliente];
            login_user user = null;

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT ID_USR, usr_name, usr_nick, usr_pass, usr_active FROM login_user WHERE ID_USR = @ID_USR";
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@ID_USR", id);

                using (var cursor = cmd.ExecuteReader())
                {
                    if (cursor.Read())
                    {
                        user = new login_user()
                        {
                            ID_USR = Convert.ToInt32(cursor["ID_USR"]),
                            usr_name = Convert.ToString(cursor["usr_name"]),
                            usr_nick = Convert.ToString(cursor["usr_nick"]),
                            usr_pass = Convert.ToString(cursor["usr_pass"]),
                            usr_active = Convert.ToBoolean(cursor["usr_active"])
                        };
                    }
                }
            }

            if (user == null)
            {
                return NotFound();
            }
            ViewData["StatusOptions"] = new List<SelectListItem>
    {
        new SelectListItem { Value = "true", Text = "Activo" },
        new SelectListItem { Value = "false", Text = "Desactivado" }
    };

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, login_user user)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string cliente = HttpContext.Session.GetString("conexion") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (ModelState.IsValid)
            {
                string connectionString = Configuration["BDs:" + cliente];

                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    MySqlCommand fetchCmd = new MySqlCommand();
                    fetchCmd.Connection = conexion;
                    fetchCmd.CommandText = "SELECT ID_USR, usr_name, usr_nick, usr_pass, usr_active FROM login_user WHERE ID_USR = @ID_USR";
                    fetchCmd.CommandType = System.Data.CommandType.Text;
                    fetchCmd.Parameters.AddWithValue("@ID_USR", id);

                    login_user existingUser = null;
                    using (var cursor = fetchCmd.ExecuteReader())
                    {
                        if (cursor.Read())
                        {
                            existingUser = new login_user()
                            {
                                ID_USR = Convert.ToInt32(cursor["ID_USR"]),
                                usr_name = Convert.ToString(cursor["usr_name"]),
                                usr_nick = Convert.ToString(cursor["usr_nick"]),
                                usr_pass = Convert.ToString(cursor["usr_pass"]),
                                usr_active = Convert.ToBoolean(cursor["usr_active"])
                            };
                        }
                    }
                    if (string.IsNullOrEmpty(user.usr_pass))
                    {
                        user.usr_pass = existingUser.usr_pass;
                    }

                    MySqlCommand updateCmd = new MySqlCommand();
                    updateCmd.Connection = conexion;
                    updateCmd.CommandText = "UPDATE login_user SET usr_name = @usr_name, usr_nick = @usr_nick, usr_pass = @usr_pass, usr_active = @usr_active WHERE ID_USR = @ID_USR";
                    updateCmd.CommandType = System.Data.CommandType.Text;
                    updateCmd.Parameters.AddWithValue("@ID_USR", id);
                    updateCmd.Parameters.AddWithValue("@usr_name", user.usr_name);
                    updateCmd.Parameters.AddWithValue("@usr_nick", user.usr_nick);
                    updateCmd.Parameters.AddWithValue("@usr_pass", user.usr_pass);
                    updateCmd.Parameters.AddWithValue("@usr_active", user.usr_active);

                    updateCmd.ExecuteNonQuery();

                    Cambios cambios = new Cambios();
                    cambios.Usuario = username;
                    cambios.Datos = existingUser.ToJson();
                    cambios.FechaCambios = DateTime.Now;

                    MySqlCommand logCmd = new MySqlCommand();
                    logCmd.Connection = conexion;
                    logCmd.CommandText = "INSERT INTO CAmbios (Usuario, Datos, FechaCambios) VALUES (@Usuario, @Datos, @FechaCambios)";
                    logCmd.CommandType = System.Data.CommandType.Text;
                    logCmd.Parameters.AddWithValue("@Usuario", cambios.Usuario);
                    logCmd.Parameters.AddWithValue("@Datos", cambios.Datos);
                    logCmd.Parameters.AddWithValue("@FechaCambios", cambios.FechaCambios);

                    logCmd.ExecuteNonQuery();
                }

                return RedirectToAction("Index");
            }

            return View(user);
        }

    }
}
