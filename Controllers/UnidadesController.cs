using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using NuGet.Protocol;
using System.Data;
using System.Text;
using Serfitex.Data;
using Serfitex.Models;
using WebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace Serfitex.Controllers
{
    public class UnidadesController : Controller
    {
        private readonly ILogger<UnidadesController> _logger;
        private readonly IConfiguration Configuration;

        public UnidadesController(ILogger<UnidadesController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        private void LoadViewBagEstatus()
        {
            string connectionString = Configuration["BDs:SemiCC"];
            List<Unidades> registros = new List<Unidades>();

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Unidades", conexion);

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        int Estatus = Convert.ToInt32(cursor["Estatus"]);
                        registros.Add(new Unidades { Estatus = Estatus });
                    }
                }
            }

            ViewBag.Estatus = registros.Select(r => r.Estatus == 1 ? "Disponible" : "Vendido").ToList();
        }

        public IActionResult Index()
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:SemiCC"];

            List<Unidades> registros = new List<Unidades>();

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT * FROM Unidades";
                cmd.CommandType = System.Data.CommandType.Text;

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        int Id_unidad = Convert.ToInt32(cursor["Id_unidad"]);
                        string Modelo = Convert.ToString(cursor["Modelo"]) ?? string.Empty;
                        string Marca = Convert.ToString(cursor["Marca"]) ?? string.Empty;
                        string Num_serie = Convert.ToString(cursor["Num_serie"]) ?? string.Empty;
                        int Estatus = Convert.ToInt32(cursor["Estatus"]);

                        Unidades registro = new Unidades() { Id_unidad = Id_unidad, Modelo = Modelo, Marca = Marca, Num_serie = Num_serie, Estatus = Estatus };
                        registros.Add(registro);
                    }
                }
            }
            LoadViewBagEstatus();
            return View(registros);
        }

        // GET: Unidades/Details/5
        public IActionResult Details(int? id)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "Login");

            if (id == null)
            {
                return NotFound();
            }

            string connectionString = Configuration["BDs:SemiCC"];
            Unidades? unidad = null;

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT * FROM Unidades WHERE Id_unidad = @Id_unidad";
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@Id_unidad", id);

                using (var cursor = cmd.ExecuteReader())
                {
                    if (cursor.Read())
                    {
                        unidad = new Unidades()
                        {
                            Id_unidad = Convert.ToInt32(cursor["Id_unidad"]),
                            Modelo = Convert.ToString(cursor["Modelo"]),
                            Marca = Convert.ToString(cursor["Marca"]),
                            Num_serie = Convert.ToString(cursor["Num_serie"]),
                            Ano = Convert.ToString(cursor["Ano"]),
                            Fecha_factura = Convert.ToDateTime(cursor["Fecha_factura"]),
                            Fecha_tenencia = Convert.ToDateTime(cursor["Fecha_tenencia"]),
                            Seguro = Convert.ToString(cursor["Seguro"]),
                            Comentario = Convert.ToString(cursor["Comentario"]),
                            Estatus = Convert.ToInt32(cursor["Estatus"]),
                            Fecha_ingreso = Convert.ToDateTime(cursor["Fecha_ingreso"]),
                        };
                        unidad.EstatusTexto = unidad.Estatus == 1 ? "Disponible" : "Vendido";
                    }
                }
            }

            if (unidad == null)
            {
                return NotFound();
            }
            return View(unidad);
        }

        // GET: Unidades/Create
        public IActionResult Create()
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            LoadViewBagEstatus();

            return View();
        }

        // POST: Unidades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Unidades newUniddes)
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
                    MySqlCommand checkColumnCmd = new MySqlCommand(string.Format("SELECT * FROM Unidades WHERE Id_unidad='{0}'", newUniddes.Id_unidad), conexion);
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
                    cmd.Parameters.AddWithValue("@Id_unidad", newUniddes.Id_unidad);
                    cmd.Parameters.AddWithValue("@Modelo", newUniddes.Modelo);
                    cmd.Parameters.AddWithValue("@Marca", newUniddes.Marca);
                    cmd.Parameters.AddWithValue("@Num_serie", newUniddes.Num_serie);
                    cmd.Parameters.AddWithValue("@Ano", newUniddes.Ano);
                    cmd.Parameters.AddWithValue("@Fecha_factura", newUniddes.Fecha_factura);
                    cmd.Parameters.AddWithValue("@Fecha_tenencia", newUniddes.Fecha_tenencia);
                    cmd.Parameters.AddWithValue("@Seguro", newUniddes.Seguro);
                    cmd.Parameters.AddWithValue("@Comentario", newUniddes.Comentario);
                    cmd.Parameters.AddWithValue("@Estatus", 1);
                    cmd.Parameters.AddWithValue("@Fecha_ingreso", DateTime.Now);


                    if (!exist)
                    {
                        cmd.CommandText = "INSERT INTO Unidades (Modelo,Marca,Num_serie,Ano,Fecha_factura,Fecha_tenencia,Seguro,Comentario,Estatus,Fecha_ingreso) VALUES (@Modelo,@Marca,@Num_serie,@Ano,@Fecha_factura,@Fecha_tenencia,@Seguro,@Comentario,@Estatus,@Fecha_ingreso)";
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

        // GET: Unidades/Edit/5
        public IActionResult Edit(string id)
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:SemiCC"];

            Unidades? unidades = null;

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT * FROM Unidades WHERE Id_unidad = @Id_unidad";
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@Id_unidad", id);

                using (var cursor = cmd.ExecuteReader())
                {
                    if (cursor.Read())
                    {
                        unidades = new Unidades()
                        {
                            Id_unidad = Convert.ToInt32(cursor["Id_unidad"]),
                            Modelo = Convert.ToString(cursor["Modelo"]),
                            Marca = Convert.ToString(cursor["Marca"]),
                            Num_serie = Convert.ToString(cursor["Num_serie"]),
                            Ano = Convert.ToString(cursor["Ano"]),
                            Fecha_factura = Convert.ToDateTime(cursor["Fecha_factura"]),
                            Fecha_tenencia = Convert.ToDateTime(cursor["Fecha_tenencia"]),
                            Seguro = Convert.ToString(cursor["Seguro"]),
                            Comentario = Convert.ToString(cursor["Comentario"]),
                            Estatus = Convert.ToInt32(cursor["Estatus"]),
                            Fecha_ingreso = Convert.ToDateTime(cursor["Fecha_ingreso"]),

                        };
                    }
                }
            }

            if (unidades == null)
            {
                return NotFound();
            }

            LoadViewBagEstatus();
            return View(unidades);
        }

        // POST: Unidades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Unidades updatedUnidades)
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (!ModelState.IsValid)
                return View(updatedUnidades);

            string connectionString = Configuration["BDs:SemiCC"];

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    string query = "UPDATE Unidades SET Modelo = @Modelo, Marca = @Marca, Num_serie = @Num_serie, Ano = @Ano, Fecha_factura = @Fecha_factura, Fecha_tenencia = @Fecha_tenencia, Seguro = @Seguro, Comentario = @Comentario, Estatus = @Estatus, Fecha_ingreso = @Fecha_ingreso WHERE Id_unidad = @Id_unidad";

                    using (MySqlCommand updateCmd = new MySqlCommand(query, conexion))
                    {
                        updateCmd.Parameters.AddWithValue("@Id_unidad", updatedUnidades.Id_unidad);
                        updateCmd.Parameters.AddWithValue("@Modelo", updatedUnidades.Modelo);
                        updateCmd.Parameters.AddWithValue("@Marca", updatedUnidades.Marca);
                        updateCmd.Parameters.AddWithValue("@Num_serie", updatedUnidades.Num_serie);
                        updateCmd.Parameters.AddWithValue("@Ano", updatedUnidades.Ano);
                        updateCmd.Parameters.AddWithValue("@Fecha_factura", updatedUnidades.Fecha_factura);
                        updateCmd.Parameters.AddWithValue("@Fecha_tenencia", updatedUnidades.Fecha_tenencia);
                        updateCmd.Parameters.AddWithValue("@Seguro", updatedUnidades.Seguro);
                        updateCmd.Parameters.AddWithValue("@Comentario", updatedUnidades.Comentario);
                        updateCmd.Parameters.AddWithValue("@Estatus", updatedUnidades.Estatus);
                        updateCmd.Parameters.AddWithValue("@Fecha_ingreso", updatedUnidades.Fecha_ingreso);

                        updateCmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the contract.");
                return View(updatedUnidades);
            }
        }
    }
}