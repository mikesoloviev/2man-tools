using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X2MANTools
{
    public class Templates
    {

        public static string MatTheme =
@"
@import '~@angular/material/prebuilt-themes/indigo-pink.css';
";

        public static string MatHammer =
@"
import 'hammerjs';"
;

        public static string MatIcons =
@"
<link href='https://fonts.googleapis.com/icon?family=Material+Icons' rel='stylesheet'>
";

        public static string MatImports =
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

        public static string MatModules =
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

        public static string AppHtml =
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

        public static string AppCss =
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

        public static string UsageInfo =
@"
USAGE:
  2mantols -<option>
OPTIONS:
  2mantols -upgrade
    Upgrade the ASP.NET Core Angular project to 2MAN
";

    }
}
