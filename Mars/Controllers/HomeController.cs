using System.Web.Mvc;

namespace Mars.Controllers
{
   public class HomeController : Controller
   {
      public ActionResult Index()
      {
         ViewBag.Title = "Home Page";

         return View();
      }

      public string HttpRequestSync(string url)
      {
         return WebHelpers.HttpRequestSync(url);
      }
   }
}
