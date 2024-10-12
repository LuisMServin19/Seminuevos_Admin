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
    public class ProximosPagosVerificacionController : Controller
    {
        private readonly ILogger<ProximosPagosVerificacionController> _logger;
        private readonly IConfiguration Configuration;

        public ProximosPagosVerificacionController(ILogger<ProximosPagosVerificacionController> logger, IConfiguration configuration)
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
            List<ProximosPagosVerificacion> verificacion = new List<ProximosPagosVerificacion>();

            // Obtener la fecha actual y calcular el rango de 3 meses
            DateTime fechaActual = DateTime.Today;
            DateTime fechaLimite = fechaActual.AddMonths(4);

            // Recuperar próximas verificacion
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand
                {
                    Connection = conexion,
                    CommandText = "SELECT Id_unidad, Modelo, Marca, Sucursal, Fech_prox_verificacion FROM Unidades WHERE Fech_prox_verificacion > CURDATE() AND Estatus=1 ORDER BY Fech_prox_verificacion;",
                    CommandType = System.Data.CommandType.Text
                };

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        DateTime fechaProxVerificacion = cursor["Fech_prox_verificacion"] != DBNull.Value ? Convert.ToDateTime(cursor["Fech_prox_verificacion"]) : DateTime.MinValue;

                        ProximosPagosVerificacion verificacionItem = new ProximosPagosVerificacion()
                        {
                            Id_unidad = Convert.ToInt32(cursor["Id_unidad"]),
                            Modelo = Convert.ToString(cursor["Modelo"]) ?? string.Empty,
                            Marca = Convert.ToString(cursor["Marca"]) ?? string.Empty,
                            Sucursal = Convert.ToString(cursor["Sucursal"]) ?? string.Empty,
                            Fech_prox_verificacion = fechaProxVerificacion,
                            MostrarBoton = fechaProxVerificacion <= fechaLimite // Determina si mostrar el botón
                        };
                        verificacion.Add(verificacionItem);
                    }
                }
            }

            return View(verificacion);
        }


        public IActionResult RealizarPagoV(int id)
        {
            string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (fiperfil != "1")
                return Redirect("/Unidades/");

            string connectionString = Configuration["BDs:SemiCC"];
            ProximosPagosVerificacion verificacion = new ProximosPagosVerificacion();

            // Obtener los datos de la unidad por Id_unidad
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT Id_unidad, Modelo, Fech_prox_verificacion FROM Unidades WHERE Id_unidad = @Id_unidad";
                cmd.Parameters.AddWithValue("@Id_unidad", id);
                cmd.CommandType = CommandType.Text;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        verificacion.Id_unidad = Convert.ToInt32(reader["Id_unidad"]);
                        verificacion.Modelo = Convert.ToString(reader["Modelo"]);
                        verificacion.Fech_prox_verificacion = reader["Fech_prox_verificacion"] != DBNull.Value ? Convert.ToDateTime(reader["Fech_prox_verificacion"]) : DateTime.MinValue;
                    }
                }
            }
            verificacion.Fecha_pago = DateTime.Now;
            return View(verificacion); // Enviar el modelo a la vista
        }

        [HttpPost]
        public IActionResult GuardarPago(ProximosPagosVerificacion model)
        {
            string connectionString = Configuration["BDs:SemiCC"];

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                // Primer INSERT en Ta_pago_verificacion
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = @"INSERT INTO Ta_pago_verificacion (Id_unidad, Fecha_verificacion, Fecha_pago, Modelo)
                            VALUES (@Id_unidad, @Fecha_verificacion, @Fecha_pago, @Modelo)";
                cmd.Parameters.AddWithValue("@Id_unidad", model.Id_unidad);
                cmd.Parameters.AddWithValue("@Fecha_verificacion", model.Fech_prox_verificacion);
                cmd.Parameters.AddWithValue("@Fecha_pago", model.Fecha_pago); // Este campo se recibe del formulario
                cmd.Parameters.AddWithValue("@Modelo", model.Modelo);
                cmd.ExecuteNonQuery();

                // Segundo UPDATE para actualizar Fech_prox_verificacion en Unidades
                MySqlCommand updateCmd = new MySqlCommand();
                updateCmd.Connection = conexion;
                updateCmd.CommandText = @"UPDATE Unidades 
                                  SET Fech_prox_verificacion = @NuevaFech_prox_verificacion, Fecha_verificacion = @Fecha_pago
                                  WHERE Id_unidad = @Id_unidad";
                updateCmd.Parameters.AddWithValue("@NuevaFech_prox_verificacion", model.Fech_prox_verificacion);
                updateCmd.Parameters.AddWithValue("@Fecha_pago", model.Fecha_pago);
                updateCmd.Parameters.AddWithValue("@Id_unidad", model.Id_unidad);
                updateCmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index"); // Redirigir a la vista de lista después de guardar el pago
        }

    }
}