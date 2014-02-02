using System.Collections.Generic;
using System.Web.Http;
using Mars.BusinessLogic;
using Mars.Models;

namespace Mars.Controllers
{
   public class ColumnDefinitionsController : ApiController
   {
      private readonly Repository _repository = new Repository();

      // GET api/columnDefinitions
      public IEnumerable<ColumnDefinition> Get(int? startRow = null, int? endRow = null)
      {
         return _repository.GetColumnDefinitions();
      }
   }
}