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

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            return View(new List<ReporteUnidades>());
        }
        public List<ReporteUnidades> reporte(DateTime fechaInicio, DateTime fechaFin)
{
    string connectionString = Configuration["BDs:SemiCC"];
    List<ReporteUnidades> registros = new List<ReporteUnidades>();

    using (MySqlConnection conexion = new MySqlConnection(connectionString))
    {
        conexion.Open();

        MySqlCommand cmd = new MySqlCommand();
        cmd.Connection = conexion;

        // Remover cualquier filtro por id_emp
        string sqlText = "SELECT * FROM Unidades WHERE Fecha_ingreso BETWEEN @fechaInicio AND @fechaFin;";

        cmd.CommandText = sqlText;

        cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@fechaFin", fechaFin.ToString("yyyy-MM-dd"));

        using (var cursor = cmd.ExecuteReader())
        {
            ReporteUnidades titulos = new ReporteUnidades()
            {
                Modelo = "Modelo",
                Marca = "Marca",
                Num_serie = "Num_serie",
                Ano = "Ano"
            };

            registros.Add(titulos);

            while (cursor.Read())
            {
                string Modelo = Convert.ToString(cursor["Modelo"]);
                string Marca = Convert.ToString(cursor["Marca"]);
                string Num_serie = Convert.ToString(cursor["Num_serie"]);
                string Ano = Convert.ToString(cursor["Ano"]);

                ReporteUnidades registro = new ReporteUnidades()
                {
                    Modelo = Modelo,
                    Marca = Marca,
                    Num_serie = Num_serie,
                    Ano = Ano
                };
                registros.Add(registro);
            }
        }
    }

    return registros;
}

[HttpPost]
public async Task<IActionResult> DescargarReporte(DateTime fechaInicio, DateTime fechaFin)
{
    try
    {
        if (fechaInicio == new DateTime() || fechaFin == new DateTime())
            return RedirectToAction("Index", new List<ReporteUnidades>());

        List<ReporteUnidades> registros = reporte(fechaInicio, fechaFin);

        StringBuilder constructor = new StringBuilder();

        foreach (var item in registros)
        {
            constructor.AppendLine
            (
                item.Modelo + "," +
                item.Marca + "," +
                item.Num_serie + "," +
                item.Ano
            );
        }

        string pathTxt = @".\Reporte_" + fechaInicio.ToString("yyyy-MM-dd") + "_" + fechaFin.ToString("yyyy-MM-dd") + ".csv";
        System.IO.File.WriteAllText(pathTxt, constructor.ToString());

        var fileName = System.IO.Path.GetFileName(pathTxt);
        var content = await System.IO.File.ReadAllBytesAsync(pathTxt);
        new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType);

        return File(content, contentType, fileName);
    }
    catch (Exception)
    {
        // Manejo de excepciones
    }

    return RedirectToAction("Index", new List<ReporteUnidades>());
}
    }
}