using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using Mars.Models;

namespace Mars.BusinessLogic
{
   public class Repository
   {
      private const string IndexColumnDefinitionsUrl = "http://atmos.nmsu.edu/PDS/data/mslrem_0001/INDEX/INDEX.LBL";
      private const string IndexDataUrl = "http://atmos.nmsu.edu/PDS/data/mslrem_0001/INDEX/INDEX.TAB";
      private const string AcqColumnDefinitionsUrl = "http://atmos.nmsu.edu/PDS/data/mslrem_0001/LABEL/ACQ.FMT";
      private const string AcqDataUrl = "http://atmos.nmsu.edu/PDS/data/mslrem_0001/DATA/SOL_00000_00089/SOL00001/RME_397535244ESE00010000000ACQ____M1.TAB";

      private readonly ColumnDefinitionsParser _columnDefinitionsParser = new ColumnDefinitionsParser();

      #region Index

      public IndexTable GetIndexColumnDefinitions()
      {
         using (var stream = WebHelpers.HttpRequest(IndexColumnDefinitionsUrl))
         {
            return _columnDefinitionsParser.Parse<IndexTable>(stream.ReadToEnd()).Single();
         }
      }

      public IEnumerable<IDictionary<string, string>> GetIndexData(int? startRow = null, int? endRow = null)
      {
         var columnDefs = GetIndexColumnDefinitions().Columns.ToArray();
         return GetData(IndexDataUrl, columnDefs, startRow, endRow);
      }

      #endregion

      #region Acq

      public IEnumerable<ColumnDefinition> GetAcqColumnDefinitions()
      {
         using (var stream = WebHelpers.HttpRequest(AcqColumnDefinitionsUrl))
         {
            return _columnDefinitionsParser.Parse<ColumnDefinition>(stream.ReadToEnd());
         }
      }

      public IEnumerable<IDictionary<string, string>> GetAcqData(int? startRow = null, int? endRow = null)
      {
         var columnDefs = GetAcqColumnDefinitions().ToArray();
         return GetData(AcqDataUrl, columnDefs, startRow, endRow);
      }

      #endregion

      // ToDo: move into separate classes

      #region Data parser

      private static IEnumerable<IDictionary<string, string>> GetData(string url, ColumnDefinition[] columnDefs, int? startRow = null, int? endRow = null)
      {
         var result = new List<IDictionary<string, string>>();
         using (var stream = WebHelpers.HttpRequest(url))
         {
            using (var reader = new CsvReader(stream))
            {
               var i = 0;
               while (reader.Read())
               {
                  if (endRow.HasValue && i >= endRow)
                     break;

                  if (startRow.HasValue && i < startRow)
                     continue;

                  var outputRow = new Dictionary<string, string>();
                  for (var j = 0; j < Math.Min(reader.FieldHeaders.Length, columnDefs.Count()); j++)
                  {
                     var column = columnDefs[j].Name;
                     var value = reader.GetField(j).Trim();
                     outputRow.Add(column, value);
                  }

                  result.Add(outputRow);
                  i++;
               }
            }
         }

         return result;
      }

      #endregion
   }
}