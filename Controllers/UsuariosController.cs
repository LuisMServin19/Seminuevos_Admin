using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using Serfitex.Models;
using System.IO;
using WebApp.Models;

namespace Serfitex.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ILogger<UsuariosController> _logger;
        private readonly IConfiguration Configuration;

        public UsuariosController(ILogger<UsuariosController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IActionResult Index()
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";


            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (fiperfil != "1")
                return Redirect("/Unidades/");

            string connectionString = Configuration["BDs:SemiCC"];

            var registros = new List<login_user>();

            using (var conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                using (var cmd = new MySqlCommand("SELECT *,CASE WHEN fiperfil = 1 THEN 'Administrativo' WHEN fiperfil = 2 THEN 'Vendedor' END AS tipo_perfil FROM login_user;", conexion))
                {
                    using (var cursor = cmd.ExecuteReader())
                    {
                        while (cursor.Read())
                        {
                            var registro = new login_user
                            {
                                usr_name = cursor["usr_name"].ToString() ?? string.Empty,
                                tipo_perfil = cursor["tipo_perfil"].ToString() ?? string.Empty
                            };

                            registros.Add(registro);
                        }
                    }
                }
            }
            return View(registros);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");
            if (fiperfil != "1")
                return Redirect("/Unidades/");



            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(login_user newUsuario)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";


            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (fiperfil != "1")
                return Redirect("/Unidades/");



            string connectionString = Configuration["BDs:SemiCC"];

            if (ModelState.IsValid)
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    bool exist = false;
                    MySqlCommand checkColumnCmd = new MySqlCommand("SELECT * FROM login_user WHERE usr_nick = @usr_nick", conexion);
                    checkColumnCmd.Parameters.AddWithValue("@usr_nick", newUsuario.usr_nick);

                    using (MySqlDataReader reader = checkColumnCmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            exist = true;
                        }
                    }

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conexion;

                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@usr_name", newUsuario.usr_name);
                    cmd.Parameters.AddWithValue("@usr_nick", newUsuario.usr_nick);
                    cmd.Parameters.AddWithValue("@usr_pass", newUsuario.usr_pass);
                    cmd.Parameters.AddWithValue("@fiperfil", newUsuario.fiperfil);
                    cmd.Parameters.AddWithValue("@usr_active", 1);
                    cmd.Parameters.AddWithValue("@fecha_alta", DateTime.Now);

                    if (!exist)
                    {
                        cmd.CommandText = "INSERT INTO login_user (usr_name,usr_nick,usr_pass,fiperfil,usr_active,fecha_alta) " +
                                          "VALUES (@usr_name,@usr_nick,@usr_pass,@fiperfil,@usr_active,@fecha_alta)";
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        return View(newUsuario);
                    }
                }
                return RedirectToAction("Index");
            }

            return View(newUsuario);
        }



        // GET: Unidades/Edit/5
        public IActionResult Edit(string ID_USR)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";


            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (fiperfil != "1")
                return Redirect("/Unidades/");

            string connectionString = Configuration["BDs:SemiCC"];

            login_user? Login_user = null;

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT * FROM login_user WHERE ID_USR = @ID_USR";
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@ID_USR", ID_USR);

                using (var cursor = cmd.ExecuteReader())
                {
                    if (cursor.Read())
                    {
                        Login_user = new login_user()
                        {
                            usr_name = Convert.ToString(cursor["usr_name"]),

                        };
                    }
                }
            }

            if (Login_user == null)
            {
                return NotFound();
            }

            return View(Login_user);
        }

        // POST: Unidades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int ID_USR, login_user updatedlogin_user)
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "Login");

            if (!ModelState.IsValid)
                return View(updatedlogin_user);

            string connectionString = Configuration["BDs:SemiCC"];

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    await conexion.OpenAsync();

                    // Actualizar los datos de la unidad
                    string query = @"UPDATE login_user 
                             SET usr_name = @usr_name, Tipo
                             WHERE ID_USR = @ID_USR";

                    using (MySqlCommand updateCmd = new MySqlCommand(query, conexion))
                    {
                        updateCmd.Parameters.AddWithValue("@usr_name", updatedlogin_user.usr_name);
                        updateCmd.Parameters.AddWithValue("@ID_USR", updatedlogin_user.ID_USR);

                        await updateCmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ocurrió un error al actualizar la unidad: " + ex.Message);
                return View(updatedlogin_user);
            }
        }
    }
}