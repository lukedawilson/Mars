using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Models;

namespace Mars.BusinessLogic
{
   public class Repository
   {
      private const string ColumnDefinitionsUrl = "http://atmos.nmsu.edu/PDS/data/mslrem_0001/LABEL/ACQ.FMT";
      private const string DataUrl = "http://atmos.nmsu.edu/PDS/data/mslrem_0001/DATA/SOL_00000_00089/SOL00001/RME_397535244ESE00010000000ACQ____M1.TAB";

      public IEnumerable<ColumnDefinition> GetColumnDefinitions()
      {
         var columnDefsRaw = WebHelpers.HttpRequestSync(ColumnDefinitionsUrl);

         var columns = new List<ColumnDefinition>();
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

         ColumnDefinition columnDefinition = null;
         for (var i = 0; i < lines.Count(); i++)
         {
            var currentLine = lines[i];
            var value = currentLine.Value.Trim(new[] { ' ', '"' });
            switch (currentLine.Name)
            {
               case "OBJECT":
                  columnDefinition = new ColumnDefinition();
                  break;

               case "END_OBJECT":
                  columns.Add(columnDefinition);
                  break;

               case "COLUMN_NUMBER":
                  if (columnDefinition == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  columnDefinition.ColumnNumber = Int32.Parse(value);
                  break;

               case "NAME":
                  if (columnDefinition == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  columnDefinition.Name += value;
                  break;

               case "UNIT":
                  if (columnDefinition == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  columnDefinition.Unit += value;
                  break;

               case "DESCRIPTION":
                  if (columnDefinition == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  columnDefinition.Description += value;
                  break;

               case "DATA_TYPE":
                  if (columnDefinition == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  columnDefinition.DataType += value;
                  break;

               case "START_BYTE":
                  if (columnDefinition == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  columnDefinition.StartByte = Int32.Parse(value);
                  break;

               case "BYTES":
                  if (columnDefinition == null)
                     throw new InvalidOperationException("Invalid syntax - field defined outside of COLUMN object");

                  columnDefinition.Bytes = Int32.Parse(value);
                  break;
            }
         }

         return columns;
      }

      public IEnumerable<IDictionary<string, string>> GetData(int? startRow = null, int? endRow = null)
      {
         var columnDefs = GetColumnDefinitions().ToArray();

         var dataRaw = WebHelpers.HttpRequestSync(DataUrl);
         var data = dataRaw.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                           .Select(line => line.Split(',').Select(value => value.Trim()).ToArray())
                           .ToArray();

         var result = new List<IDictionary<string, string>>();
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
   }
}