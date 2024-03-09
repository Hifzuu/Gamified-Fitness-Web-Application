using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ProjectWebApp.Controllers
{
    public class CommunityController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
