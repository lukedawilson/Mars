namespace Mars.Models
{
   public class ColumnDefinition
   {
      public int ColumnNumber { get; set; }
      public string Name { get; set; }
      public string Unit { get; set; }
      public string Description { get; set; }
      public string DataType { get; set; }
      public int StartByte { get; set; }
      public int Bytes { get; set; }
   }
}