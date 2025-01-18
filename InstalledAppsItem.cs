using Microsoft.WindowsAPICodePack.Shell;
using Quokka;
using Quokka.ListItems;
using System.Diagnostics;
using System.IO;

namespace Plugin_InstalledApps {

  /// <summary>
  ///  The item class for the plugin - representing an app installed on the system
  /// </summary>
  internal class InstalledAppsItem : ListItem {

    public InstalledAppsItem(ShellObject app) {
      Name = app.Name!;
      Description = app.ParsingName; // or app.Properties.System.AppUserModel.ID

      Icon = InstalledApps.PluginSettings.IconSize switch {
        "ExtraLarge" => app.Thumbnail.ExtraLargeBitmapSource,
        "Large" => app.Thumbnail.LargeBitmapSource,
        "Small" => app.Thumbnail.SmallBitmapSource,
        _ => app.Thumbnail.MediumBitmapSource,
      };

      Path = app.Properties.System.Link.TargetParsingPath.Value;
      // link may be invalid or a shell link that cannot be used to get extra info
      try {
        if (Path != null && Path.Contains(":\\")) {
          Description = Path;
          if (FileVersionInfo.GetVersionInfo(Path).LegalCopyright != null) {
            ExtraDetails += FileVersionInfo.GetVersionInfo(Path).LegalCopyright + "\n";
          }
          if (FileVersionInfo.GetVersionInfo(Path).CompanyName != null) {
            ExtraDetails += FileVersionInfo.GetVersionInfo(Path).CompanyName + "\n";
          }
          if (FileVersionInfo.GetVersionInfo(Path).FileVersion != null) {
            ExtraDetails += FileVersionInfo.GetVersionInfo(Path).FileVersion + "\n";
          }
        }
      } catch (FileNotFoundException e) {
        App.ShowErrorMessageBox(e, "Invalid link");
      }
    }

    public string? ExtraDetails { get; set; }
    public string? Path { get; set; }

    public override void Execute() {
      if (Path != null && Path.Contains(":\\")) {
        Process.Start(Description);
      } else {
        System.Diagnostics.Process.Start("explorer.exe", @" shell:appsFolder\" + Description);
      }
      App.Current.MainWindow.Close();
    }
  }
}
