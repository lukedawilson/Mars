using System;
using System.Collections.Generic;
using System.IO;
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

      #region Index

      public IEnumerable<IndexTable> GetIndexColumnDefinitions()
      {
         return (IEnumerable<IndexTable>)GetColumnDefinitions(IndexColumnDefinitionsUrl);
      }

      public IEnumerable<IDictionary<string, string>> GetIndexData(int? startRow = null, int? endRow = null)
      {
         var columnDefs = GetIndexColumnDefinitions().ToArray();
         return GetData(IndexDataUrl, columnDefs.Single().ToArray(), startRow, endRow);
      }

      #endregion

      #region Acq

      public IEnumerable<ColumnDefinition> GetAcqColumnDefinitions()
      {
         return (IEnumerable<ColumnDefinition>)GetColumnDefinitions(AcqColumnDefinitionsUrl);
      }

      public IEnumerable<IDictionary<string, string>> GetAcqData(int? startRow = null, int? endRow = null)
      {
         var columnDefs = GetAcqColumnDefinitions().ToArray();
         return GetData(AcqDataUrl, columnDefs, startRow, endRow);
      }

      #endregion

      // ToDo: move into separate classes

      #region Column defs parser

      private object GetColumnDefinitions(string url)
      {
         var columnDefsRaw = WebHelpers.HttpRequestSync(url);

         var lines = columnDefsRaw.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                                  .Select(line => line.Trim())
                                  .Where(line => !String.IsNullOrEmpty(line))
                                  .Select(line => line.Split('='))
                                  .Select(line =>
                                     {
                                        if (line.Count() == 1) // continuation of previous line
                                           return new { Name = String.Empty, Value = Environment.NewLine + line[0].Trim() };
                                
                                        return new { Name = line[0].Trim().ToUpperInvariant(), Value = line[1].Trim() };
                                     })
                                  .ToArray();

         for (var i = 1; i < lines.Length; i++)
         {
            var line = lines[i];
            if (String.IsNullOrEmpty(line.Name))
               lines[i] = new {lines[i - 1].Name, line.Value};
         }

         var stack = new Stack<object>();
         object lastPopped = null;
         for (var i = 0; i < lines.Count(); i++)
         {
            var currentLine = lines[i];
            var value = currentLine.Value.Trim(new[] { ' ', '"' });
            switch (currentLine.Name)
            {
               case "OBJECT":
                  switch (currentLine.Value.ToUpperInvariant())
                  {
                     case "COLUMN":
                        CreateAndAddToParent<ColumnDefinition>(stack);
                        break;
                     case "INDEX_TABLE":
                        CreateAndAddToParent<IndexTable>(stack);
                        break;
                  }

                  break;

               case "END_OBJECT":
                  lastPopped = stack.Pop();
                  break;

               default:
                  if (stack.Any())
                     SetValue((dynamic)stack.Peek(), currentLine.Name, value);
                  
                  break;
            }
         }

         return stack.Any() ? stack.Pop() : lastPopped;
      }

      private void SetValue(IndexTable item, string name, string value)
      {
         switch (name)
         {
            case "INTERCHANGE_FORMAT":
               item.InterchangeFormat = value;
               break;

            case "ROW_BYTES":
               item.RowBytes = int.Parse(value);
               break;

            case "ROWS":
               item.Rows = int.Parse(value);
               break;

            case "COLUMNS":
               item.Columns = int.Parse(value);
               break;

            case "INDEX_TYPE":
               item.IndexType = value;
               break;
         }
      }

      private void SetValue(ColumnDefinition item, string name, string value)
      {
         switch (name)
         {
            case "COLUMN_NUMBER":
               item.ColumnNumber = Int32.Parse(value);
               break;

            case "NAME":
               item.Name += value;
               break;

            case "UNIT":
               item.Unit += value;
               break;

            case "DESCRIPTION":
               item.Description += value;
               break;

            case "DATA_TYPE":
               item.DataType += value;
               break;

            case "START_BYTE":
               item.StartByte = Int32.Parse(value);
               break;

            case "BYTES":
               item.Bytes = Int32.Parse(value);
               break;
         }
      }

      private void CreateAndAddToParent<TChild>(Stack<object> stack) where TChild : new()
      {
         var child = new TChild();

         if (!stack.Any()) // no root element specified
            stack.Push(new List<TChild>());

         AddToParent((dynamic)stack.Peek(), (dynamic)child);
         stack.Push(child);
      }
      
      private void AddToParent(ICollection<ColumnDefinition> parent, ColumnDefinition child)
      {
         parent.Add(child);
      }

      private void AddToParent(ICollection<IndexTable> parent, IndexTable child)
      {
         parent.Add(child);
      }

      #endregion

      #region Data parser

      private static IEnumerable<IDictionary<string, string>> GetData(string url, ColumnDefinition[] columnDefs, int? startRow = null, int? endRow = null)
      {
         var dataRaw = WebHelpers.HttpRequestSync(url);
         var result = new List<IDictionary<string, string>>();
         using (var stringReader = new StringReader(dataRaw))
         {
            using (var reader = new CsvReader(stringReader))
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