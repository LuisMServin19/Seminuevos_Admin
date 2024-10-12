using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Serfitex.Models;
using System.Data;
using System.Text;
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

        // Validar sesión
        private bool IsUserAuthenticated(out string username, out string fiperfil)
        {
            username = HttpContext.Session.GetString("username") ?? "";
            fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";
            return !string.IsNullOrEmpty(username) && fiperfil == "1";
        }

        public IActionResult Index()
        {
            if (!IsUserAuthenticated(out string username, out string fiperfil))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:SemiCC"];
            var registros = new List<login_user>();

            using (var conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();
                using (var cmd = new MySqlCommand("SELECT *, CASE WHEN fiperfil = 1 THEN 'Administrativo' WHEN fiperfil = 2 THEN 'Vendedor' END AS tipo_perfil FROM login_user;", conexion))
                {
                    using (var cursor = cmd.ExecuteReader())
                    {
                        while (cursor.Read())
                        {
                            var registro = new login_user
                            {
                                ID_USR = Convert.ToInt32(cursor["ID_USR"]),
                                usr_name = cursor["usr_name"].ToString(),
                                tipo_perfil = cursor["tipo_perfil"].ToString()
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
            if (!IsUserAuthenticated(out _, out _))
                return RedirectToAction("Index", "LogIn");

            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(login_user newUsuario)
        {
            if (!IsUserAuthenticated(out string username, out string fiperfil))
                return RedirectToAction("Index", "LogIn");

            if (ModelState.IsValid)
            {
                string connectionString = Configuration["BDs:SemiCC"];
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    MySqlCommand checkColumnCmd = new MySqlCommand("SELECT COUNT(*) FROM login_user WHERE usr_nick = @usr_nick", conexion);
                    checkColumnCmd.Parameters.AddWithValue("@usr_nick", newUsuario.usr_nick);

                    int userCount = Convert.ToInt32(checkColumnCmd.ExecuteScalar());
                    if (userCount == 0)
                    {
                        MySqlCommand cmd = new MySqlCommand("INSERT INTO login_user (usr_name, usr_nick, usr_pass, fiperfil, usr_active, fecha_alta) VALUES (@usr_name, @usr_nick, @usr_pass, @fiperfil, 1, @fecha_alta)", conexion);
                        cmd.Parameters.AddWithValue("@usr_name", newUsuario.usr_name);
                        cmd.Parameters.AddWithValue("@usr_nick", newUsuario.usr_nick);
                        cmd.Parameters.AddWithValue("@usr_pass", newUsuario.usr_pass);
                        cmd.Parameters.AddWithValue("@fiperfil", newUsuario.fiperfil);
                        cmd.Parameters.AddWithValue("@fecha_alta", DateTime.Now);
                        cmd.ExecuteNonQuery();

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "El nombre de usuario ya existe.");
                    }
                }
            }
            return View(newUsuario);
        }

        // GET: Usuarios/Edit/5
        public IActionResult Edit(int ID_USR)
        {
            if (!IsUserAuthenticated(out _, out _))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:SemiCC"];
            login_user Login_user = null;

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM login_user WHERE ID_USR = @ID_USR", conexion);
                cmd.Parameters.AddWithValue("@ID_USR", ID_USR);

                using (var cursor = cmd.ExecuteReader())
                {
                    if (cursor.Read())
                    {
                        Login_user = new login_user()
                        {
                            ID_USR = Convert.ToInt32(cursor["ID_USR"]),
                            usr_name = cursor["usr_name"].ToString(),
                            usr_nick = cursor["usr_nick"].ToString(),
                            usr_pass = cursor["usr_pass"].ToString(),
                            fiperfil = cursor["fiperfil"].ToString(),
                            usr_active = cursor["usr_active"] != DBNull.Value ? (bool?)Convert.ToBoolean(cursor["usr_active"]) : null
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

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int ID_USR, login_user updatedlogin_user)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (!ModelState.IsValid)
                return View(updatedlogin_user);

            string connectionString = Configuration["BDs:SemiCC"];

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    await conexion.OpenAsync();

                    // Obtener el usuario actual de la base de datos
                    login_user existingUser = null;
                    string selectQuery = "SELECT * FROM login_user WHERE ID_USR = @ID_USR";
                    using (MySqlCommand selectCmd = new MySqlCommand(selectQuery, conexion))
                    {
                        selectCmd.Parameters.AddWithValue("@ID_USR", ID_USR);
                        using (var reader = await selectCmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                existingUser = new login_user()
                                {
                                    ID_USR = Convert.ToInt32(reader["ID_USR"]),
                                    usr_name = reader["usr_name"].ToString(),
                                    usr_nick = reader["usr_nick"].ToString(),
                                    usr_pass = reader["usr_pass"].ToString(),
                                    fiperfil = reader["fiperfil"].ToString(),
                                    usr_active = reader["usr_active"] != DBNull.Value ? (bool?)Convert.ToBoolean(reader["usr_active"]) : null
                                };
                            }
                        }
                    }

                    // Verificar si se encontró el usuario
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Crear una lista de cambios
                    var updates = new List<string>();
                    var parameters = new List<MySqlParameter>();

                    // Comparar los valores y construir la consulta de actualización
                    if (updatedlogin_user.usr_name != existingUser.usr_name)
                    {
                        updates.Add("usr_name = @usr_name");
                        parameters.Add(new MySqlParameter("@usr_name", updatedlogin_user.usr_name));
                    }
                    if (updatedlogin_user.usr_nick != existingUser.usr_nick)
                    {
                        updates.Add("usr_nick = @usr_nick");
                        parameters.Add(new MySqlParameter("@usr_nick", updatedlogin_user.usr_nick));
                    }
                    if (updatedlogin_user.usr_pass != existingUser.usr_pass)
                    {
                        updates.Add("usr_pass = @usr_pass");
                        parameters.Add(new MySqlParameter("@usr_pass", updatedlogin_user.usr_pass));
                    }
                    if (updatedlogin_user.fiperfil != existingUser.fiperfil)
                    {
                        updates.Add("fiperfil = @fiperfil");
                        parameters.Add(new MySqlParameter("@fiperfil", updatedlogin_user.fiperfil));
                    }
                    if (updatedlogin_user.usr_active != existingUser.usr_active)
                    {
                        updates.Add("usr_active = @usr_active");
                        parameters.Add(new MySqlParameter("@usr_active", updatedlogin_user.usr_active));
                    }

                    // Si no hay cambios, redirigir a la vista de índice
                    if (updates.Count == 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }

                    // Construir la consulta de actualización
                    string updateQuery = $"UPDATE login_user SET {string.Join(", ", updates)} WHERE ID_USR = @ID_USR";
                    parameters.Add(new MySqlParameter("@ID_USR", ID_USR));

                    using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conexion))
                    {
                        updateCmd.Parameters.AddRange(parameters.ToArray());
                        await updateCmd.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ocurrió un error al actualizar el usuario: " + ex.Message);
                return View(updatedlogin_user); // Este retorno ya está cubierto, pero aquí se mantiene para mostrar el error
            }

            // Si llegamos aquí, significa que algo falló en el bloque try-catch
            // Asegúrate de devolver una acción en caso de error no manejado
            return View(updatedlogin_user);
        }

    }
}