using Microsoft.WindowsAPICodePack.Shell;
using Newtonsoft.Json;
using Quokka.ListItems;
using Quokka.PluginArch;
using System.Collections.ObjectModel;
using System.IO;
using WinCopies.Util;

namespace PluginInstalledApps
{

  /// <summary>
  ///  The Installed Apps plugin
  /// </summary>
  public partial class InstalledApps : Plugin
  {

    internal static Collection<ListItem> AllSystemApps { private set; get; } = new();
    internal static Settings PluginSettings { get; set; } = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Environment.CurrentDirectory
          + "\\PlugBoard\\PluginInstalledApps\\Plugin\\settings.json"))!;

    /// <summary>
    ///  <inheritdoc/>
    /// </summary>
    public override string PluginName { get; set; } = "InstalledApps";


    private static Collection<ListItem> RemoveBlacklistItems(List<ListItem> list)
    {
      foreach (string i in PluginSettings.BlackList)
      {
        list.RemoveAll(x => x.Name.Equals(i, StringComparison.Ordinal));
      }
      return new Collection<ListItem>(list);
    }

    /// <summary>
    /// <inheritdoc/>
    /// Creates the collection of all installed apps
    /// </summary>
    public override void OnAppStartup()
    {
      // GUID taken from https://learn.microsoft.com/en-us/windows/win32/shell/knownfolderid
      var FOLDERID_AppsFolder = new Guid("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");
      ShellObject appsFolder = (ShellObject)
          KnownFolderHelper.FromKnownFolderId(FOLDERID_AppsFolder);

      foreach (var app in (IKnownFolder)appsFolder)
        AllSystemApps.Add(new InstalledAppsItem(app));
    }

    private static Collection<ListItem> ProduceItems(string query)
    {
      Collection<ListItem> IdentifiedApps = new();
      IdentifiedApps.AddRange(
      FuzzySearch.SearchAll(query, AllSystemApps, PluginSettings.FuzzySearchThreshold)
      );
      IdentifiedApps = RemoveBlacklistItems(IdentifiedApps.ToList());
      return IdentifiedApps;
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="query">The app being searched for</param>
    /// <returns>Collection of InstalledApps that possibly match what is being searched for</returns>
    public override Collection<ListItem> OnQueryChange(string query)
    {
      return ProduceItems(query);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="command">The AllAppsSpecialCommand from plugin settings (Since there is only 1 special command for this plugin)</param>
    /// <returns>All Apps sorted alphabetically + a shortcut to shell:appsFolder</returns>
    public override Collection<ListItem> OnSpecialCommand(string command)
    {
      List<ListItem> AllList = new(AllSystemApps);
      AllList = AllList.OrderBy(x => x.Name).ToList();
      AllList.Insert(0, new AllAppsItem());
      return RemoveBlacklistItems(AllList);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns>The AllAppsSpecialCommand from plugin settings</returns>
    public override Collection<String> SpecialCommands()
    {
      Collection<String> SpecialCommand = new() { PluginSettings.AllAppsSpecialCommand };
      return SpecialCommand;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns>
    /// The InstalledAppsSignifier from plugin settings
    /// </returns>
    public override Collection<string> CommandSignifiers()
    {
      return new Collection<string>() { PluginSettings.InstalledAppsSignifier };
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="command">The InstalledAppsSignifier (Since there is only 1 signifier for this plugin), followed by the app being searched for</param>
    /// <returns>Collection of InstalledApps that possibly match what is being searched for</returns>
    public override Collection<ListItem> OnSignifier(string command)
    {
      command ??= "";
      command = command.Substring(PluginSettings.InstalledAppsSignifier.Length);
      return FuzzySearch.Sort(command, ProduceItems(command));
    }

  }
}
