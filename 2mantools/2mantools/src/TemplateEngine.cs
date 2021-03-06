﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace X2MANTools {

    public class TemplateEngine {

        string appBaseDirectory;
        string projectDirectory;
        string templateDirectory;
        string templateExtension = ".sut"; // SmilUp Template
        string os;

        Settings settings;
        CrossSql crossSql;
        List<string> lines;

        public TemplateEngine(string appBaseDirectory, string projectDirectory, string templateDirectory, string os) {
            this.appBaseDirectory = appBaseDirectory;
            this.projectDirectory = projectDirectory;
            this.templateDirectory = templateDirectory;
            this.os = os;
            settings = new Settings(appBaseDirectory, projectDirectory, os);
            crossSql = new CrossSql(appBaseDirectory);
        }

        public void Apply(string module, string template) {
            if (module == "this") {
                module = settings.GetValue("module");
            }
            if (!LoadTemplate(module, template)) return;
            var success = true;
            var guarded = false;
            var i = 0;
            while (i < lines.Count()) {
                var line = lines[i];
                if (line.StartsWith("(:") && !line.StartsWith("(:)")) {
                    var fields = ParseCommand(line);
                    if (guarded) {
                        if (fields[0] == "unguard") {
                            guarded = false;
                        }
                    }
                    else {
                        try {
                            switch (fields[0]) {
                                case "apply":
                                    CallApply(fields[1], fields[2]); break;
                                case "print":
                                    Print(fields[1], fields[2]); break;
                                case "run":
                                    Run(fields[1], fields[2], fields[3]); break;
                                case "create-folder":
                                    CreateFolder(fields[1], fields[2]); break;
                                case "delete-folder":
                                    DeleteFolder(fields[1], fields[2]); break;
                                case "delete-file":
                                    DeleteFile(fields[1], fields[2]); break;
                                case "edit-create":
                                    EditCreate(fields[1], fields[2], GetContent(ref i)); break;
                                case "edit-append":
                                    EditAppend(fields[1], fields[2], GetContent(ref i)); break;
                                case "edit-replace":
                                    EditReplace(fields[1], fields[2], fields[3], GetContent(ref i)); break;
                                case "edit-insert-before":
                                    EditInsertBefore(fields[1], fields[2], fields[3], GetContent(ref i)); break;
                                case "edit-insert-after":
                                    EditInsertAfter(fields[1], fields[2], fields[3], GetContent(ref i)); break;
                                case "edit-insert-before-block-end":
                                    EditInsertBeforeBlockEnd(fields[1], fields[2], fields[3], fields[4], GetContent(ref i)); break;
                                case "edit-delete":
                                    EditDelete(fields[1], fields[2], fields[3]); break;
                                case "transform-sql":
                                    TranformSql(fields[1], fields[2], fields[3], fields[4]); break;
                                case "test-defined":
                                    success = TestDefined(fields[1]); break;
                                case "test-value":
                                    success = TestValue(fields[1], fields[2]); break;
                                case "test-file-exist":
                                    success = TestFileExist(fields[1], fields[2]); break;
                                case "guard":
                                    guarded = !success;
                                    success = true;
                                    if (guarded && fields.Count == 3) Print(fields[1], fields[2]);
                                    break;
                                case "unguard":
                                    break;
                                default:
                                    break;
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

        bool LoadTemplate(string module, string template) {
            var path = Path.Combine(Path.Combine(templateDirectory, module), template + templateExtension);
            if (File.Exists(path)) {
                lines = new List<string>();
                foreach (var line in File.ReadAllLines(path)) {
                    lines.Add(settings.Eval(line));
                }
                return true;
            }
            else {
                Print("Error", $"Template '{template}' in module '{module}' not found at: {path}.");
                return false;
            }
        }

        bool TestDefined(string key) {
            return settings.HasValue(key);
        }

        bool TestValue(string key, string value) {
            return settings.GetValue(key) == value;
        }

        bool TestFileExist(string folder, string file) {
            return File.Exists(Path.Combine(folder, file));
        }

        void CallApply(string module, string template) {
            new TemplateEngine(appBaseDirectory, projectDirectory, templateDirectory, os).Apply(module, template);
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

        void CreateFolder(string parent, string folder) {
            var path = Path.Combine(parent, folder);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        void DeleteFolder(string parent, string folder) {
            try {
                Directory.Delete(Path.Combine(parent, folder), true);
            }
            catch {
            }
        }

        void DeleteFile(string parent, string folder) {
            try {
                File.Delete(Path.Combine(parent, folder));
            }
            catch {
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

        void EditInsertBefore(string folder, string file, string label, string content) {
            EditInsert(folder, file, label, content, false);
        }

        void EditInsertAfter(string folder, string file, string label, string content) {
            EditInsert(folder, file, label, content, true);
        }

        void EditInsert(string folder, string file, string label, string content, bool after) {
            var path = Path.Combine(folder, file);
            var text = File.ReadAllText(path);
            var index = text.IndexOf(label);
            var length = after ? label.Length : 0;
            if (index >= 0) {
                try {
                    text = text.Substring(0, index + length) + content + text.Substring(index + length);
                    File.WriteAllText(path, text);
                }
                catch {
                }
            }
        }

        void EditInsertBeforeBlockEnd(string folder, string file, string start, string end, string content) {
            var path = Path.Combine(folder, file);
            var text = new StringBuilder(); 
            var block = false;
            foreach(var line in File.ReadAllLines(path)) {
                if (line.Contains(start)) {
                    text.AppendLine(line);
                    block = true;
                }
                else if (block) {
                    if (line.Trim() == end) {
                        text.Append(content);
                        block = false;
                    }
                    text.AppendLine(line);
                }
                else {
                    text.AppendLine(line);
                }
            }
            File.WriteAllText(path, text.ToString());
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

        void TranformSql(string type, string folder, string source, string target) {
            crossSql.Transform(Path.Combine(folder, source), Path.Combine(folder, target), type);
        }

        void Print(string type, string content) {
            Console.WriteLine($"2mantools {type}: {content}");
        }

        string GetContent(ref int index) {
            var content = new List<string>();
            for (var i = index; i < lines.Count; i++) {
                index = i;
                if (lines[i].StartsWith("(:)")) {
                    break;
                }
                else if (lines[i].StartsWith("(:")) {
                    continue;
                }
                else {
                    content.Add(lines[i]);
                }
            }
            return string.Join(Environment.NewLine, content);
        }

        List<string> ParseCommand(string line) {
            var fields = new List<string>();
            foreach (var field in line.Replace("(:", "").Replace(":)", "").Trim().Split('|')) {
                fields.Add(field.Trim());
            }
            return fields;
        }

    }
}
