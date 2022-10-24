using Microsoft.AspNetCore.Mvc;

namespace EasyOC.Amis.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("EasyOC.Amis/amis-editor/index")]
        public IActionResult AmisEditor()
        {
            //return Redirect("~/EasyOC.Amis/amis-editor/index.html");
            return View();
        }
    }
}
