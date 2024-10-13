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
            Unidades? unidad = null;

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
                    if (cursor.Read())
                    {
                        unidad = new Unidades()
                        {
                            Concepto = Convert.ToString(cursor["Concepto"]),
                            Gasto = Convert.ToDecimal(cursor["Gasto"]),
                            Fecha_gasto = Convert.ToDateTime(cursor["Fecha_gasto"]),
                        };

                        // Construir la ruta a la imagen "~/images/LogoCMG.png"
                        string imagePath = $"~/images/Unidades/{unidad.Id_unidad}/Imagen_1.jpg";
                        ViewBag.ImagePath = imagePath;

                    }
                }
            }

            if (unidad == null)
            {
                return NotFound();
            }
            return View(unidad);
        }

        // // GET: Unidades/Create
        // public IActionResult Create()
        // {
        //     string username = HttpContext.Session.GetString("username") ?? "";
        //     string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";

        //     if (string.IsNullOrEmpty(username))
        //         return RedirectToAction("Index", "LogIn");
        //     if (fiperfil != "1")
        //         return Redirect("/Unidades/");



        //     return View();
        // }

        // // POST: Unidades/Create
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public IActionResult Create(Unidades newUniddes)
        // {
        //     string username = HttpContext.Session.GetString("username") ?? "";

        //     if (string.IsNullOrEmpty(username))
        //         return RedirectToAction("Index", "LogIn");

        //     string connectionString = Configuration["BDs:SemiCC"];

        //     if (ModelState.IsValid)
        //     {
        //         using (MySqlConnection conexion = new MySqlConnection(connectionString))
        //         {
        //             conexion.Open();

        //             bool exist = false;
        //             MySqlCommand checkColumnCmd = new MySqlCommand(string.Format("SELECT * FROM Unidades WHERE Num_serie='{0}'", newUniddes.Num_serie), conexion);
        //             using (MySqlDataReader reader = checkColumnCmd.ExecuteReader())
        //             {
        //                 if (reader.HasRows)
        //                 {
        //                     exist = true;
        //                 }
        //             }

        //             if (!exist)
        //             {
        //                 MySqlCommand cmd = new MySqlCommand();
        //                 cmd.Connection = conexion;
        //                 cmd.CommandType = System.Data.CommandType.Text;

        //                 // Ingresar datos de la unidad sin especificar el Id_unidad
        //                 cmd.CommandText = "INSERT INTO Unidades (Modelo,Tipo,Marca,Transmision,Num_placa,Num_serie,Ano,Color,Fecha_factura,Tipo_factura,Fecha_tenencia,Fecha_verificacion,Seguro,Aseguradora,Duplicado_llave,Comentario,Precio,Sucursal,Estatus,Fecha_ingreso,Fech_prox_tenecia,Fech_prox_verificacion) " +
        //                                   "VALUES (@Modelo,@Tipo,@Marca,@Transmision,@Num_placa,@Num_serie,@Ano,@Color,@Fecha_factura,@Tipo_factura,@Fecha_tenencia,@Fecha_verificacion,@Seguro,@Aseguradora,@Duplicado_llave,@Comentario,@Precio,@Sucursal,@Estatus,@Fecha_ingreso,@Fech_prox_tenecia,@Fech_prox_verificacion)";

        //                 cmd.Parameters.AddWithValue("@Modelo", newUniddes.Modelo);
        //                 cmd.Parameters.AddWithValue("@Tipo", newUniddes.Tipo);
        //                 cmd.Parameters.AddWithValue("@Marca", newUniddes.Marca);
        //                 cmd.Parameters.AddWithValue("@Transmision", newUniddes.Transmision);
        //                 cmd.Parameters.AddWithValue("@Num_placa", newUniddes.Num_placa);
        //                 cmd.Parameters.AddWithValue("@Num_serie", newUniddes.Num_serie);
        //                 cmd.Parameters.AddWithValue("@Ano", newUniddes.Ano);
        //                 cmd.Parameters.AddWithValue("@Color", newUniddes.Color);
        //                 cmd.Parameters.AddWithValue("@Fecha_factura", newUniddes.Fecha_factura);
        //                 cmd.Parameters.AddWithValue("@Tipo_factura", newUniddes.Tipo_factura);
        //                 cmd.Parameters.AddWithValue("@Fecha_tenencia", newUniddes.Fecha_tenencia);
        //                 cmd.Parameters.AddWithValue("@Fecha_verificacion", newUniddes.Fecha_verificacion);
        //                 cmd.Parameters.AddWithValue("@Seguro", newUniddes.Seguro);
        //                 cmd.Parameters.AddWithValue("@Aseguradora", newUniddes.Aseguradora);
        //                 cmd.Parameters.AddWithValue("@Duplicado_llave", newUniddes.Duplicado_llave);
        //                 cmd.Parameters.AddWithValue("@Comentario", newUniddes.Comentario);
        //                 cmd.Parameters.AddWithValue("@Precio", newUniddes.Precio);
        //                 cmd.Parameters.AddWithValue("@Sucursal", newUniddes.Sucursal);
        //                 cmd.Parameters.AddWithValue("@Estatus", 1);
        //                 cmd.Parameters.AddWithValue("@Fecha_ingreso", DateTime.Now);
        //                 cmd.Parameters.AddWithValue("@Fech_prox_tenecia", newUniddes.Fech_prox_tenecia);
        //                 cmd.Parameters.AddWithValue("@Fech_prox_verificacion", newUniddes.Fech_prox_verificacion);

        //                 cmd.ExecuteNonQuery();

        //                 // Obtener el id_unidad generado automáticamente
        //                 MySqlCommand getLastIdCmd = new MySqlCommand("SELECT LAST_INSERT_ID();", conexion);
        //                 int newIdUnidad = Convert.ToInt32(getLastIdCmd.ExecuteScalar());

        //                 // Crear la carpeta con el nombre del nuevo id_unidad generado
        //                 string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Unidades", newIdUnidad.ToString());

        //                 if (!Directory.Exists(folderPath))
        //                 {
        //                     Directory.CreateDirectory(folderPath);
        //                 }
        //             }
        //             else
        //             {
        //                 return View(newUniddes);
        //             }
        //         }
        //         return RedirectToAction("Index");
        //     }

        //     return View(newUniddes);
        // }

        // // GET: Unidades/Venta/5
        // public IActionResult Venta(string id)
        // {
        //     string username = HttpContext.Session.GetString("username") ?? "";
        //     string fiperfil = HttpContext.Session.GetString("fiperfil") ?? "";


        //     if (string.IsNullOrEmpty(username))
        //         return RedirectToAction("Index", "LogIn");
        //     if (fiperfil != "1")
        //         return Redirect("/Unidades/");

        //     string connectionString = Configuration["BDs:SemiCC"];

        //     Unidades? unidades = null;
        //     Ta_venta? venta = null;

        //     using (MySqlConnection conexion = new MySqlConnection(connectionString))
        //     {
        //         conexion.Open();

        //         // Consulta para obtener la unidad
        //         MySqlCommand cmdUnidad = new MySqlCommand();
        //         cmdUnidad.Connection = conexion;
        //         cmdUnidad.CommandText = "SELECT * FROM Unidades WHERE Id_unidad = @Id_unidad";
        //         cmdUnidad.CommandType = System.Data.CommandType.Text;
        //         cmdUnidad.Parameters.AddWithValue("@Id_unidad", id);

        //         using (var cursor = cmdUnidad.ExecuteReader())
        //         {
        //             if (cursor.Read())
        //             {
        //                 unidades = new Unidades()
        //                 {
        //                     Id_unidad = Convert.ToInt32(cursor["Id_unidad"]),
        //                     Estatus = Convert.ToInt32(cursor["Estatus"]),
        //                     Modelo = Convert.ToString(cursor["Modelo"]),
        //                 };
        //             }
        //         }
        //         // Si no se encuentra la unidad, retornar 404
        //         if (unidades != null)
        //         {
        //             ViewBag.Modelo = unidades.Modelo; // Asegúrate de esto
        //         }
        //         // Consulta para obtener la información de venta de ta_venta
        //         MySqlCommand cmdVenta = new MySqlCommand();
        //         cmdVenta.Connection = conexion;
        //         cmdVenta.CommandText = "SELECT * FROM ta_venta WHERE Id_unidad = @Id_unidad";
        //         cmdVenta.CommandType = System.Data.CommandType.Text;
        //         cmdVenta.Parameters.AddWithValue("@Id_unidad", id);

        //         using (var cursorVenta = cmdVenta.ExecuteReader())
        //         {
        //             if (cursorVenta.Read())
        //             {
        //                 venta = new Ta_venta()
        //                 {
        //                     Id_unidad = Convert.ToInt32(cursorVenta["Id_unidad"]),
        //                     Fecha_venta = cursorVenta["Fecha_venta"] != DBNull.Value ? Convert.ToDateTime(cursorVenta["Fecha_venta"]) : (DateTime?)null,
        //                     Vendedor = cursorVenta["Vendedor"] != DBNull.Value ? Convert.ToString(cursorVenta["Vendedor"]) : string.Empty,
        //                     Comprador = cursorVenta["Comprador"] != DBNull.Value ? Convert.ToString(cursorVenta["Comprador"]) : string.Empty,
        //                     Celular = Convert.ToInt32(cursorVenta["Celular"]),
        //                     Tel_casa = Convert.ToInt32(cursorVenta["Tel_casa"]),
        //                     Tel_oficina = Convert.ToInt32(cursorVenta["Tel_oficina"]),
        //                     Correo = cursorVenta["Correo"] != DBNull.Value ? Convert.ToString(cursorVenta["Correo"]) : string.Empty,


        //                 };
        //             }
        //         }
        //     }

        //     // Asignar los datos de venta a ViewBag para ser usados en la vista
        //     if (venta != null)
        //     {
        //         ViewBag.FechaVenta = venta.Fecha_venta;
        //         ViewBag.Vendedor = venta.Vendedor;
        //         ViewBag.Comprador = venta.Comprador;
        //         ViewBag.Celular = venta.Celular;
        //         ViewBag.Tel_casa = venta.Tel_casa;
        //         ViewBag.Tel_oficina = venta.Tel_oficina;
        //         ViewBag.Correo = venta.Correo;
        //     }

        //     return View(unidades);
        // }

        // // POST: Unidades/Venta/5

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public IActionResult Venta(string id, Ta_venta nuevaVenta)
        // {
        //     string username = HttpContext.Session.GetString("username") ?? "";

        //     if (string.IsNullOrEmpty(username))
        //         return RedirectToAction("Index", "LogIn");

        //     if (!ModelState.IsValid)
        //     {
        //         return View(nuevaVenta);
        //     }

        //     string connectionString = Configuration["BDs:SemiCC"];

        //     try
        //     {
        //         using (MySqlConnection conexion = new MySqlConnection(connectionString))
        //         {
        //             conexion.Open();

        //             // Fetch the Modelo again in the POST action
        //             Unidades? unidad = null;
        //             string selectUnidadQuery = "SELECT Modelo FROM Unidades WHERE Id_unidad = @Id_unidad";
        //             using (MySqlCommand selectCmd = new MySqlCommand(selectUnidadQuery, conexion))
        //             {
        //                 selectCmd.Parameters.AddWithValue("@Id_unidad", id);
        //                 using (var cursorUnidad = selectCmd.ExecuteReader())
        //                 {
        //                     if (cursorUnidad.Read())
        //                     {
        //                         unidad = new Unidades()
        //                         {
        //                             Modelo = Convert.ToString(cursorUnidad["Modelo"])
        //                         };
        //                     }
        //                 }
        //             }

        //             // Continue with updating Unidades and inserting Ta_venta
        //             string updateUnidadesQuery = "UPDATE Unidades SET Estatus = @Estatus WHERE Id_unidad = @Id_unidad";
        //             using (MySqlCommand updateCmd = new MySqlCommand(updateUnidadesQuery, conexion))
        //             {
        //                 updateCmd.Parameters.AddWithValue("@Id_unidad", id);
        //                 updateCmd.Parameters.AddWithValue("@Estatus", 0);
        //                 updateCmd.ExecuteNonQuery();
        //             }

        //             string insertVentaQuery = "INSERT INTO ta_venta (Id_unidad, Fecha_venta, Vendedor, Comprador, Modelo,Celular,Tel_casa,Tel_oficina,Correo) VALUES (@Id_unidad, @Fecha_venta, @Vendedor, @Comprador, @Modelo,@Celular,@Tel_casa,@Tel_oficina,@Correo)";
        //             using (MySqlCommand insertCmd = new MySqlCommand(insertVentaQuery, conexion))
        //             {
        //                 insertCmd.Parameters.AddWithValue("@Id_unidad", id);
        //                 insertCmd.Parameters.AddWithValue("@Fecha_venta", nuevaVenta.Fecha_venta);
        //                 insertCmd.Parameters.AddWithValue("@Vendedor", nuevaVenta.Vendedor);
        //                 insertCmd.Parameters.AddWithValue("@Comprador", nuevaVenta.Comprador);
        //                 insertCmd.Parameters.AddWithValue("@Modelo", unidad?.Modelo);
        //                 insertCmd.Parameters.AddWithValue("@Celular", nuevaVenta.Celular);
        //                 insertCmd.Parameters.AddWithValue("@Tel_casa", nuevaVenta.Tel_casa);
        //                 insertCmd.Parameters.AddWithValue("@Tel_oficina", nuevaVenta.Tel_oficina);
        //                 insertCmd.Parameters.AddWithValue("@Correo", nuevaVenta.Correo);
        //                 insertCmd.ExecuteNonQuery();
        //             }
        //         }

        //         return RedirectToAction("Index");
        //     }
        //     catch (Exception)
        //     {
        //         ModelState.AddModelError(string.Empty, "An error occurred while processing the sale.");

        //         return View(nuevaVenta);
        //     }
        // }

    }
}