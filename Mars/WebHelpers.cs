﻿using System;
using System.IO;
using System.Net;

namespace Mars
{
   public static class WebHelpers
   {
      public static TEnum AsEnum<TEnum>(this string source)
      {
         return (TEnum)Enum.Parse(typeof(TEnum), source, ignoreCase: true);
      }

      public static string HttpRequestSync(string url)
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
   }
}