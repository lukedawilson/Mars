using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using Mars.Models;

namespace Mars.BusinessLogic
{
   public class TabulatedDataParser
   {
      public IEnumerable<DataPoint> Parse(
         StreamReader stream, ColumnDefinition[] columnDefs, int? startRow = null, int? endRow = null)
      {
         var result = new List<DataPoint>();
         using (var reader = new CsvReader(stream))
         {
            var i = 0;
            while (reader.Read())
            {
               if (endRow.HasValue && i >= endRow)
                  break;

               if (startRow.HasValue && i < startRow)
                  continue;

               var outputRow = new DataPoint();
               for (var j = 0; j < Math.Min(reader.FieldHeaders.Length, columnDefs.Length); j++)
               {
                  var column = columnDefs[j].Name;
                  var value = reader.GetField(j).Trim();
                  outputRow.Add(column, value);
               }

               result.Add(outputRow);
               i++;
            }
         }

         return result;
      }
   }
}