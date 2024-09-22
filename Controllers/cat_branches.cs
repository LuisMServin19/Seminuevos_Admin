using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using NuevoBanorte.Models;
using NuGet.Protocol;
using System.Data;
using System.Text;
using WebApp.Models;

namespace Serfitex.Controllers
{
    public class cat_branchesController : Controller
    {
        private readonly ILogger<cat_branchesController> _logger;
        private readonly IConfiguration Configuration;

        public cat_branchesController(ILogger<cat_branchesController> logger, IConfiguration configuration)
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


            List<cat_branches> registros = new List<cat_branches>();

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT BRA_NAME, BRA_ACTIVE, ID_DEA FROM cat_branches ORDER BY ID_DEA DESC";
                cmd.CommandType = System.Data.CommandType.Text;

                using (var cursor = cmd.ExecuteReader())
                {
                    while (cursor.Read())
                    {
                        int ID_DEA = Convert.ToInt32(cursor["ID_DEA"]);
                        string BRA_NAME = Convert.ToString(cursor["BRA_NAME"]);
                        bool BRA_ACTIVE = Convert.ToBoolean(cursor["BRA_ACTIVE"]);

                        cat_branches registro = new cat_branches() { ID_DEA = ID_DEA, BRA_NAME = BRA_NAME, BRA_ACTIVE = BRA_ACTIVE };
                        registros.Add(registro);
                    }
                }
            }
            return View(registros);
        }

        // GET: cat_branches/Create
        public IActionResult Create()
        {
            string username = HttpContext.Session.GetString("username") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            return View();
        }

        // POST: cat_branches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(cat_branches newBranch)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string cliente = HttpContext.Session.GetString("conexion") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:" + cliente];

            if (ModelState.IsValid)
            {

                using (MySqlConnection conexion = new MySqlConnection(connectionString))
                {
                    conexion.Open();

                    MySqlCommand getLastCodeCmd = new MySqlCommand("SELECT MAX(id_dea) FROM cat_branches", conexion);
                    int lastCode = Convert.ToInt16(getLastCodeCmd.ExecuteScalar());
                    int newCode = lastCode + 1;

                    MySqlCommand getLastCodeCmdd = new MySqlCommand("SELECT MAX(ID_BRA) FROM cat_branches", conexion);
                    int lastCodee = Convert.ToInt16(getLastCodeCmdd.ExecuteScalar());
                    int newCodee = lastCodee + 1;

                    bool braMainExists = false;
                    MySqlCommand checkColumnCmd = new MySqlCommand("SHOW COLUMNS FROM cat_branches LIKE 'BRA_MAIN'", conexion);
                    using (MySqlDataReader reader = checkColumnCmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            braMainExists = true;
                        }
                    }

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conexion;
                    if (braMainExists)
                    {
                        cmd.CommandText = "INSERT INTO cat_branches (ID_BRA,ID_DEA,BRA_NAME,BRA_ACTIVE,BRA_CDATE,BRA_MAIN) VALUES (@ID_BRA,@ID_DEA,@BRA_NAME,1,@BRA_CDATE,1)";
                    }
                    else
                    {
                        cmd.CommandText = "INSERT INTO cat_branches (ID_BRA,ID_DEA,BRA_NAME,BRA_ACTIVE,BRA_CDATE) VALUES (@ID_BRA,@ID_DEA,@BRA_NAME,1,@BRA_CDATE)";
                    }

                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@ID_BRA", newCodee);
                    cmd.Parameters.AddWithValue("@ID_DEA", newCode);
                    cmd.Parameters.AddWithValue("@BRA_NAME", newBranch.BRA_NAME);
                    cmd.Parameters.AddWithValue("@BRA_CDATE", DateTime.Now);

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
        // GET: cat_branches/Edit/5
        public IActionResult Edit(int id)
        {
            string username = HttpContext.Session.GetString("username") ?? "";
            string cliente = HttpContext.Session.GetString("conexion") ?? "";

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "LogIn");

            string connectionString = Configuration["BDs:" + cliente];

            cat_branches branch = null;

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                conexion.Open();

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexion;
                cmd.CommandText = "SELECT ID_DEA, BRA_NAME, BRA_ACTIVE FROM cat_branches WHERE ID_DEA = @ID_DEA";
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@ID_DEA", id);

                using (var cursor = cmd.ExecuteReader())
                {
                    if (cursor.Read())
                    {
                        branch = new cat_branches()
                        {
                            ID_DEA = Convert.ToInt32(cursor["ID_DEA"]),
                            BRA_NAME = Convert.ToString(cursor["BRA_NAME"]),
                            BRA_ACTIVE = Convert.ToBoolean(cursor["BRA_ACTIVE"])

                        };
                    }
                }
            }

            if (branch == null)
            {
                return NotFound();
            }
            ViewData["StatusOptions"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "true", Text = "Activo" },
                new SelectListItem { Value = "false", Text = "Desactivado" }
            };
            return View(branch);
        }

        // POST: cat_branches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, cat_branches updatedBranch)
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

                    // Fetch existing branch details before updating
                    MySqlCommand fetchCmd = new MySqlCommand();
                    fetchCmd.Connection = conexion;
                    fetchCmd.CommandText = "SELECT ID_DEA, BRA_NAME, BRA_ACTIVE FROM cat_branches WHERE ID_DEA = @ID_DEA";
                    fetchCmd.CommandType = System.Data.CommandType.Text;
                    fetchCmd.Parameters.AddWithValue("@ID_DEA", id);

                    cat_branches existingBranch = null;
                    using (var cursor = fetchCmd.ExecuteReader())
                    {
                        if (cursor.Read())
                        {
                            existingBranch = new cat_branches()
                            {
                                ID_DEA = Convert.ToInt32(cursor["ID_DEA"]),
                                BRA_NAME = Convert.ToString(cursor["BRA_NAME"]),
                                BRA_ACTIVE = Convert.ToBoolean(cursor["BRA_ACTIVE"])
                            };
                        }
                    }

                    MySqlCommand updateCmd = new MySqlCommand();
                    updateCmd.Connection = conexion;
                    updateCmd.CommandText = "UPDATE cat_branches SET BRA_NAME = @BRA_NAME, BRA_ACTIVE = @BRA_ACTIVE WHERE ID_DEA = @ID_DEA";
                    updateCmd.CommandType = System.Data.CommandType.Text;
                    updateCmd.Parameters.AddWithValue("@BRA_NAME", updatedBranch.BRA_NAME);
                    updateCmd.Parameters.AddWithValue("@ID_DEA", id);
                    updateCmd.Parameters.AddWithValue("@BRA_ACTIVE", updatedBranch.BRA_ACTIVE);

                    updateCmd.ExecuteNonQuery();

                    Cambios cambios = new Cambios();
                    cambios.Usuario = HttpContext.Session.GetString("username") ?? "";
                    cambios.Datos = existingBranch.ToJson();
                    cambios.FechaCambios = DateTime.Now;

                    MySqlCommand logCmd = new MySqlCommand();
                    logCmd.Connection = conexion;
                    logCmd.CommandText = "INSERT INTO CAmbios (Usuario, Datos, FechaCambios) VALUES (@Usuario, @Datos, @FechaCambios)";
                    logCmd.CommandType = System.Data.CommandType.Text;
                    logCmd.Parameters.AddWithValue("@Usuario", cambios.Usuario);
                    logCmd.Parameters.AddWithValue("@Datos", cambios.Datos);
                    logCmd.Parameters.AddWithValue("@FechaCambios", cambios.FechaCambios);

                    logCmd.ExecuteNonQuery();
                }

                return RedirectToAction("Index");
            }

            return View(updatedBranch);
        }
    }
}

