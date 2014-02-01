using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Mars.Controllers
{
   public class ValuesController : ApiController
   {
      // GET api/values
      public IEnumerable<Dictionary<string, string>> Get(int? startRow = null, int? endRow = null)
      {
         const string columnDefsUrl = "http://atmos.nmsu.edu/PDS/data/mslrem_0001/LABEL/ACQ.FMT";
         const string dataUrl = "http://atmos.nmsu.edu/PDS/data/mslrem_0001/DATA/SOL_00000_00089/SOL00001/RME_397535244ESE00010000000ACQ____M1.TAB";

         var columnDefsRaw = WebHelpers.HttpRequestSync(columnDefsUrl);
         var columnDefs = ParseColumns(columnDefsRaw).ToArray();
         
         var dataRaw = WebHelpers.HttpRequestSync(dataUrl);
         var data = ParseData(dataRaw).ToArray();

         var result = new List<Dictionary<string, string>>();
         for (var i = startRow ?? 0; i < (endRow ?? data.Length); i++)
         {
            var row = data[i];
            var dr = new Dictionary<string, string>();

            for (var j = 0; j < row.Count(); j++)
               dr.Add(columnDefs[j].Name, row[j]);

            result.Add(dr);
         }

         return result;
      }

      private static IEnumerable<Column> ParseColumns(string data)
      {
         var columns = new List<Column>();
         var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                         .Select(line => line.Trim())
                         .Where(line => !String.IsNullOrEmpty(line))
                         .Select(line => line.Split('='))
                         .Where(line => line.Count() == 2) // drop rest of Description, for now
                         .Select(line => new { Name = line[0].Trim().ToUpperInvariant(), Value = line[1].Trim() })
                         .ToArray();

         Column column = null;
         for (var i = 0; i < lines.Count(); i++)
         {
            var currentLine = lines[i];

            switch (currentLine.Name)
            {
               case "OBJECT":
                  column = new Column();
                  break;

               case "END_OBJECT":
                  columns.Add(column);
                  break;

               case "COLUMN_NUMBER":
                  if (column == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  column.ColumnNumber = int.Parse(currentLine.Value);
                  break;

               case "NAME":
                  if (column == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  column.Name = currentLine.Value;
                  break;

               case "UNIT":
                  if (column == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  column.Unit = currentLine.Value;
                  break;

               case "DESCRIPTION":
                  if (column == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  column.Description = currentLine.Value;
                  break;

               case "DATA_TYPE":
                  if (column == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  column.DataType = currentLine.Value;
                  break;

               case "START_BYTE":
                  if (column == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  column.StartByte = int.Parse(currentLine.Value);
                  break;

               case "BYTES":
                  if (column == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  column.Bytes = int.Parse(currentLine.Value);
                  break;
            }
         }

         return columns;
      }

      private static IEnumerable<string[]> ParseData(string data)
      {
         return data.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select(line => line.Split(',').Select(value => value.Trim()).ToArray());
      }
   }
}
