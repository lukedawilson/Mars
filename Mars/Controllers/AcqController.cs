using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Mars.BusinessLogic;

namespace Mars.Controllers
{
   public class Page
   {
      public int? StartRow { get; set; }
      public int? EndRow { get; set; }
   }

   public class AcqController : ApiController
   {
      private readonly Repository _repository = new Repository();

      [HttpGet]
      public HttpResponseMessage Data([FromUri]Page page)
      {
         var data = _repository.GetAcqData(page.StartRow, page.EndRow);
         var content = new ObjectContent(typeof(IEnumerable<IDictionary<string, string>>), data, new JsonMediaTypeFormatter());
         return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
      }
   }
}
