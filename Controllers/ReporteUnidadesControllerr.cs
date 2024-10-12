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
    public class ReporteUnidadesController : Controller
    {
        private readonly ILogger<ReporteUnidadesController> _logger;
        private readonly IConfiguration Configuration;

        public ReporteUnidadesController(ILogger<ReporteUnidadesController> logger, IConfiguration configuration)
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


            return View(new List<ReporteUnidades>());
        }
        public List<ReporteUnidades> reporte()
        {
            string connectionString = Configuration["BDs:SemiCC"];
            List<ReporteUnidades> registros = new List<ReporteUnidades>();

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;

                string sqlText = "SELECT * FROM Unidades WHERE Estatus = 1 ORDER BY Fecha_ingreso;";

                cmd.CommandText = sqlText;

                using (var cursor = cmd.ExecuteReader())
                {
                    ReporteUnidades titulos = new ReporteUnidades()
                    {
                        Modelo = "Modelo",
                        Tipo = "Tipo",
                        Marca = "Marca",
                        Transmision = "Transmision",
                        Num_placa = "Num_placa",
                        Num_serie = "Num_serie",
                        Ano = "Ano",
                        Color = "Color",
                        Seguro = "Seguro",
                        Aseguradora = "Aseguradora",
                        Duplicado_llave = "Duplicado_llave"
                    };

                    registros.Add(titulos);

                    while (cursor.Read())
                    {
                        string Modelo = Convert.ToString(cursor["Modelo"]);
                        string Tipo = Convert.ToString(cursor["Tipo"]);
                        string Marca = Convert.ToString(cursor["Marca"]);
                        string Transmision = Convert.ToString(cursor["Transmision"]);
                        string Num_placa = Convert.ToString(cursor["Num_placa"]);
                        string Num_serie = Convert.ToString(cursor["Num_serie"]);
                        string Ano = Convert.ToString(cursor["Ano"]);
                        string Color = Convert.ToString(cursor["Color"]);
                        string Seguro = Convert.ToString(cursor["Seguro"]);
                        string Aseguradora = Convert.ToString(cursor["Aseguradora"]);
                        string Duplicado_llave = Convert.ToString(cursor["Duplicado_llave"]);

                        ReporteUnidades registro = new ReporteUnidades()
                        {
                            Modelo = Modelo,
                            Tipo = Tipo,
                            Marca = Marca,
                            Transmision = Transmision,
                            Num_placa = Num_placa,
                            Num_serie = Num_serie,
                            Ano = Ano,
                            Color = Color,
                            Seguro = Seguro,
                            Aseguradora = Aseguradora,
                            Duplicado_llave = Duplicado_llave
                        };
                        registros.Add(registro);
                    }
                }
            }

            return registros;
        }

        [HttpPost]
        public async Task<IActionResult> DescargarReporte()
        {
            try
            {
                // Obtener todos los registros
                List<ReporteUnidades> registros = reporte();

                StringBuilder constructor = new StringBuilder();

                foreach (var item in registros)
                {
                    constructor.AppendLine
                    (
                        item.Modelo + "," +
                        item.Tipo + "," +
                        item.Marca + "," +
                        item.Transmision + "," +
                        item.Num_placa + "," +
                        item.Num_serie + "," +
                        item.Ano + "," +
                        item.Color + "," +
                        item.Seguro + "," +
                        item.Aseguradora + "," +
                        item.Duplicado_llave
                    );
                }

                // Ruta del archivo CSV
                string pathTxt = @".\Reporte_unidades_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                System.IO.File.WriteAllText(pathTxt, constructor.ToString());

                var fileName = System.IO.Path.GetFileName(pathTxt);
                var content = await System.IO.File.ReadAllBytesAsync(pathTxt);
                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType);

                return File(content, contentType, fileName);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", new List<ReporteUnidades>());
            }
        }
    }
}