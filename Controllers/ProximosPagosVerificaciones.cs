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
    public class ProximosPagosVerificacionesController : Controller
    {
        private readonly ILogger<ProximosPagosVerificacionesController> _logger;
        private readonly IConfiguration Configuration;

        public ProximosPagosVerificacionesController(ILogger<ProximosPagosVerificacionesController> logger, IConfiguration configuration)
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

            List<Unidades> verificaciones = new List<Unidades>();

            // Retrieve upcoming verificaciones
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT Id_unidad, Modelo, Marca, Sucursal, Fech_prox_verificacion FROM Unidades WHERE Fech_prox_verificacion > CURDATE() AND Estatus=1 ORDER BY Fech_prox_verificacion;";
                cmd.CommandType = System.Data.CommandType.Text;

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        int Id_unidad = Convert.ToInt32(cursor["Id_unidad"]);
                        string Modelo = Convert.ToString(cursor["Modelo"]) ?? string.Empty;
                        string Marca = Convert.ToString(cursor["Marca"]) ?? string.Empty;
                        string Sucursal = Convert.ToString(cursor["Sucursal"]) ?? string.Empty;
                        DateTime? Fech_prox_verificacion = cursor["Fech_prox_verificacion"] != DBNull.Value ? Convert.ToDateTime(cursor["Fech_prox_verificacion"]) : (DateTime?)null;

                        Unidades verificacion = new Unidades()
                        {
                            Id_unidad = Id_unidad,
                            Modelo = Modelo,
                            Marca = Marca,
                            Sucursal = Sucursal,
                            Fech_prox_verificacion = Fech_prox_verificacion ?? DateTime.MinValue // Manejo de DateTime? con valor por defecto
                        };
                        verificaciones.Add(verificacion);
                    }
                }
            }

            // Pass both lists to the view using ViewData or ViewModel
            ViewData["Verificaciones"] = verificaciones;

            return View();
        }

    }
}