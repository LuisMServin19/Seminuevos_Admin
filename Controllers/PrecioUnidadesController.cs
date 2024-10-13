using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using Serfitex.Models;
using System.IO;

namespace Serfitex.Controllers
{
    public class PrecioUnidadesController : Controller
    {
        private readonly ILogger<PrecioUnidadesController> _logger;
        private readonly IConfiguration Configuration;

        public PrecioUnidadesController(ILogger<PrecioUnidadesController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IActionResult Index()
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:SemiCC"];

            var registros = new List<Unidades>();

            using (var conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                using (var cmd = new MySqlCommand("SELECT * FROM Unidades WHERE Estatus = 1 ORDER BY Estatus DESC", conexion))
                {
                    using (var cursor = cmd.ExecuteReader())
                    {
                        while (cursor.Read())
                        {
                            var registro = new Unidades
                            {
                                Id_unidad = Convert.ToInt32(cursor["Id_unidad"]),
                                Modelo = cursor["Modelo"].ToString() ?? string.Empty,
                                Sucursal = cursor["Sucursal"].ToString() ?? string.Empty,
                                Precio_compra = cursor["Precio_compra"] != DBNull.Value ? Convert.ToDecimal(cursor["Precio_compra"]) : 0m,
                                Gastos = cursor["Gastos"] != DBNull.Value ? Convert.ToDecimal(cursor["Gastos"]) : 0m,
                                Total_compra_gastos = cursor["Total_compra_gastos"] != DBNull.Value ? Convert.ToDecimal(cursor["Total_compra_gastos"]) : 0m,
                                Precio = cursor["Precio"] != DBNull.Value ? Convert.ToDecimal(cursor["Precio"]) : 0m,
                            };

                            registros.Add(registro);
                        }
                    }
                }
            }
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
            List<Unidades> listaUnidades = new List<Unidades>();

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT * FROM Ta_gastos WHERE Id_unidad = @Id_unidad";
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@Id_unidad", id);

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read()) // Usar while para leer múltiples filas
                    {
                        Unidades unidad = new Unidades()
                        {
                            Concepto = Convert.ToString(cursor["Concepto"]),
                            Gasto = Convert.ToDecimal(cursor["Gasto"]),
                            Fecha_gasto = Convert.ToDateTime(cursor["Fecha_gasto"]),
                            Id_unidad = id.Value
                        };

                        listaUnidades.Add(unidad); // Añadir a la lista
                    }
                }
            }

            if (!listaUnidades.Any()) // Comprobar si la lista está vacía
            {
                return NotFound();
            }

            // Construir la ruta a la imagen para la vista
            ViewBag.ImagePath = $"~/images/Unidades/{id}/Imagen_1.jpg";

            return View(listaUnidades); // Devolver la lista a la vista
        }


        // GET: Unidades/Gasto/5
        public IActionResult Gasto(string id)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");
            if (fiperfil != "1")
                return Redirect("/Unidades/");

            string connectionString = Configuration["BDs:SemiCC"];

            var Ta_gastos = new GastoViewModel();

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                // Consulta para obtener la unidad
                MySqlCommand cmdUnidad = new MySqlCommand();
                cmdUnidad.Connection = conexion;
                cmdUnidad.CommandText = "SELECT * FROM Unidades WHERE Id_unidad = @Id_unidad";
                cmdUnidad.CommandType = System.Data.CommandType.Text;
                cmdUnidad.Parameters.AddWithValue("@Id_unidad", id);

                using (var cursor = cmdUnidad.ExecuteReader())
                {
                    if (cursor.Read())
                    {
                        Ta_gastos.Unidad = new Unidades()
                        {
                            Id_unidad = Convert.ToInt32(cursor["Id_unidad"]),
                            Modelo = Convert.ToString(cursor["Modelo"]),
                        };
                    }
                }
            }

            return View(Ta_gastos);
        }

        // POST: Unidades/Gasto/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Gasto(string id, GastoViewModel Ta_gastos)
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (!ModelState.IsValid)
            {
                return View(Ta_gastos);
            }

            string connectionString = Configuration["BDs:SemiCC"];

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    string insertGastoQuery = "INSERT INTO Ta_gastos (Id_unidad, Fecha_gasto, Gastos, Concepto) VALUES (@Id_unidad, @Fecha_gasto, @Gastos, @Concepto)";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertGastoQuery, conexion))
                    {
                        insertCmd.Parameters.AddWithValue("@Id_unidad", id);
                        insertCmd.Parameters.AddWithValue("@Fecha_gasto", DateTime.Now);
                        insertCmd.Parameters.AddWithValue("@Gastos", Ta_gastos.Gasto.Gastos);
                        insertCmd.Parameters.AddWithValue("@Concepto", Ta_gastos.Gasto.Concepto);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the expense.");
                return View(Ta_gastos);
            }
        }


    }
}