using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Mars.Controllers
{
   public class Column
   {
      public int COLUMN_NUMBER { get; set; }
      public string NAME  { get; set; }
      public string UNIT  { get; set; }
      public string DESCRIPTION  { get; set; }
      public string DATA_TYPE  { get; set; }
      public int START_BYTE  { get; set; }
      public int BYTES { get; set; }
   }

   public class HomeController : Controller
   {
      public ActionResult Index()
      {
         ViewBag.Title = "Home Page";

         return View();
      }

      public string HttpRequestSync(string url)
      {
         using (var client = new WebClient())
         {
            using (var stream = client.OpenRead(url))
            {
               if (stream == null)
                  throw new WebException("Failed to connect to " + url);

               using (var streamReader = new StreamReader(stream))
               {
                  return streamReader.ReadToEnd();
               }
            }
         }
      }

      public string Get()
      {
         const string columnDefsUrl = "http://atmos.nmsu.edu/PDS/data/mslrem_0001/LABEL/ACQ.FMT";
         const string dataUrl = "http://atmos.nmsu.edu/PDS/data/mslrem_0001/DATA/SOL_00000_00089/SOL00001/RME_397535244ESE00010000000ACQ____M1.TAB";

         var columns = ParseColumns(HttpRequestSync(columnDefsUrl));
         var data = ParseData(HttpRequestSync(dataUrl));

         var dt = new DataTable();
         foreach(var column in columns)
            dt.Columns.Add(column.NAME, typeof(string));
         
         foreach (var row in data)
         {
            var dr = dt.NewRow();
            for (var i = 0; i < row.Count(); i++)
               dr[i] = row.ToArray()[i].Trim();

            dt.Rows.Add(dr);
         }

         return JsonConvert.SerializeObject(dt);
      }

      private IEnumerable<Column> ParseColumns(string data)
      {
         var columns = new List<Column>();
         var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

         var i = 0;
         while (i < lines.Count())
         {
            var column = new Column();
            while (i < lines.Count() && lines[i] != "END_OBJECT      = COLUMN")
            {
               var kvp = lines[i].Split('=');
               switch (kvp[0].Trim())
               {
                  case "COLUMN_NUMBER":
                     column.COLUMN_NUMBER = int.Parse(kvp[1].Trim('"', ' '));
                     break;

                  case "NAME":
                     column.NAME = kvp[1].Trim('"', ' ');
                     break;

                  case "UNIT":
                     column.UNIT = kvp[1].Trim('"', ' ');
                     break;

                  case "DESCRIPTION":
                     column.DESCRIPTION = kvp[1].Trim('"', ' ');
                     break;

                  case "DATA_TYPE":
                     column.DATA_TYPE = kvp[1].Trim('"', ' ');
                     break;

                  case "START_BYTE":
                     column.START_BYTE = int.Parse(kvp[1].Trim('"', ' '));
                     break;

                  case "BYTES":
                     column.BYTES = int.Parse(kvp[1].Trim('"', ' '));
                     break;
               }

               i++;
            }

            i++;
            columns.Add(column);
         }

         return columns;
      }

      private IEnumerable<IEnumerable<string>> ParseData(string data)
      {
         var table = new List<IEnumerable<string>>();
         var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

         foreach(var line in lines)
         {
            var row = line.Split(',');
            table.Add(row);
         }

         return table;
      }
   }
}
