using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using NuevoBanorte.Models;
using NuGet.Protocol;
using System.Data;
using System.Text;
using WebApp.Models;

namespace Serfitex.Controllers
{
    public class cat_dealersController : Controller
    {
        private readonly ILogger<cat_dealersController> _logger;
        private readonly IConfiguration Configuration;

        public cat_dealersController(ILogger<cat_dealersController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IActionResult Index()
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string cliente = HttpContext.Session.GetString("conexion") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:" + cliente];


            List<cat_dealers> registros = new List<cat_dealers>();

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT DEA_NAME, ID_DEA, DEA_ACTIVE FROM cat_dealers ORDER BY ID_DEA DESC";
                cmd.CommandType = System.Data.CommandType.Text;

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        int ID_DEA = Convert.ToInt16(cursor["ID_DEA"]);
                        string DEA_NAME = Convert.ToString(cursor["DEA_NAME"]);
                        bool DEA_ACTIVE = Convert.ToBoolean(cursor["DEA_ACTIVE"]);

                        cat_dealers registro = new cat_dealers() { ID_DEA = ID_DEA, DEA_NAME = DEA_NAME, DEA_ACTIVE = DEA_ACTIVE };
                        registros.Add(registro);
                    }
                }
            }
            return View(registros);
        }

        // GET: cat_dealers/Create
        public IActionResult Create()
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            return View();
        }

        // POST: cat_dealers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(cat_dealers newBranch)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string cliente = HttpContext.Session.GetString("conexion") ?? "";

            if (ModelState.IsValid)
            {
                string connectionString = Configuration["BDs:" + cliente];

                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    MySqlCommand getLastCodeCmd = new MySqlCommand("SELECT REPLACE(DEA_CODE, 'D', '') AS DEA_CODE FROM cat_dealers ORDER BY DEA_CDATE DESC LIMIT 1;", conexion);
                    string lastCode = Convert.ToString(getLastCodeCmd.ExecuteScalar());
                    string newCode = string.Empty;
                    if (lastCode == string.Empty)
                    {
                        newCode = "D000";
                    }
                    else
                    {
                        newCode = "D" + (Convert.ToInt32(lastCode) + 1).ToString();

                    }
                    MySqlCommand getLastCodeCmdd = new MySqlCommand("SELECT MAX(id_dea) FROM cat_dealers", conexion);
                    int lastCodee = Convert.ToInt16(getLastCodeCmdd.ExecuteScalar());
                    int ID_DEA = lastCodee + 1;

                    MySqlCommand getLastCodeCmddd = new MySqlCommand("SELECT id_cus FROM cat_dealers ORDER BY ID_DEA DESC LIMIT 1", conexion);
                    int ID_CUS = Convert.ToInt16(getLastCodeCmddd.ExecuteScalar());

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conexion;
                    cmd.CommandText = "INSERT INTO cat_dealers (ID_DEA,ID_CUS,DEA_NAME,DEA_CODE,DEA_ACTIVE,DEA_CDATE) VALUES (@ID_DEA,@ID_CUS,@DEA_NAME,@DEA_CODE,1,@DEA_CDATE)";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@ID_DEA", ID_DEA);
                    cmd.Parameters.AddWithValue("@ID_CUS", ID_CUS);
                    cmd.Parameters.AddWithValue("@DEA_NAME", newBranch.DEA_NAME);
                    cmd.Parameters.AddWithValue("@DEA_CODE", newCode);
                    cmd.Parameters.AddWithValue("@DEA_CDATE", DateTime.Now);


                    cmd.ExecuteNonQuery();

                    Cambios cambios = new Cambios
                    {
                        Usuario = HttpContext.Session.GetString("username") ?? "",
                        Datos = newBranch.ToJson(),
                        FechaCambios = DateTime.Now
                    };

                    MySqlCommand logCmd = new MySqlCommand();
                    logCmd.Connection = conexion;
                    logCmd.CommandText = "INSERT INTO Cambios (Usuario, Datos, FechaCambios) VALUES (@Usuario, @Datos, @FechaCambios)";
                    logCmd.CommandType = System.Data.CommandType.Text;
                    logCmd.Parameters.AddWithValue("@Usuario", cambios.Usuario);
                    logCmd.Parameters.AddWithValue("@Datos", cambios.Datos);
                    logCmd.Parameters.AddWithValue("@FechaCambios", cambios.FechaCambios);

                    logCmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index");
            }

            return View(newBranch);
        }

        // GET: cat_dealers/Edit/5
        public IActionResult Edit(int id)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string cliente = HttpContext.Session.GetString("conexion") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:" + cliente];
            cat_dealers dealer = null;

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT ID_DEA, DEA_NAME, DEA_ACTIVE FROM cat_dealers WHERE ID_DEA = @ID_DEA", conexion);
                cmd.Parameters.AddWithValue("@ID_DEA", id);

                using (var cursor = cmd.ExecuteReader())
                {
                    if (cursor.Read())
                    {
                        dealer = new cat_dealers
                        {
                            ID_DEA = Convert.ToInt32(cursor["ID_DEA"]),
                            DEA_NAME = Convert.ToString(cursor["DEA_NAME"]),
                            DEA_ACTIVE = Convert.ToBoolean(cursor["DEA_ACTIVE"])
                        };
                    }
                }
            }

            if (dealer == null)
                return NotFound();
            ViewData["StatusOptions"] = new List<SelectListItem>
    {
        new SelectListItem { Value = "true", Text = "Activo" },
        new SelectListItem { Value = "false", Text = "Desactivado" }
    };

            return View(dealer);
        }
        // POST: cat_dealers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, cat_dealers dealer)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string cliente = HttpContext.Session.GetString("conexion") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            if (ModelState.IsValid)
            {
                string connectionString = Configuration["BDs:" + cliente];

                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    MySqlCommand fetchCmd = new MySqlCommand();
                    fetchCmd.Connection = conexion;
                    fetchCmd.CommandText = "SELECT ID_DEA, DEA_NAME, DEA_ACTIVE FROM cat_dealers WHERE ID_DEA = @ID_DEA";
                    fetchCmd.CommandType = System.Data.CommandType.Text;
                    fetchCmd.Parameters.AddWithValue("@ID_DEA", id);

                    cat_dealers existingBranch = null;
                    using (var cursor = fetchCmd.ExecuteReader())
                    {
                        if (cursor.Read())
                        {
                            existingBranch = new cat_dealers()
                            {
                                ID_DEA = Convert.ToInt32(cursor["ID_DEA"]),
                                DEA_NAME = Convert.ToString(cursor["DEA_NAME"]),
                                DEA_ACTIVE = Convert.ToBoolean(cursor["DEA_ACTIVE"])
                            };
                        }
                    }

                    if (existingBranch != null)
                    {
                        MySqlCommand cmd = new MySqlCommand("UPDATE cat_dealers SET DEA_NAME = @DEA_NAME, DEA_ACTIVE = @DEA_ACTIVE WHERE ID_DEA = @ID_DEA", conexion);
                        cmd.Parameters.AddWithValue("@DEA_NAME", dealer.DEA_NAME);
                        cmd.Parameters.AddWithValue("@ID_DEA", id);
                        cmd.Parameters.AddWithValue("@DEA_ACTIVE", dealer.DEA_ACTIVE);

                        cmd.ExecuteNonQuery();

                        Cambios cambios = new Cambios();
                        cambios.Usuario = HttpContext.Session.GetString("username") ?? "";
                        cambios.Datos = existingBranch.ToJson();
                        cambios.FechaCambios = DateTime.Now;

                        MySqlCommand logCmd = new MySqlCommand();
                        logCmd.Connection = conexion;
                        logCmd.CommandText = "INSERT INTO Cambios (Usuario, Datos, FechaCambios) VALUES (@Usuario, @Datos, @FechaCambios)";
                        logCmd.CommandType = System.Data.CommandType.Text;
                        logCmd.Parameters.AddWithValue("@Usuario", cambios.Usuario);
                        logCmd.Parameters.AddWithValue("@Datos", cambios.Datos);
                        logCmd.Parameters.AddWithValue("@FechaCambios", cambios.FechaCambios);

                        logCmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }

            return View(dealer);
        }

    }
}
