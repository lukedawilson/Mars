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

      public string HttpRequest(string url)
      {
         using (var stream = WebHelpers.HttpRequest(url))
         {
            return stream.ReadToEnd();
         }
      }
   }
}
