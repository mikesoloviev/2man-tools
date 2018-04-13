using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace X2MANTools {

    class Program {

        static string Usage =
@"
USAGE:
  2mantols -a <template> -p <project-folder> -t <template-folder>
PROJECT-FOLDER:
  Optional. The default value is the folder from where 2mantools.exe is executed.
TEMPLATE-FOLDER:
  Optional. The default value is the folder where 2mantools.exe is located.
TEMPLATES:
  upgrade
    Upgrade the ASP.NET Core Angular project to 2MAN.
";

        static void Main(string[] args) {
            var projectDirectory = Environment.CurrentDirectory;
            var templateDirectory = AppContext.BaseDirectory;
            var templates = new List<string>();
            if (!args.Any()) {
                Console.WriteLine(Usage);
                Thread.Sleep(20000);
                return;
            }
            for (var i = 0; i < args.Count(); i++) {
                if (args[i].StartsWith("-") && (i + 1) < args.Count()) {
                    switch (args[i]) {
                        case "-a":
                            templates.Add(args[i + 1]); break;
                        case "-p":
                            projectDirectory = args[i + 1]; break;
                        case "-t":
                            templateDirectory = args[i + 1]; break;
                    }
                }
            }
            foreach (var template in templates) {
                new TemplateEngine(projectDirectory, templateDirectory).Apply(template);
            }
        }

    }
}
