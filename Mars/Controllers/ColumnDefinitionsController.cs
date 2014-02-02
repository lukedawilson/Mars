using System;
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
      public IEnumerable<object> Get(string dataset, int? startRow = null, int? endRow = null)
      {
         switch (dataset.AsEnum<Dataset>())
         {
            case Dataset.Index:
               return _repository.GetIndexColumnDefinitions();

            case Dataset.Acq:
               return _repository.GetAcqColumnDefinitions();

            default:
               throw new NotSupportedException("The " + dataset + "dataset is not supported.");
         }
      }
   }
}