using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using Serfitex.Data;
using Serfitex.Models;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Text;
using WebApp.Models;
using MySqlX.XDevAPI;
using NuevoBanorte.Models;
using NuGet.Protocol;

namespace Serfitex.Controllers
{
    public class login_customerController : Controller
    {
        private readonly ILogger<login_customerController> _logger;
        private readonly IConfiguration Configuration;

        public login_customerController(ILogger<login_customerController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }


        public IActionResult Index()
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string cliente = HttpContext.Session.GetString("conexion") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:" + cliente];


            List<login_customer> registros = new List<login_customer>();

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT LOG_NAME,LOG_ACTIVE,ID_LOG FROM login_customer ORDER BY ID_LOG DESC";
                cmd.CommandType = System.Data.CommandType.Text;

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        int ID_LOG = Convert.ToInt32(cursor["ID_LOG"]);
                        string LOG_NAME = Convert.ToString(cursor["LOG_NAME"]);
                        bool LOG_ACTIVE = Convert.ToBoolean(cursor["LOG_ACTIVE"]);


                        login_customer registro = new login_customer() { ID_LOG = ID_LOG, LOG_NAME = LOG_NAME, LOG_ACTIVE = LOG_ACTIVE };
                        registros.Add(registro);
                    }
                }
            }
            return View(registros);
        }

        // GET: login_customer/Create
        public IActionResult Create()
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            return View();
        }

        // POST: login_customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(login_customer newBranch)
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

                    MySqlCommand getLastCodeCmd = new MySqlCommand("SELECT MAX(ID_LOG) FROM login_customer", conexion);
                    int lastCode = Convert.ToInt16(getLastCodeCmd.ExecuteScalar());
                    int newCode = lastCode + 1;

                    MySqlCommand getLastCodeCmdd = new MySqlCommand("SELECT ID_CUS FROM login_customer ORDER BY ID_LOG DESC LIMIT 1", conexion);
                    int ID_CUS = Convert.ToInt16(getLastCodeCmdd.ExecuteScalar());

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conexion;
                    cmd.CommandText = "INSERT INTO login_customer (ID_LOG,ID_CUS, LOG_NICK, LOG_PASS, LOG_NAME,LOG_ACTIVE,LOG_CDATE) values (@ID_LOG,@ID_CUS,@LOG_NICK,sha1(@log_pass),@LOG_NAME,1,@LOG_CDATE)";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@ID_LOG", newCode);
                    cmd.Parameters.AddWithValue("@ID_CUS", ID_CUS);
                    cmd.Parameters.AddWithValue("@LOG_NICK", newBranch.LOG_NICK);
                    cmd.Parameters.AddWithValue("@LOG_PASS", newBranch.LOG_PASS);
                    cmd.Parameters.AddWithValue("@LOG_NAME", newBranch.LOG_NAME);
                    cmd.Parameters.AddWithValue("@LOG_ACTIVE", newBranch.LOG_ACTIVE);
                    cmd.Parameters.AddWithValue("@LOG_CDATE", DateTime.Now);

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

        // GET: login_customer/Edit/5
        public IActionResult Edit(int id)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string cliente = HttpContext.Session.GetString("conexion") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:" + cliente];
            login_customer registro = null;

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT ID_LOG, LOG_NICK, LOG_NAME, LOG_ACTIVE FROM login_customer WHERE ID_LOG = @ID_LOG";
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@ID_LOG", id);

                using (var cursor = cmd.ExecuteReader())
                {
                    if (cursor.Read())
                    {
                        registro = new login_customer()
                        {
                            ID_LOG = Convert.ToInt32(cursor["ID_LOG"]),
                            LOG_NICK = Convert.ToString(cursor["LOG_NICK"]),
                            LOG_NAME = Convert.ToString(cursor["LOG_NAME"]),
                            LOG_ACTIVE = Convert.ToBoolean(cursor["LOG_ACTIVE"])
                        };
                    }
                }
            }

            if (registro == null)
            {
                return NotFound();
            }
            ViewData["StatusOptions"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "true", Text = "Activo" },
                new SelectListItem { Value = "false", Text = "Desactivado" }
            };

            return View(registro);
        }

        // POST: login_customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, login_customer user)
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
                    fetchCmd.CommandText = "SELECT ID_LOG, LOG_NICK, LOG_PASS, LOG_NAME, LOG_ACTIVE FROM login_customer WHERE ID_LOG = @ID_LOG";
                    fetchCmd.CommandType = System.Data.CommandType.Text;
                    fetchCmd.Parameters.AddWithValue("@ID_LOG", id);

                    login_customer existingUser = null;
                    using (var cursor = fetchCmd.ExecuteReader())
                    {
                        if (cursor.Read())
                        {
                            existingUser = new login_customer()
                            {
                                ID_LOG = Convert.ToInt32(cursor["ID_LOG"]),
                                LOG_NICK = Convert.ToString(cursor["LOG_NICK"]),
                                LOG_PASS = Convert.ToString(cursor["LOG_PASS"]),
                                LOG_NAME = Convert.ToString(cursor["LOG_NAME"]),
                                LOG_ACTIVE = Convert.ToBoolean(cursor["LOG_ACTIVE"])
                            };
                        }
                    }

                    if (string.IsNullOrEmpty(user.LOG_PASS))
                    {
                        user.LOG_PASS = existingUser.LOG_PASS;
                    }
                    else
                    {
                        user.LOG_PASS = GetSHA1Hash(user.LOG_PASS);
                    }

                    MySqlCommand updateCmd = new MySqlCommand();
                    updateCmd.Connection = conexion;
                    updateCmd.CommandText = "UPDATE login_customer SET LOG_NICK = @LOG_NICK, LOG_PASS = @LOG_PASS, LOG_NAME = @LOG_NAME, LOG_ACTIVE = @LOG_ACTIVE WHERE ID_LOG = @ID_LOG";
                    updateCmd.CommandType = System.Data.CommandType.Text;
                    updateCmd.Parameters.AddWithValue("@ID_LOG", id);
                    updateCmd.Parameters.AddWithValue("@LOG_NICK", user.LOG_NICK);
                    updateCmd.Parameters.AddWithValue("@LOG_PASS", user.LOG_PASS);
                    updateCmd.Parameters.AddWithValue("@LOG_NAME", user.LOG_NAME);
                    updateCmd.Parameters.AddWithValue("@LOG_ACTIVE", user.LOG_ACTIVE);

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

        private string GetSHA1Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}