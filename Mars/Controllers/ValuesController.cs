using System;
using System.Collections.Generic;
using System.Web.Http;
using Mars.BusinessLogic;
using Mars.Models;

namespace Mars.Controllers
{
   public class ValuesController : ApiController
   {
      private readonly Repository _repository = new Repository();

      // GET api/values
      public IEnumerable<IDictionary<string, string>> Get(
         string dataset, int? startRow = null, int? endRow = null)
      {
         switch (dataset.AsEnum<Dataset>())
         {
            case Dataset.Index:
               return _repository.GetIndexData(startRow, endRow);

            case Dataset.Acq:
               return _repository.GetAcqData(startRow, endRow);

            default:
               throw new NotSupportedException("The " + dataset + "dataset is not supported.");
         }
      }
   }
}
