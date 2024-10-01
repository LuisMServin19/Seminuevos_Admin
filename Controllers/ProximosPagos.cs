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
    public class ProximosPagosController : Controller
    {
        private readonly ILogger<ProximosPagosController> _logger;
        private readonly IConfiguration Configuration;

        public ProximosPagosController(ILogger<ProximosPagosController> logger, IConfiguration configuration)
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

            List<Unidades> tenencias = new List<Unidades>();
            List<Unidades> verificaciones = new List<Unidades>();

            // Retrieve upcoming tenencias
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT Id_unidad, Modelo, Marca, Num_serie, Fech_prox_tenecia FROM Unidades WHERE Fech_prox_tenecia > CURDATE() AND Estatus=1;";
                cmd.CommandType = System.Data.CommandType.Text;

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        int Id_unidad = Convert.ToInt32(cursor["Id_unidad"]);
                        string Modelo = Convert.ToString(cursor["Modelo"]) ?? string.Empty;
                        string Marca = Convert.ToString(cursor["Marca"]) ?? string.Empty;
                        string Num_serie = Convert.ToString(cursor["Num_serie"]) ?? string.Empty;
                        DateTime? Fech_prox_tenecia = cursor["Fech_prox_tenecia"] != DBNull.Value ? Convert.ToDateTime(cursor["Fech_prox_tenecia"]) : (DateTime?)null;

                        Unidades tenencia = new Unidades()
                        {
                            Id_unidad = Id_unidad,
                            Modelo = Modelo,
                            Marca = Marca,
                            Num_serie = Num_serie,
                            Fech_prox_tenecia = Fech_prox_tenecia ?? DateTime.MinValue // Manejo de DateTime? con valor por defecto
                        };
                        tenencias.Add(tenencia);
                    }
                }
            }

            // Retrieve upcoming verificaciones
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT Id_unidad, Modelo, Marca, Num_serie, Fech_prox_verificacion FROM Unidades WHERE Fech_prox_verificacion > CURDATE() AND Estatus=1;";
                cmd.CommandType = System.Data.CommandType.Text;

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        int Id_unidad = Convert.ToInt32(cursor["Id_unidad"]);
                        string Modelo = Convert.ToString(cursor["Modelo"]) ?? string.Empty;
                        string Marca = Convert.ToString(cursor["Marca"]) ?? string.Empty;
                        string Num_serie = Convert.ToString(cursor["Num_serie"]) ?? string.Empty;
                        DateTime? Fech_prox_verificacion = cursor["Fech_prox_verificacion"] != DBNull.Value ? Convert.ToDateTime(cursor["Fech_prox_verificacion"]) : (DateTime?)null;

                        Unidades verificacion = new Unidades()
                        {
                            Id_unidad = Id_unidad,
                            Modelo = Modelo,
                            Marca = Marca,
                            Num_serie = Num_serie,
                            Fech_prox_verificacion = Fech_prox_verificacion ?? DateTime.MinValue // Manejo de DateTime? con valor por defecto
                        };
                        verificaciones.Add(verificacion);
                    }
                }
            }

            // Pass both lists to the view using ViewData or ViewModel
            ViewData["Tenencias"] = tenencias;
            ViewData["Verificaciones"] = verificaciones;

            return View();
        }

    }
}