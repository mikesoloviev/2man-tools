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
            DefineHome();
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
                        var key = fields.Length > 0 ? fields[0] : "";
                        var value = fields.Length > 1 ? fields[1] : "";
                        if (key.StartsWith("$$"))
                            context[key] = Call(value);
                        else
                            context[key] = Eval(value);
                    }
                }
            }
            catch {
            }
        }

        void DefineHome() {
            context["HOME"] = projectDirectory;
        }

        string Call(string code) {
            var fields = code.TrimEnd(')').Split('(');
            var name = fields.Length > 0 ? fields[0] : "";
            var value = fields.Length > 1 ? fields[1] : "";
            switch (name) {
                case "$$LAST": return CallLast(Eval(value));
                case "$$LOWER": return CallLower(Eval(value));
                default: return "";
            }
        }

        string CallLast(string value) {
            return value.Replace(@"\", "/").TrimEnd('/').Split('/').Last();
        }

        string CallLower(string value) {
            return value.ToLower();
        }

    }
}
