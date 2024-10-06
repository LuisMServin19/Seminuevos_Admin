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
    public class ProximosPagosTenenciasController : Controller
    {
        private readonly ILogger<ProximosPagosTenenciasController> _logger;
        private readonly IConfiguration Configuration;

        public ProximosPagosTenenciasController(ILogger<ProximosPagosTenenciasController> logger, IConfiguration configuration)
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
            List<ProximosPagosTenencias> tenencias = new List<ProximosPagosTenencias>();

            // Retrieve upcoming tenencias
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT Id_unidad, Modelo, Marca, Sucursal, Fech_prox_tenecia FROM Unidades WHERE Fech_prox_tenecia > CURDATE() AND Estatus=1 ORDER BY Fech_prox_tenecia;";
                cmd.CommandType = System.Data.CommandType.Text;

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        ProximosPagosTenencias tenencia = new ProximosPagosTenencias()
                        {
                            Id_unidad = Convert.ToInt32(cursor["Id_unidad"]),
                            Modelo = Convert.ToString(cursor["Modelo"]) ?? string.Empty,
                            Marca = Convert.ToString(cursor["Marca"]) ?? string.Empty,
                            Sucursal = Convert.ToString(cursor["Sucursal"]) ?? string.Empty,
                            Fech_prox_tenecia = cursor["Fech_prox_tenecia"] != DBNull.Value ? Convert.ToDateTime(cursor["Fech_prox_tenecia"]) : DateTime.MinValue
                        };
                        tenencias.Add(tenencia);
                    }
                }
            }

            return View(tenencias);   // Aquí se pasa el modelo 'tenencias' a la vista
        }


        // GET: Unidades/RealizarPagoT/5
        public IActionResult RealizarPagoT(string id)
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:SemiCC"];

            Unidades? unidades = null;
            Ta_pago_tenencia? realizarPagoT = null;

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                // Consulta para obtener la unidad
                MySqlCommand cmdUnidad = new MySqlCommand();
                cmdUnidad.Connection = conexion;
                cmdUnidad.CommandText = "SELECT Id_unidad, Modelo, Marca, Sucursal, Fech_prox_tenecia FROM Unidades WHERE Id_unidad = @Id_unidad AND Estatus=1";
                cmdUnidad.CommandType = System.Data.CommandType.Text;
                cmdUnidad.Parameters.AddWithValue("@Id_unidad", id);

                using (var cursor = cmdUnidad.ExecuteReader())
                {
                    if (cursor.Read())
                    {
                        unidades = new Unidades()
                        {
                            Id_unidad = Convert.ToInt32(cursor["Id_unidad"]),
                            Modelo = Convert.ToString(cursor["Modelo"]),
                            Fech_prox_tenecia = cursor["Fech_prox_tenecia"] != DBNull.Value
                                                  ? Convert.ToDateTime(cursor["Fech_prox_tenecia"])
                                                  : DateTime.MinValue
                        };
                    }
                }

                if (unidades == null)
                {
                    return NotFound();
                }

                // Consulta para obtener la información de RealizarPagoT de Ta_pago_tenencia
                MySqlCommand cmdRealizarPagoT = new MySqlCommand();
                cmdRealizarPagoT.Connection = conexion;
                cmdRealizarPagoT.CommandText = "SELECT * FROM Ta_pago_tenencia WHERE Id_unidad = @Id_unidad";
                cmdRealizarPagoT.CommandType = System.Data.CommandType.Text;
                cmdRealizarPagoT.Parameters.AddWithValue("@Id_unidad", id);

                using (var cursorRealizarPagoT = cmdRealizarPagoT.ExecuteReader())
                {
                    if (cursorRealizarPagoT.Read())
                    {
                        realizarPagoT = new Ta_pago_tenencia()
                        {
                            Id_unidad = Convert.ToInt32(cursorRealizarPagoT["Id_unidad"]),
                            Modelo = cursorRealizarPagoT["Modelo"] != DBNull.Value ? Convert.ToString(cursorRealizarPagoT["Modelo"]) : string.Empty,
                            Fecha_tenencia = cursorRealizarPagoT["Fecha_tenencia"] != DBNull.Value ? Convert.ToDateTime(cursorRealizarPagoT["Fecha_tenencia"]) : (DateTime?)null,
                            Fecha_pago = cursorRealizarPagoT["Fecha_pago"] != DBNull.Value ? Convert.ToDateTime(cursorRealizarPagoT["Fecha_pago"]) : (DateTime?)null,
                        };
                    }
                }
            }

            // Crear el modelo de vista
            var viewModel = new RealizarPagoViewModel
            {
                Unidad = unidades,
                PagoTenencia = realizarPagoT
            };

            return View(viewModel);
        }


        // POST: Unidades/RealizarPagoT/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RealizarPagoT(string id, RealizarPagoViewModel viewModel)
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (!ModelState.IsValid)
            {
                return View(viewModel); // Regresar el viewModel si hay errores de validación
            }

            string connectionString = Configuration["BDs:SemiCC"];

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    // 1. Recuperar los detalles de la unidad antes de la actualización
                    Unidades unidad = null;
                    string selectUnidadQuery = "SELECT Modelo, Fech_prox_tenecia FROM Unidades WHERE Id_unidad = @Id_unidad";
                    using (MySqlCommand selectCmd = new MySqlCommand(selectUnidadQuery, conexion))
                    {
                        selectCmd.Parameters.AddWithValue("@Id_unidad", id);
                        using (var cursorUnidad = selectCmd.ExecuteReader())
                        {
                            if (cursorUnidad.Read())
                            {
                                unidad = new Unidades()
                                {
                                    Modelo = Convert.ToString(cursorUnidad["Modelo"]),
                                    Fech_prox_tenecia = Convert.ToDateTime(cursorUnidad["Fech_prox_tenecia"])  // Valor actual de Fech_prox_tenecia antes de actualizar
                                };
                            }
                        }
                    }

                    if (unidad == null)
                    {
                        return NotFound();
                    }

                    // 2. Insertar en la tabla Ta_pago_tenencia usando el valor actual de Fech_prox_tenecia
                    string insertRealizarPagoTQuery = "INSERT INTO Ta_pago_tenencia (Id_unidad, Fecha_tenencia, Fecha_pago, Modelo) VALUES (@Id_unidad, @Fecha_tenencia, @Fecha_pago, @Modelo)";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertRealizarPagoTQuery, conexion))
                    {
                        insertCmd.Parameters.AddWithValue("@Id_unidad", id);
                        insertCmd.Parameters.AddWithValue("@Fecha_tenencia", unidad.Fech_prox_tenecia);  // Fech_prox_tenecia antes de la actualización
                        insertCmd.Parameters.AddWithValue("@Fecha_pago", viewModel.Fecha_pago);  // El nuevo Fecha_pago
                        insertCmd.Parameters.AddWithValue("@Modelo", unidad.Modelo);  // Modelo
                        insertCmd.ExecuteNonQuery();
                    }

                    // 3. Actualizar Fech_prox_tenecia en la tabla Unidades
                    string updateUnidadesQuery = "UPDATE Unidades SET Fech_prox_tenecia = @Fech_prox_tenecia WHERE Id_unidad = @Id_unidad";
                    using (MySqlCommand updateCmd = new MySqlCommand(updateUnidadesQuery, conexion))
                    {
                        updateCmd.Parameters.AddWithValue("@Id_unidad", id);
                        updateCmd.Parameters.AddWithValue("@Fech_prox_tenecia", viewModel.Fech_prox_tenecia ?? (object)DBNull.Value);  // La nueva Fech_prox_tenecia
                        updateCmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al realizar el pago: " + ex.Message);
                return View(viewModel); // Retornar el viewModel con los errores
            }
        }




        // Pass both lists to the view using ViewData or ViewModel
    }

}