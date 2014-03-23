using System.Collections.Generic;
using System.Linq;
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
      private readonly TabulatedDataParser _tabulatedDataParser = new TabulatedDataParser();

      #region Index

      public IndexTable GetIndexColumnDefinitions()
      {
         using (var stream = WebHelpers.HttpRequest(IndexColumnDefinitionsUrl))
            return _columnDefinitionsParser.Parse<IndexTable>(stream.ReadToEnd()).Single();
      }

      public IEnumerable<DataPoint> GetIndexData(int? startRow = null, int? endRow = null)
      {
         var columnDefs = GetIndexColumnDefinitions().Columns.ToArray();
         using (var stream = WebHelpers.HttpRequest(IndexDataUrl))
            return _tabulatedDataParser.Parse(stream, columnDefs, startRow, endRow);
      }

      #endregion

      #region Acq

      public IEnumerable<ColumnDefinition> GetAcqColumnDefinitions()
      {
         using (var stream = WebHelpers.HttpRequest(AcqColumnDefinitionsUrl))
            return _columnDefinitionsParser.Parse<ColumnDefinition>(stream.ReadToEnd());
      }

      public IEnumerable<DataPoint> GetAcqData(int? startRow = null, int? endRow = null)
      {
         var columnDefs = GetAcqColumnDefinitions().ToArray();
         using (var stream = WebHelpers.HttpRequest(AcqDataUrl))
            return _tabulatedDataParser.Parse(stream, columnDefs, startRow, endRow);
      }

      #endregion
   }
}