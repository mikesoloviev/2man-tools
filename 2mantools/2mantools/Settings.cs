using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

/*
 TODO: Possible values for sql-type: mysql, mssql, oracle, sqlite.
 */ 

namespace X2MANTools {

    public class Settings {

        public Dictionary<string, string> Data = new Dictionary<string, string>();

        public void Load(string path) {
            try {
                foreach (var line in File.ReadAllLines(path)) {
                    if (line.Contains("=")) {
                        var fields = line.Split('=');
                        Data[fields[0].Trim()] = fields[1].Trim();
                    }
                }
            }
            catch {
            }
        }

        public string GetValue(string key) {
            return Data.ContainsKey(key) ? Data[key] : "";
        }

    }
}
