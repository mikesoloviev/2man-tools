using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;

using System.Text;
using System.Threading.Tasks;

namespace X2MANTools {

    public class TemplateEngine {

        string projectDirectory;
        string templateDirectory;
        string templateExtension = ".txt";

        Dictionary<string, string> vars;
        List<string> lines;

        public TemplateEngine(string projectDirectory, string templateDirectory) {
            this.projectDirectory = projectDirectory;
            this.templateDirectory = templateDirectory;
        }

        public void Apply(string template) {
            Fill();
            if (!Load(template)) return;
            var skipNext = false;
            var i = 0;
            while (i < lines.Count()) {
                var line = lines[i];
                if (line.StartsWith(">>")) {
                    if (skipNext) {
                        skipNext = false;
                    }
                    else {
                        try {
                            var fields = ParseCommand(line);
                            switch (fields[0]) {
                                case "print":
                                    Print(fields[1], fields[2]); break;
                                case "run":
                                    Run(fields[1], fields[2], fields[3]); break;
                                case "make-folder":
                                    MakeFolder(fields[1], fields[2]); break;
                                case "edit-create":
                                    EditCreate(fields[1], fields[2], GetContent(ref i)); break;
                                case "edit-append":
                                    EditAppend(fields[1], fields[2], GetContent(ref i)); break;
                                case "edit-prepend-json":
                                    EditPrependJson(fields[1], fields[2], GetContent(ref i)); break;
                                case "edit-replace":
                                    EditReplace(fields[1], fields[2], fields[3], GetContent(ref i)); break;
                                case "edit-insert-before":
                                    EditInsertBefore(fields[1], fields[2], fields[3], GetContent(ref i)); break;
                                case "edit-insert-after":
                                    EditInsertAfter(fields[1], fields[2], fields[3], GetContent(ref i)); break;
                                case "edit-delete":
                                    EditDelete(fields[1], fields[2], fields[3]); break;
                                case "edit-comment":
                                    EditComment(fields[1], fields[2], fields[3]); break;
                                case "guard-next":
                                    skipNext = !Guard(fields[1], fields[2], fields[3], fields[4]); break;
                                case "guard-rest":
                                    if (!Guard(fields[1], fields[2], fields[3], fields[4])) return; else break;
                            }
                        }
                        catch (Exception e) {
                            Print("Error", $"Line #{i + 1}: {e.ToString()}");
                            break;
                        }
                    }
                }
                i++;
            }
        }

