using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Mars.BusinessLogic;
using Mars.Models;

namespace Mars.Controllers
{
   public class AcqController : ApiController
   {
      private readonly Repository _repository = new Repository();

      [HttpGet]
      public HttpResponseMessage Get([FromUri]DataRequestModel request)
      {
         var data = _repository.GetAcqData(request.StartRow, request.EndRow);
         var content = new ObjectContent(typeof(IEnumerable<IDictionary<string, string>>), data, new JsonMediaTypeFormatter());
         return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
      }
   }
}
