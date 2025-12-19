using Quokka;
using Quokka.ListItems;
using Quokka.PluginArch;

namespace PluginInstalledApps
{
  internal class AllAppsItem : ListItem
  {

    public AllAppsItem()
    {
      Name = "Applications";
      Description = "Shortcut to shell:appsFolder. Menu key will open installed apps settings.";
      Icon = IconCache.GetOrAdd(
        Environment.CurrentDirectory + "\\PlugBoard\\PluginInstalledApps\\Plugin\\apps.png"
      );
    }

    public override void Execute()
    {
      System.Diagnostics.Process.Start("explorer.exe", @" shell:appsFolder\");
      App.Current.MainWindow.Close();
    }
  }
}
