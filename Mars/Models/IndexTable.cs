using System.Collections.Generic;

namespace Mars.Models
{
   public class IndexTable
   {
      private readonly IList<ColumnDefinition> _columns;

      public IndexTable()
      {
         _columns = new List<ColumnDefinition>();
      }

      public string InterchangeFormat { get; set; }
      public int RowBytes { get; set; }
      public int RowCount   { get; set; }
      public int ColumnCount { get; set; }
      public string IndexType { get; set; }
      public IEnumerable<ColumnDefinition> Columns { get { return _columns; } }

      public void Add(ColumnDefinition column)
      {
         _columns.Add(column);
      }
   }
}