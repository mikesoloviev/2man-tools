using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
{
  "ConnectionStrings": { "DataStore": "server=localhost;port=3306;user=root;password=mapleace;database=toolstest6;" },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  }
}

 */

namespace X2MANTools {

    public class JsonSearch {

        // TODO: 'pattern' can contain arbitrary number of levels separated by '/' or (maybe) '.'

        public string FindString(string path, string pattern) {
            try {
                var text = File.ReadAllText(path);
                var levels = pattern.Split('/');
                var i = 0;
                var j = 0;
                i = text.IndexOf($"\"{levels[0]}\"", i);
                if (i < 0) return null;
                i = text.IndexOf(":", i);
                if (i < 0) return null;
                i = text.IndexOf($"\"{levels[1]}\"", i);
                if (i < 0) return null;
                i = text.IndexOf(":", i);
                if (i < 0) return null;
                i = text.IndexOf("\"", i);
                if (i < 0) return null;
                j = text.IndexOf("\"", i + 1);
                if (j < 0) return null;
                return text.Substring(i + 1, j - i - 1).Trim();
            }
            catch {
                return null;
            }
        }
    }
}
