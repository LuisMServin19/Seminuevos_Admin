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

        // GET: Unidades/Create
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

        // POST: Unidades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(login_user newUniddes)
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:SemiCC"];

            if (ModelState.IsValid)
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    bool exist = false;
                    MySqlCommand checkColumnCmd = new MySqlCommand(string.Format("SELECT * FROM login_user WHERE usr_nick='{0}'", newUniddes.usr_nick), conexion);
                    using (MySqlDataReader reader = checkColumnCmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            exist = true;
                        }
                    }

                    if (!exist)
                    {
                        MySqlCommand cmd = new MySqlCommand();
                        cmd.Connection = conexion;
                        cmd.CommandType = System.Data.CommandType.Text;

                        // Ingresar datos de la unidad sin especificar el Id_unidad
                        cmd.CommandText = "INSERT INTO login_user (usr_name, usr_nick, usr_pass, fiperfil, usr_active, fecha_alta) VALUES (@usr_name, @usr_nick, @usr_pass, 1, 1, @fecha_alta)";

                        cmd.Parameters.AddWithValue("@usr_name", newUniddes.usr_name);
                        cmd.Parameters.AddWithValue("@usr_nick", newUniddes.usr_nick);
                        cmd.Parameters.AddWithValue("@usr_pass", newUniddes.usr_pass);
                        cmd.Parameters.AddWithValue("@fecha_alta", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        return View(newUniddes);
                    }
                }
                return RedirectToAction("Index");
            }

            return View(newUniddes);
        }



        // // GET: Unidades/Edit/5
        // public IActionResult Edit(string id)
        // {
        //     string username = HttpContext.Session.GetString("username") ?? "";
        //     string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";


        //     if (string.IsNullOrEmpty(username))
        //         return RedirectToAction("Index", "LogIn");

        //     if (fiperfil != "1")
        //         return Redirect("/Unidades/");

        //     string connectionString = Configuration["BDs:SemiCC"];

        //     Unidades? unidades = null;

        //     using (MySqlConnection conexion = new MySqlConnection(connectionString))
        //     {
        //         conexion.Open();

        //         MySqlCommand cmd = new MySqlCommand();
        //         cmd.Connection = conexion;
        //         cmd.CommandText = "SELECT * FROM Unidades WHERE Id_unidad = @Id_unidad";
        //         cmd.CommandType = System.Data.CommandType.Text;
        //         cmd.Parameters.AddWithValue("@Id_unidad", id);

        //         using (var cursor = cmd.ExecuteReader())
        //         {
        //             if (cursor.Read())
        //             {
        //                 unidades = new Unidades()
        //                 {
        //                     Id_unidad = Convert.ToInt32(cursor["Id_unidad"]),
        //                     Modelo = Convert.ToString(cursor["Modelo"]),
        //                     Tipo = Convert.ToString(cursor["Tipo"]),
        //                     Marca = Convert.ToString(cursor["Marca"]),
        //                     Transmision = Convert.ToString(cursor["Transmision"]),
        //                     Num_placa = Convert.ToString(cursor["Num_placa"]),
        //                     Num_serie = Convert.ToString(cursor["Num_serie"]),
        //                     Ano = Convert.ToInt32(cursor["Ano"]),
        //                     Color = Convert.ToString(cursor["Color"]),
        //                     Fecha_factura = Convert.ToDateTime(cursor["Fecha_factura"]),
        //                     Tipo_factura = Convert.ToString(cursor["Tipo_factura"]),
        //                     Fecha_tenencia = Convert.ToDateTime(cursor["Fecha_tenencia"]),
        //                     Fecha_verificacion = Convert.ToDateTime(cursor["Fecha_verificacion"]),
        //                     Seguro = Convert.ToString(cursor["Seguro"]),
        //                     Aseguradora = Convert.ToString(cursor["Aseguradora"]),
        //                     Duplicado_llave = Convert.ToString(cursor["Duplicado_llave"]),
        //                     Comentario = Convert.ToString(cursor["Comentario"]),
        //                     Precio = Convert.ToInt32(cursor["Precio"]),
        //                     Sucursal = Convert.ToString(cursor["Sucursal"]),
        //                     Fecha_ingreso = Convert.ToDateTime(cursor["Fecha_ingreso"]),
        //                     Fech_prox_tenecia = Convert.ToDateTime(cursor["Fech_prox_tenecia"]),
        //                     Fech_prox_verificacion = Convert.ToDateTime(cursor["Fech_prox_verificacion"]),

        //                 };
        //             }
        //         }
        //     }

        //     if (unidades == null)
        //     {
        //         return NotFound();
        //     }

        //     return View(unidades);
        // }

        // // POST: Unidades/Edit/5
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Edit(int id, Unidades updatedUnidades, IFormFile? ImageFile)
        // {
        //     string username = HttpContext.Session.GetString("username") ?? "";

        //     if (string.IsNullOrEmpty(username))
        //         return RedirectToAction("Index", "Login");

        //     if (!ModelState.IsValid)
        //         return View(updatedUnidades);

        //     string connectionString = Configuration["BDs:SemiCC"];

        //     try
        //     {
        //         using (MySqlConnection conexion = new MySqlConnection(connectionString))
        //         {
        //             await conexion.OpenAsync();

        //             // Actualizar los datos de la unidad
        //             string query = @"UPDATE Unidades 
        //                      SET Modelo = @Modelo, Tipo = @Tipo, Marca = @Marca, Transmision = @Transmision, 
        //                          Num_placa = @Num_placa, Num_serie = @Num_serie, Ano = @Ano, Color = @Color, 
        //                          Fecha_factura = @Fecha_factura, Tipo_factura = @Tipo_factura, 
        //                          Fecha_tenencia = @Fecha_tenencia, Fecha_verificacion = @Fecha_verificacion, 
        //                          Seguro = @Seguro, Aseguradora = @Aseguradora, Duplicado_llave = @Duplicado_llave, 
        //                          Comentario = @Comentario, Precio = @Precio, Sucursal = @Sucursal, 
        //                          Estatus = @Estatus, Fecha_ingreso = @Fecha_ingreso, 
        //                          Fech_prox_tenecia = @Fech_prox_tenecia, Fech_prox_verificacion = @Fech_prox_verificacion 
        //                      WHERE Id_unidad = @Id_unidad";

        //             using (MySqlCommand updateCmd = new MySqlCommand(query, conexion))
        //             {
        //                 updateCmd.Parameters.AddWithValue("@Id_unidad", updatedUnidades.Id_unidad);
        //                 updateCmd.Parameters.AddWithValue("@Modelo", updatedUnidades.Modelo);
        //                 updateCmd.Parameters.AddWithValue("@Tipo", updatedUnidades.Tipo);
        //                 updateCmd.Parameters.AddWithValue("@Marca", updatedUnidades.Marca);
        //                 updateCmd.Parameters.AddWithValue("@Transmision", updatedUnidades.Transmision);
        //                 updateCmd.Parameters.AddWithValue("@Num_placa", updatedUnidades.Num_placa);
        //                 updateCmd.Parameters.AddWithValue("@Num_serie", updatedUnidades.Num_serie);
        //                 updateCmd.Parameters.AddWithValue("@Ano", updatedUnidades.Ano);
        //                 updateCmd.Parameters.AddWithValue("@Color", updatedUnidades.Color);
        //                 updateCmd.Parameters.AddWithValue("@Fecha_factura", updatedUnidades.Fecha_factura);
        //                 updateCmd.Parameters.AddWithValue("@Tipo_factura", updatedUnidades.Tipo_factura);
        //                 updateCmd.Parameters.AddWithValue("@Fecha_tenencia", updatedUnidades.Fecha_tenencia);
        //                 updateCmd.Parameters.AddWithValue("@Fecha_verificacion", updatedUnidades.Fecha_verificacion);
        //                 updateCmd.Parameters.AddWithValue("@Seguro", updatedUnidades.Seguro);
        //                 updateCmd.Parameters.AddWithValue("@Aseguradora", updatedUnidades.Aseguradora);
        //                 updateCmd.Parameters.AddWithValue("@Duplicado_llave", updatedUnidades.Duplicado_llave);
        //                 updateCmd.Parameters.AddWithValue("@Comentario", updatedUnidades.Comentario);
        //                 updateCmd.Parameters.AddWithValue("@Precio", updatedUnidades.Precio);
        //                 updateCmd.Parameters.AddWithValue("@Sucursal", updatedUnidades.Sucursal);
        //                 updateCmd.Parameters.AddWithValue("@Estatus", 1);
        //                 updateCmd.Parameters.AddWithValue("@Fecha_ingreso", updatedUnidades.Fecha_ingreso);
        //                 updateCmd.Parameters.AddWithValue("@Fech_prox_tenecia", updatedUnidades.Fech_prox_tenecia);
        //                 updateCmd.Parameters.AddWithValue("@Fech_prox_verificacion", updatedUnidades.Fech_prox_verificacion);

        //                 await updateCmd.ExecuteNonQueryAsync();
        //             }

        //             // Si se cargó una imagen, guardarla en la ruta correspondiente
        //             if (ImageFile != null && ImageFile.Length > 0)
        //             {
        //                 string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Unidades", updatedUnidades.Id_unidad.ToString());
        //                 string filePath = Path.Combine(folderPath, "Imagen_1.jpg");

        //                 // Crear la carpeta si no existe
        //                 if (!Directory.Exists(folderPath))
        //                 {
        //                     Directory.CreateDirectory(folderPath);
        //                 }

        //                 // Guardar la imagen como Imagen_1.jpg
        //                 using (var stream = new FileStream(filePath, FileMode.Create))
        //                 {
        //                     await ImageFile.CopyToAsync(stream);
        //                 }
        //             }
        //         }

        //         return RedirectToAction(nameof(Index));
        //     }
        //     catch (Exception ex)
        //     {
        //         ModelState.AddModelError("", "Ocurrió un error al actualizar la unidad: " + ex.Message);
        //         return View(updatedUnidades);
        //     }
        // }

    }
}