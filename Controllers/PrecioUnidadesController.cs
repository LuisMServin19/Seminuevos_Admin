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


        // GET: Unidades/Gasto
        public IActionResult Gasto(int? id)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");
            if (fiperfil != "1")
                return Redirect("/Unidades/");


            return View();
        }

        // POST: Unidades/Gasto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Gasto(int? id, Unidades newGasto)
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

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conexion;
                    cmd.CommandType = System.Data.CommandType.Text;

                    // Ingresar datos de la unidad sin especificar el Id_unidad
                    cmd.CommandText = "INSERT INTO Ta_gasto (Id_unidad, Gastos, Concepto, Fecha_gasto) " +
                                      "VALUES (@Id_unidad, @Gastos, @Concepto, @Fecha_gasto)";

                    cmd.Parameters.AddWithValue("@Id_unidad", id);
                    cmd.Parameters.AddWithValue("@Gastos", newGasto.Gastos);
                    cmd.Parameters.AddWithValue("@Concepto", newGasto.Concepto);
                    cmd.Parameters.AddWithValue("@Fecha_gasto", DateTime.Now); // Asegúrate de que este es el nombre correcto en la base de datos

                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction("Index");
            }

            return View(newGasto);
        }

    }
}