using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace X2MANTools
{
    class Program
    {

        static string projectDirectory = "";
        static string clientDirectory = "";
        static string sourceDirectory = "";
        static string appDirectory = "";

        static void Main(string[] args)
        {
            projectDirectory = Environment.CurrentDirectory;
            clientDirectory = Path.Combine(projectDirectory, "ClientApp");
            sourceDirectory = Path.Combine(clientDirectory, "src");
            appDirectory = Path.Combine(sourceDirectory, "app");

            if (!args.Any())
            {
                Usage();
                return;
            }
            foreach (var arg in args)
            {
                switch (arg.TrimStart('-'))
                {
                    case "upgrade":
                        Upgrade(projectDirectory);
                        break;
                    case "migrate":
                        Migrate(projectDirectory);
                        break;
                    case "deploy":
                        Deploy(projectDirectory);
                        break;
                    case "example":
                        Example(projectDirectory);
                        break;
                    default:
                        Message("Error", $"Invalid argument: {arg}");
                        break;
                }
            }
        }

        static void Upgrade(string projectDirectory)
        {
            // MySQL Libraries
            Message("Info", "Upgrading - MySQL Libraries");
            RunShell("dotnet", "add package Microsoft.EntityFrameworkCore.Tools -v 2.1.0-preview2-final", projectDirectory);
            RunShell("dotnet", "add package Pomelo.EntityFrameworkCore.MySql", projectDirectory);
            // Angular Material Libraries
            Message("Info", "Upgrading - Angular Material Libraries");
            RunShell("npm", "install --save @angular/material @angular/cdk", clientDirectory);
            RunShell("npm", "install --save @angular/animations", clientDirectory);
            RunShell("npm", "install --save hammerjs", clientDirectory);
            // Angular Material File Templates
            Message("Info", "Upgrading - Angular Material File Templates");
            Append(Path.Combine(sourceDirectory, "styles.css"), Templates.MatTheme);
            Append(Path.Combine(sourceDirectory, "main.ts"), Templates.MatHammer);
            InsertBefore(Path.Combine(sourceDirectory, "index.html"), "</head>", Templates.MatIcons);
            InsertBefore(Path.Combine(appDirectory, "app.module.ts"), "@NgModule", Templates.MatImports);
            InsertAfter(Path.Combine(appDirectory, "app.module.ts"), "imports: [", Templates.MatModules);
        }

        static void Migrate(string projectDirectory)
        {
            Message("Info", "Migrating - Modifying Database");
            var settingsConnection = Modify(projectDirectory, "appsettings.Development.json");
            if (string.IsNullOrEmpty(settingsConnection)) { return; }
            Message("Info", "Migrating - Scaffolding Database");
            var outFolder = "Models/Data";
            if (!Directory.Exists(outFolder)) { Directory.CreateDirectory(outFolder); }
            RunShell("dotnet", $"ef dbcontext scaffold \"{settingsConnection}\" Pomelo.EntityFrameworkCore.MySql -o \"{outFolder}\" -c DataStore -f", projectDirectory);
            Replace(Path.Combine(outFolder, "DataStore.cs"), "protected override void OnConfiguring", "void _OnConfiguring");
            Replace(Path.Combine(outFolder, "DataStore.cs"), "#warning", "                // #warning");
        }

        static void Deploy(string projectDirectory)
        {
            Message("Info", "Deploying - Modifying Database");
            Modify(projectDirectory, "appsettings.json");
        }

        static string Modify(string projectDirectory, string settingsFile)
        {
            var settingsConnection = GetSettingsConnection(Path.Combine(projectDirectory, settingsFile));
            if (string.IsNullOrEmpty(settingsConnection))
            {
                Message("Error", "Failed to get the database connection string from settings");
                return null;
            }
            var shellConnection = MakeShellConnection(settingsConnection);
            if (string.IsNullOrEmpty(shellConnection))
            {
                Message("Error", "Failed to parse the database connection string");
                return null;
            }
            RunShell("mysqlsh", $"{shellConnection} --sql --file=migrate.sql", projectDirectory);
            return settingsConnection;
        }

        // "ConnectionStrings": { "DataStore": "server=localhost;port=3306;user=root;password=mapleace;database=mywebapp;" }
        // --host=localhost --port=3306 --user=root --password=mapleace --sql --file=migrate.sql

        static string GetSettingsConnection(string path)
        {
            try
            {
                var settings = JObject.Parse(File.ReadAllText(path));
                return (string)settings["ConnectionStrings"]["DataStore"];
            }
            catch
            {
                return null;
            }
        }

        static string MakeShellConnection(string inConnection)
        {
            try
            {
                var fields = inConnection.Split(';').Select(x => x.Split('=')).ToDictionary(x => x.First().Trim().ToLower(), x => x.Last().Trim());
                var outConnection = $"--host={fields["server"]} --user={fields["user"]} --password={fields["password"]}";
                if (fields.ContainsKey("port")) outConnection += $" --port={fields["port"]}";
                return outConnection;
            }
            catch
            {
                return null;
            }
        }

        static void Example(string projectDirectory)
        {
            Message("Info", "Example - File Templates");
            Create(Path.Combine(appDirectory, "app.component.html"), Templates.AppHtml);
            Create(Path.Combine(appDirectory, "app.component.css"), Templates.AppCss);
        }

        static void RunShell(string command, string arguments, string workingDirectory)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.FileName = command;
            startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = workingDirectory;
            try
            {
                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Message("Error", e.ToString());
            }
        }

        static void Create(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        static void Append(string path, string content)
        {
            var text = File.ReadAllText(path) + content;
            File.WriteAllText(path, text);
        }

        static void InsertBefore(string path, string label, string content)
        {
            var text = File.ReadAllText(path);
            content = content + label;
            text = text.Replace(label, content);
            File.WriteAllText(path, text);
        }

        static void InsertAfter(string path, string label, string content)
        {
            var text = File.ReadAllText(path);
            content = label + content;
            text = text.Replace(label, content);
            File.WriteAllText(path, text);
        }

        static void Replace(string path, string label, string content)
        {
            var text = File.ReadAllText(path);
            text = text.Replace(label, content);
            File.WriteAllText(path, text);
        }

        static void Message(string type, string content)
        {
            Console.WriteLine($"2mantools {type}: {content}");
        }

        static void Usage()
        {
            Console.WriteLine(Templates.UsageInfo);
            Thread.Sleep(20000);
        }

    }
}