        void Fill() {
            vars = new Dictionary<string, string>();

            vars["$project-name"] = projectDirectory.Replace(@"\", "/").TrimEnd('/').Split('/').Last(); // TODO: get from *.csproj -> <RootNamespace>MyWebApp</RootNamespace>
            vars["$database-name"] = vars["$project-name"].Replace("-", "").Replace("_", "").Replace(" ", "").ToLower();

            vars["$project-folder"] = projectDirectory;
            vars["$client-folder"] = Path.Combine(vars["$project-folder"], "ClientApp");
            vars["$source-folder"] = Path.Combine(vars["$client-folder"], "src");
            vars["$app-folder"] = Path.Combine(vars["$source-folder"], "app");

            vars["$config-connection-dev"] = GetSettingsConnection(Path.Combine(projectDirectory, "appsettings.Development.json")) ?? "";
            vars["$config-connection-pub"] = GetSettingsConnection(Path.Combine(projectDirectory, "appsettings.json")) ?? "";
            vars["$shell-connection-dev"] = MakeShellConnection(vars["$config-connection-dev"]) ?? "";
            vars["$shell-connection-pub"] = MakeShellConnection(vars["$config-connection-pub"]) ?? "";
        }

        bool Load(string template) {
            var path = Path.Combine(templateDirectory, template + templateExtension);
            if (File.Exists(path)) {
                lines = new List<string>();
                foreach(var line in File.ReadAllLines(path)) {
                    lines.Add(Eval(line));
                }
                return true;
            }
            else {
                Print("Error", $"Template {template} not found at {path}.");
                return false;
            }
        }

        bool Guard(string predicate, string value, string messageType, string messageContent) {
            var success = false;
            switch (predicate) {
                case "defined-var":
                    success = vars.ContainsKey($"${value}") && !string.IsNullOrEmpty(vars[$"${value}"]); break;
                case "exist-file":
                    success = File.Exists(value); break;
            }
            if (!success) {
                Print(messageType, messageContent);
            }
            return success;
        }

        void Run(string workingDirectory, string command, string arguments) {
            var startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.FileName = command;
            startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = workingDirectory;
            try {
                using (var process = Process.Start(startInfo)) {
                    process.WaitForExit();
                }
            }
            catch (Exception e) {
                Print("Error", e.ToString());
            }
        }

        void MakeFolder(string parent, string folder) {
            var path = Path.Combine(parent, folder);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        void EditCreate(string folder, string file, string content) {
            File.WriteAllText(Path.Combine(folder, file), content);
        }

        void EditAppend(string folder, string file, string content) {
            var path = Path.Combine(folder, file);
            var text = File.ReadAllText(path) + content;
            File.WriteAllText(path, text);
        }

        void EditPrependJson(string folder, string file, string content) {
            var path = Path.Combine(folder, file);
            var text = File.ReadAllText(path);
            text = "{" + content + text.TrimStart('{');
            File.WriteAllText(path, text);
        }

        void EditInsertBefore(string folder, string file, string label, string content) {
            var path = Path.Combine(folder, file);
            var text = File.ReadAllText(path);
            content = content + label;
            text = text.Replace(label, content);
            File.WriteAllText(path, text);
        }

        void EditInsertAfter(string folder, string file, string label, string content) {
            var path = Path.Combine(folder, file);
            var text = File.ReadAllText(path);
            content = label + content;
            text = text.Replace(label, content);
            File.WriteAllText(path, text);
        }

        void EditReplace(string folder, string file, string label, string content) {
            var path = Path.Combine(folder, file);
            var text = File.ReadAllText(path);
            text = text.Replace(label, content);
            File.WriteAllText(path, text);
        }

        void EditDelete(string folder, string file, string label) {
            EditReplace(folder, file, label, "");
        }

        void EditComment(string folder, string file, string label) {
            var path = Path.Combine(folder, file);
            var text = File.ReadAllText(path);
            text = text.Replace(label, "// " + label);
            File.WriteAllText(path, text);
        }

        void Print(string type, string content) {
            Console.WriteLine($"2mantools {type}: {content}");
        }

        string GetContent(ref int index) {
            var content = new List<string>();
            for (var i = index; i < lines.Count; i++) {
                index = i;
                if (lines[i].StartsWith(">>")) {
                    continue;
                }
                else if (lines[i].StartsWith("//")) {
                    break;
                }
                else {
                    content.Add(lines[i]);
                }
            }
            return string.Join(Environment.NewLine, content);
        }

        List<string> ParseCommand(string line) {
            var fields = new List<string>();
            foreach(var field in line.TrimStart('>').Split('|')) {
                fields.Add(field.Trim());
            }
            return fields;
        }

        string GetSettingsConnection(string path) {
            try {
                var settings = JObject.Parse(File.ReadAllText(path));
                return (string)settings["ConnectionStrings"]["DataStore"];
            }
            catch {
                return null;
            }
        }

        string MakeShellConnection(string connection) {
            try {
                var fields = connection.Split(';').Select(x => x.Split('=')).ToDictionary(x => x.First().Trim().ToLower(), x => x.Last().Trim());
                return $"--host={fields["server"]} --user={fields["user"]} --password={fields["password"]}" + (fields.ContainsKey("port") ? $" --port={fields["port"]}" : "");
            }
            catch {
                return null;
            }
        }

        string Eval(string line) {
            if (line.Contains("$")) {
                foreach (var key in vars.Keys) {
                    if (line.Contains(key)) {
                        line = line.Replace(key, vars[key]);
                    }
                }
            }
            return line;
        }

    }
}
