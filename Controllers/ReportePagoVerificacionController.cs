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
    public class ReportePagoVerificacionController : Controller
    {
        private readonly ILogger<ReportePagoVerificacionController> _logger;
        private readonly IConfiguration Configuration;

        public ReportePagoVerificacionController(ILogger<ReportePagoVerificacionController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }


        public IActionResult Index()
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";


            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");
            if (fiperfil != "1")
                return Redirect("/Unidades/");


            return View(new List<Ta_pago_verificacion>());
        }
        public List<Ta_pago_verificacion> reporte()
        {
            string connectionString = Configuration["BDs:SemiCC"];
            List<Ta_pago_verificacion> registros = new List<Ta_pago_verificacion>();

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;

                string sqlText = "SELECT * FROM Ta_pago_verificacion";

                cmd.CommandText = sqlText;

                using (var cursor = cmd.ExecuteReader())
                {
                    // Aquí ya no se agrega el registro con los títulos
                    while (cursor.Read())
                    {
                        string Modelo = Convert.ToString(cursor["Modelo"]);
                        DateTime Fecha_pago = cursor["Fecha_pago"] != DBNull.Value ? Convert.ToDateTime(cursor["Fecha_pago"]) : DateTime.MinValue;

                        Ta_pago_verificacion registro = new Ta_pago_verificacion()
                        {
                            Modelo = Modelo,
                            Fecha_pago = Fecha_pago,
                        };
                        registros.Add(registro);
                    }
                }
            }

            return registros;
        }

        [HttpPost]
        public async Task<IActionResult> DescargarReporteVerificacion()
        {
            try
            {
                // Obtener todos los registros
                List<Ta_pago_verificacion> registros = reporte();

                StringBuilder constructor = new StringBuilder();
                constructor.AppendLine("Modelo,Fecha Pago");

                foreach (var item in registros)
                {
                    constructor.AppendLine
                    (
                        item.Modelo + "," +
                        item.Fecha_pago.ToString("dd/MM/yyyy")

                    );
                }

                // Ruta del archivo CSV
                string pathTxt = @".\Reporte_Pago_Verificacions_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                System.IO.File.WriteAllText(pathTxt, constructor.ToString());

                var fileName = System.IO.Path.GetFileName(pathTxt);
                var content = await System.IO.File.ReadAllBytesAsync(pathTxt);
                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType);

                return File(content, contentType, fileName);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", new List<Ta_pago_verificacion>());
            }
        }
    }
}