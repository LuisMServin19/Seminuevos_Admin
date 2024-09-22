using Microsoft.AspNetCore.Mvc;
using Serfitex.Data;

namespace Serfitex.Controllers
{
    public class LogOutController : Controller
    {
        private readonly NuevoBanorteContext _context;

        public LogOutController(NuevoBanorteContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Login");
        }
    }
}