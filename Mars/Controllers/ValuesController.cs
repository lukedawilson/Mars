using System.Collections.Generic;
using System.Web.Http;
using Mars.BusinessLogic;

namespace Mars.Controllers
{
   public class ValuesController : ApiController
   {
      private readonly Repository _repository = new Repository();

      // GET api/values
      public IEnumerable<IDictionary<string, string>> Get(int? startRow = null, int? endRow = null)
      {
         return _repository.GetData(startRow, endRow);
      }
   }
}
