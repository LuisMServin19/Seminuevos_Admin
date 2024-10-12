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
            string usr_active = HttpContext.Session.GetString("1") ?? "";
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            else if (usr_active != "1" )
        {
            return Redirect("/Unidades/");
        }

            string connectionString = Configuration["BDs:SemiCC"];
            List<ProximosPagosTenencias> tenencias = new List<ProximosPagosTenencias>();

            // Obtener la fecha actual y calcular el rango de 3 meses
            DateTime fechaActual = DateTime.Today;
            DateTime fechaLimite = fechaActual.AddMonths(4);

            // Recuperar próximas tenencias
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand
                {
                    Connection = conexion,
                    CommandText = "SELECT Id_unidad, Modelo, Marca, Sucursal, Fech_prox_tenecia FROM Unidades WHERE Fech_prox_tenecia > CURDATE() AND Estatus=1 ORDER BY Fech_prox_tenecia;",
                    CommandType = System.Data.CommandType.Text
                };

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        DateTime fechaProxTenencia = cursor["Fech_prox_tenecia"] != DBNull.Value ? Convert.ToDateTime(cursor["Fech_prox_tenecia"]) : DateTime.MinValue;

                        ProximosPagosTenencias tenencia = new ProximosPagosTenencias()
                        {
                            Id_unidad = Convert.ToInt32(cursor["Id_unidad"]),
                            Modelo = Convert.ToString(cursor["Modelo"]) ?? string.Empty,
                            Marca = Convert.ToString(cursor["Marca"]) ?? string.Empty,
                            Sucursal = Convert.ToString(cursor["Sucursal"]) ?? string.Empty,
                            Fech_prox_tenecia = fechaProxTenencia,
                            MostrarBoton = fechaProxTenencia <= fechaLimite // Determina si mostrar el botón
                        };
                        tenencias.Add(tenencia);
                    }
                }
            }

            return View(tenencias);
        }


        public IActionResult RealizarPagoT(int id)
        {
            string connectionString = Configuration["BDs:SemiCC"];
            ProximosPagosTenencias tenencia = new ProximosPagosTenencias();

            // Obtener los datos de la unidad por Id_unidad
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT Id_unidad, Modelo, Fech_prox_tenecia FROM Unidades WHERE Id_unidad = @Id_unidad";
                cmd.Parameters.AddWithValue("@Id_unidad", id);
                cmd.CommandType = CommandType.Text;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        tenencia.Id_unidad = Convert.ToInt32(reader["Id_unidad"]);
                        tenencia.Modelo = Convert.ToString(reader["Modelo"]);
                        tenencia.Fech_prox_tenecia = reader["Fech_prox_tenecia"] != DBNull.Value ? Convert.ToDateTime(reader["Fech_prox_tenecia"]) : DateTime.MinValue;
                    }
                }
            }
            tenencia.Fecha_pago = DateTime.Now;
            return View(tenencia); // Enviar el modelo a la vista
        }

        [HttpPost]
        public IActionResult GuardarPago(ProximosPagosTenencias model)
        {
            string connectionString = Configuration["BDs:SemiCC"];

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                // Primer INSERT en Ta_pago_tenencia
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = @"INSERT INTO Ta_pago_tenencia (Id_unidad, Fecha_tenencia, Fecha_pago, Modelo)
                            VALUES (@Id_unidad, @Fecha_tenencia, @Fecha_pago, @Modelo)";
                cmd.Parameters.AddWithValue("@Id_unidad", model.Id_unidad);
                cmd.Parameters.AddWithValue("@Fecha_tenencia", model.Fech_prox_tenecia);
                cmd.Parameters.AddWithValue("@Fecha_pago", model.Fecha_pago); // Este campo se recibe del formulario
                cmd.Parameters.AddWithValue("@Modelo", model.Modelo);
                cmd.ExecuteNonQuery();

                // Segundo UPDATE para actualizar Fech_prox_tenencia en Unidades
                MySqlCommand updateCmd = new MySqlCommand();
                updateCmd.Connection = conexion;
                updateCmd.CommandText = @"UPDATE Unidades 
                                  SET Fech_prox_tenecia = @NuevaFech_prox_tenencia 
                                  WHERE Id_unidad = @Id_unidad";
                updateCmd.Parameters.AddWithValue("@NuevaFech_prox_tenencia", model.Fech_prox_tenecia);
                updateCmd.Parameters.AddWithValue("@Id_unidad", model.Id_unidad);
                updateCmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index"); // Redirigir a la vista de lista después de guardar el pago
        }


    }
}