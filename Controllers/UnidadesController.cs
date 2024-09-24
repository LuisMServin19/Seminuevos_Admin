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

namespace Serfitex.Controllers
{
    public class UnidadesController : Controller
    {
        private readonly ILogger<UnidadesController> _logger;
        private readonly IConfiguration Configuration;

        public UnidadesController(ILogger<UnidadesController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IActionResult Index()
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:cetelem"];

            List<Unidades> registros = new List<Unidades>();

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT * FROM Unidades";
                cmd.CommandType = System.Data.CommandType.Text;

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        int Id_unidad = Convert.ToInt32(cursor["Id_unidad"]);
                        string Modelo = Convert.ToString(cursor["Modelo"]);
                        string Marca = Convert.ToString(cursor["Marca"]);

                        Unidades registro = new Unidades() { Id_unidad = Id_unidad, Modelo = Modelo, Marca = Marca };
                        registros.Add(registro);
                    }
                }
            }

            return View(registros);
        }

        // GET: Unidades/Create
        public IActionResult Create()
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            return View();
        }

        // POST: Unidades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Unidades newUniddes)
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:cetelem"];

            if (ModelState.IsValid)
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    bool exist = false;
                    MySqlCommand checkColumnCmd = new MySqlCommand(string.Format("SELECT * FROM Unidades WHERE Id_unidad='{0}'", newUniddes.Id_unidad), conexion);
                    using (MySqlDataReader reader = checkColumnCmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            exist = true;
                        }
                    }

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conexion;

                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@Id_unidad", newUniddes.Id_unidad);
                    cmd.Parameters.AddWithValue("@Modelo", newUniddes.Modelo);
                    cmd.Parameters.AddWithValue("@Marca", newUniddes.Marca);
                    


                    if (!exist)
                    {
                        cmd.CommandText = "INSERT INTO Unidades (Modelo,Marca) VALUES (@Modelo,@Marca)";
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        return View(newUniddes);
                    }
                }
                return RedirectToAction("Index");
            }

            return View(newUniddes);
        }

        // GET: Unidades/Edit/5
        public IActionResult Edit(string id)
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:cetelem"];

            Unidades unidades = null;

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT * FROM Unidades WHERE Id_unidad = @Id_unidad";
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@Id_unidad", id);

                using (var cursor = cmd.ExecuteReader())
                {
                    if (cursor.Read())
                    {
                        unidades = new Unidades()
                        {
                            Id_unidad = Convert.ToInt32(cursor["Id_unidad"]),
                            Modelo = Convert.ToString(cursor["Modelo"]),
                            Marca = Convert.ToString(cursor["Marca"]),
                        };
                    }
                }
            }

            if (unidades == null)
            {
                return NotFound();
            }

            return View(unidades);
        }

        // POST: Unidades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Unidades updatedUnidades)
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (!ModelState.IsValid)
                return View(updatedUnidades);

            string connectionString = Configuration["BDs:cetelem"];

            try
            {
                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    string query = "UPDATE Unidades SET Modelo = @Modelo, Marca = @Marca WHERE Id_unidad = @Id_unidad";
                    using (MySqlCommand updateCmd = new MySqlCommand(query, conexion))
                    {
                        updateCmd.Parameters.AddWithValue("@Modelo", updatedUnidades.Modelo);
                        updateCmd.Parameters.AddWithValue("@Marca", updatedUnidades.Marca);
                        updateCmd.Parameters.AddWithValue("@Id_unidad", updatedUnidades.Id_unidad);

                        updateCmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the contract.");
                return View(updatedUnidades);
            }
        }
    }
}