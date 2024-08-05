using Quokka;
using Quokka.ListItems;
using System;
using System.Windows.Media.Imaging;

namespace Plugin_InstalledApps {
  internal class AllAppsItem : ListItem {

    public AllAppsItem() {
      Name = "Applications";
      Description = "Shortcut to shell:appsFolder. Menu key will open installed apps settings.";
      Icon = new BitmapImage(
          new Uri(Environment.CurrentDirectory + "\\PlugBoard\\Plugin_InstalledApps\\Plugin\\apps.png")
      );
      ;
    }

    public override void Execute() {
      System.Diagnostics.Process.Start("explorer.exe", @" shell:appsFolder\");
      App.Current.MainWindow.Close();
    }
  }
}
