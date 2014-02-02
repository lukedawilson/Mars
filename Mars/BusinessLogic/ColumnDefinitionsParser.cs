using System;
using System.Collections.Generic;
using System.Linq;
using Mars.Models;

namespace Mars.BusinessLogic
{
   public class ColumnDefinitionsParser
   {
      public IEnumerable<TItem> Parse<TItem>(string columnDefsRaw)
      {
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
               lines[i] = new { lines[i - 1].Name, line.Value };
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

         var topOfStack = stack.Any() ? stack.Pop() : lastPopped;
         return (IEnumerable<TItem>)topOfStack;
      }

      private static void SetValue(IndexTable item, string name, string value)
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

      private static void SetValue(ColumnDefinition item, string name, string value)
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

      private static void CreateAndAddToParent<TChild>(Stack<object> stack) where TChild : new()
      {
         var child = new TChild();

         if (!stack.Any()) // no root element specified
            stack.Push(new List<TChild>());

         AddToParent((dynamic)stack.Peek(), (dynamic)child);
         stack.Push(child);
      }

      private static void AddToParent(ICollection<ColumnDefinition> parent, ColumnDefinition child)
      {
         parent.Add(child);
      }

      private static void AddToParent(ICollection<IndexTable> parent, IndexTable child)
      {
         parent.Add(child);
      }
   }
}