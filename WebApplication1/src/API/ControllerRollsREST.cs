using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.API;

public class ControllerRollsREST : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}