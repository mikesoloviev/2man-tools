using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading;

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace X2MANTools
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Usage();
            }
            else
            {
                var projectDirectory = Environment.CurrentDirectory;
                switch (args.First())
                {
                    case "-upgrade":
                        Upgrade(projectDirectory);
                        break;
                    default:
                        Usage();
                        break;
                }
            }
        }

        static void Upgrade(string projectDirectory)
        {
            var clientDirectory = Path.Combine(projectDirectory, "ClientApp");
            var sourceDirectory = Path.Combine(clientDirectory, "src");
            var appDirectory = Path.Combine(sourceDirectory, "app");
            // MySQL Libraries
            Message("Info", "Upgrading - MySQL Libraries");
            RunShell("dotnet", "add package Microsoft.EntityFrameworkCore.Tools -v 2.1.0-preview1-final", projectDirectory);
            RunShell("dotnet", "add package Pomelo.EntityFrameworkCore.MySql", projectDirectory);
            // Angular Material Libraries
            Message("Info", "Upgrading - Angular Material Libraries");
            RunShell("npm", "install --save @angular/material @angular/cdk", clientDirectory);
            RunShell("npm", "install --save @angular/animations", clientDirectory);
            RunShell("npm", "install --save hammerjs", clientDirectory);
            // Angular Material File Templates
            Message("Info", "Upgrading - Angular Material File Templates");
            Append(Path.Combine(sourceDirectory, "styles.css"), "@import '~@angular/material/prebuilt-themes/indigo-pink.css';");
            Append(Path.Combine(sourceDirectory, "main.ts"), "import 'hammerjs';");
            InsertBefore(Path.Combine(sourceDirectory, "index.html"), "</head>", "  <link href='https://fonts.googleapis.com/icon?family=Material+Icons' rel='stylesheet'>");
            InsertBefore(Path.Combine(appDirectory, "app.module.ts"), "@NgModule", MatImports);
            InsertAfter(Path.Combine(appDirectory, "app.module.ts"), "imports: [", MatModules);
            Create(Path.Combine(appDirectory, "app.component.html"), AppHtml);
            Create(Path.Combine(appDirectory, "app.component.css"), AppCss);
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
            var text = File.ReadAllText(path) + Environment.NewLine + content;
            File.WriteAllText(path, text);
        }

        static void InsertBefore(string path, string label, string content)
        {
            var text = File.ReadAllText(path);
            content = Environment.NewLine + content + Environment.NewLine + label;
            text = text.Replace(label, content);
            File.WriteAllText(path, text);
        }

        static void InsertAfter(string path, string label, string content)
        {
            var text = File.ReadAllText(path);
            content = label + Environment.NewLine + content + Environment.NewLine;
            text = text.Replace(label, content);
            File.WriteAllText(path, text);
        }

        static void Message(string type, string content)
        {
            Console.WriteLine($"2mantools {type}: {content}");
        }

        static void Usage()
        {
            Console.WriteLine(UsageInfo);
            Thread.Sleep(20000);
        }

        #region Templates

        static string MatImports =
@"
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {
  MatAutocompleteModule,
  MatButtonModule,
  MatButtonToggleModule,
  MatCardModule,
  MatCheckboxModule,
  MatChipsModule,
  MatDatepickerModule,
  MatDialogModule,
  MatDividerModule,
  MatExpansionModule,
  MatGridListModule,
  MatIconModule,
  MatInputModule,
  MatListModule,
  MatMenuModule,
  MatNativeDateModule,
  MatPaginatorModule,
  MatProgressBarModule,
  MatProgressSpinnerModule,
  MatRadioModule,
  MatRippleModule,
  MatSelectModule,
  MatSidenavModule,
  MatSliderModule,
  MatSlideToggleModule,
  MatSnackBarModule,
  MatSortModule,
  MatStepperModule,
  MatTableModule,
  MatTabsModule,
  MatToolbarModule,
  MatTooltipModule
} from '@angular/material';
";

        static string MatModules =
@"
    BrowserAnimationsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatCheckboxModule,
    MatChipsModule,
    MatDatepickerModule,
    MatDialogModule,
    MatDividerModule,
    MatExpansionModule,
    MatGridListModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatNativeDateModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatRippleModule,
    MatSelectModule,
    MatSidenavModule,
    MatSliderModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatSortModule,
    MatStepperModule,
    MatTableModule,
    MatTabsModule,
    MatToolbarModule,
    MatTooltipModule,
";

        static string AppHtml =
@"
<mat-sidenav-container class='main-sidenav-container'>
  <mat-sidenav mode='side' opened class='main-sidenav'>
    <mat-nav-list>
      <a mat-list-item routerLink='/'>Home</a>
      <a mat-list-item routerLink='/fetch-data'>Fetch data</a>
      <a mat-list-item routerLink='/counter'>Counter</a>
    </mat-nav-list>
  </mat-sidenav>
  <mat-sidenav-content>
    <router-outlet></router-outlet>
  </mat-sidenav-content>
</mat-sidenav-container>
";

        static string AppCss =
@"
.main-sidenav-container {
  position: absolute;
  top: 0;
  bottom: 0;
  left: 0;
  right: 0;
}

.main-sidenav {
  width: 300px;
}
";

        static string UsageInfo =
@"
USAGE:
  2mantols -<option>
OPTIONS:
  2mantols -upgrade
    Upgrade the ASP.NET Core Angular project to 2MAN
";
        #endregion
    }
}
