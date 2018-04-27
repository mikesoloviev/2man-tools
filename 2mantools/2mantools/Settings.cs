using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace X2MANTools {

    public class Settings {

        Dictionary<string, string> context = new Dictionary<string, string>();

        string appBaseDirectory;
        string projectDirectory;
        string os;

        public Settings(string appBaseDirectory, string projectDirectory, string os) {
            this.appBaseDirectory = appBaseDirectory;
            this.projectDirectory = projectDirectory;
            this.os = os;
            DefineSystem();
            Load();
        }

        public string GetValue(string key) {
            return context.ContainsKey(key) ? context[key] : "";
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
                var group = Term.any;
                foreach (var rawLine in File.ReadAllLines(Path.Combine(appBaseDirectory, "settings.ini"))) {
                    var line = rawLine.Trim();
                    if (line.StartsWith(";") || line.StartsWith("#")) {
                        // comment
                    }
                    else if (line.StartsWith("[")) {
                        group = line.TrimStart('[').TrimEnd(']').Trim().ToLower();
                    }
                    else if (line.Contains("=") && (group == Term.any || group == os)) {
                        var fields = line.Split('=');
                        context[fields[0].Trim()] = Eval(fields[1].Trim());
                    }
                }
            }
            catch {
            }
        }

        void DefineSystem() {
            context["HOME"] = projectDirectory;
            context["PROJECT"] = projectDirectory.Replace(@"\", "/").TrimEnd('/').Split('/').Last();
            context["DATABASE"] = context["PROJECT"].Replace("-", "_").Replace(" ", "_").ToLower();
        }
        
    }
}
