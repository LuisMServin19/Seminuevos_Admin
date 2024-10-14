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
    public class GastosController : Controller
    {
        private readonly ILogger<GastosController> _logger;
        private readonly IConfiguration Configuration;

        public GastosController(ILogger<GastosController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }



        public IActionResult Index()
        {
            string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (fiperfil != "1")
                return Redirect("/Unidades/");

            string connectionString = Configuration["BDs:SemiCC"];
            List<Ta_gastos> gatosss = new List<Ta_gastos>();


            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand
                {
                    Connection = conexion,
                    CommandText = "SELECT Id_unidad,Modelo,Sucursal,Precio_compra,Gastos,Total_compra_gastos,Precio from unidades where Estatus =1;",
                    CommandType = System.Data.CommandType.Text
                };

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {

                        Ta_gastos gatoss = new Ta_gastos()
                        {
                            Id_unidad = Convert.ToInt32(cursor["Id_unidad"]),
                            Modelo = Convert.ToString(cursor["Modelo"]) ?? string.Empty,
                            Sucursal = Convert.ToString(cursor["Sucursal"]) ?? string.Empty,
                            Precio_compra = cursor["Precio_compra"] != DBNull.Value ? Convert.ToDecimal(cursor["Precio_compra"]) : 0m,
                            Gastos = cursor["Gastos"] != DBNull.Value ? Convert.ToDecimal(cursor["Gastos"]) : 0m,
                            Total_compra_gastos = cursor["Total_compra_gastos"] != DBNull.Value ? Convert.ToDecimal(cursor["Total_compra_gastos"]) : 0m,
                            Precio = cursor["Precio"] != DBNull.Value ? Convert.ToDecimal(cursor["Precio"]) : 0m,

                        };
                        gatosss.Add(gatoss);
                    }
                }
            }

            return View(gatosss);
        }

        // GET: Gastos/Agregar Gasto
        public IActionResult Gasto(int id)
        {
            string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (fiperfil != "1")
                return Redirect("/Unidades/");

            string connectionString = Configuration["BDs:SemiCC"];
            Ta_gastos gatosss = new Ta_gastos();

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT * FROM Ta_gastos WHERE Id_unidad = @Id_unidad";
                cmd.Parameters.AddWithValue("@Id_unidad", id);
                cmd.CommandType = CommandType.Text;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        gatosss.Id_unidad = Convert.ToInt32(reader["Id_unidad"]);
                    }
                }
            }
            gatosss.Id_unidad = id;
            return View(gatosss);
        }

        // POST: Gastos/Agregar Gasto
        [HttpPost]
        public IActionResult GuardarGasto(Ta_gastos model)
        {
            string connectionString = Configuration["BDs:SemiCC"];
            Console.WriteLine("Id_unidad recibido: " + model.Id_unidad);

            if (model.Id_unidad == 0)
            {
                ModelState.AddModelError("", "El Id_unidad no es válido.");
                return View(model);
            }

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                // Primer INSERT en Ta_pago_gatosss
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = @"INSERT INTO Ta_gastos (Id_unidad, Concepto, Gasto, Fecha_gasto)
                            VALUES (@Id_unidad, @Concepto, @Gasto, @Fecha_gasto)";
                cmd.Parameters.AddWithValue("@Id_unidad", model.Id_unidad);
                cmd.Parameters.AddWithValue("@Concepto", model.Concepto);
                cmd.Parameters.AddWithValue("@Gasto", model.Gastos);
                cmd.Parameters.AddWithValue("@Fecha_gasto", DateTime.Now);
                cmd.ExecuteNonQuery();

            }

            return RedirectToAction("Index");
        }

        // GET: Gastos/Details/
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
            List<Ta_gastos> gastos = new List<Ta_gastos>();

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT * FROM Ta_gastos WHERE Id_unidad = @Id_unidad";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@Id_unidad", id);

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        var gasto = new Ta_gastos()
                        {
                            Id_unidad = Convert.ToInt32(cursor["Id_unidad"]),
                            Concepto = Convert.ToString(cursor["Concepto"]),
                            Gasto = Convert.ToDecimal(cursor["Gasto"]),
                            Fecha_gasto = Convert.ToDateTime(cursor["Fecha_gasto"]),
                        };

                        // Genera la ruta de la imagen por cada gasto
                        string imagePath = $"~/images/Unidades/{gasto.Id_unidad}/Imagen_1.jpg";
                        if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.Substring(2))))
                        {
                            gasto.ImagePath = imagePath;
                        }
                        else
                        {
                            gasto.ImagePath = null; // Sin imagen disponible
                        }

                        gastos.Add(gasto);
                    }
                }
            }

            if (gastos == null || !gastos.Any())
            {
                return NotFound();
            }

            return View(gastos);
        }




    }
}