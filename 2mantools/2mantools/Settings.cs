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

        public Dictionary<string, string> context = new Dictionary<string, string>();

        string appBaseDirectory;
        string projectDirectory;

        public Settings(string appBaseDirectory, string projectDirectory) {
            this.appBaseDirectory = appBaseDirectory;
            this.projectDirectory = projectDirectory;
            DefineSystem();
            Load();
        }

        public string GetValue(string key) {
            return context.ContainsKey(key) ? Data[key] : "";
        }

        public bool HasValue(string key) {
            return GetValue(key) != "";
        }

        public string Eval(string line) {
            if (line.Contains("$")) {
                foreach (var key in context.Keys) {
                    var target = "$" + key;
                    if (line.Contains(target)) {
                        line = line.Replace(target, context[key]);
                    }
                }
            }
            return line;
        }

        void Load() {
            try {
                foreach (var line in File.ReadAllLines(Path.Combine(appBaseDirectory, "settings.ini"))) {
                    if (line.Contains("=")) {
                        var fields = line.Split('=');
                        context[fields[0].Trim()] = Eval(fields[1].Trim());
                    }
                }
            }
            catch {
            }
        }

        void DefineSystem() {
            context["ROOT"] = projectDirectory;
            context["NAME"] = projectDirectory.Replace(@"\", "/").TrimEnd('/').Split('/').Last();
            context["BASE"] = = context["NAME"].Replace("-", "_").Replace(" ", "_").ToLower();
        }
        
    }
}
