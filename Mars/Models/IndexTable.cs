using System.Collections;
using System.Collections.Generic;

namespace Mars.Models
{
   public class IndexTable : ICollection<ColumnDefinition>
   {
      private readonly IList<ColumnDefinition> _columns;

      public IndexTable()
      {
         _columns = new List<ColumnDefinition>();
      }

      public string InterchangeFormat { get; set; }
      public int RowBytes { get; set; }
      public int Rows   { get; set; }
      public int Columns { get; set; }
      public string IndexType { get; set; }

      #region ICollection

      public void Add(ColumnDefinition column)
      {
         _columns.Add(column);
      }

      public void Clear()
      {
         _columns.Clear();
      }

      public bool Contains(ColumnDefinition item)
      {
         return _columns.Contains(item);
      }

      public void CopyTo(ColumnDefinition[] array, int arrayIndex)
      {
         _columns.CopyTo(array, arrayIndex);
      }

      public bool Remove(ColumnDefinition item)
      {
         throw new System.NotImplementedException();
      }

      public int Count
      {
         get { return _columns.Count; }
      }

      public bool IsReadOnly
      {
         get { return false; }
      }

      public IEnumerator<ColumnDefinition> GetEnumerator()
      {
         return _columns.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      #endregion
   }
}