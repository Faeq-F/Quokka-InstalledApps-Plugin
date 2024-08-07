using Microsoft.WindowsAPICodePack.Shell;
using Newtonsoft.Json;
using Quokka.ListItems;
using Quokka.PluginArch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WinCopies.Util;

namespace Plugin_InstalledApps {

  /// <summary>
  ///   Interaction logic for plugin
  /// </summary>
  public partial class InstalledApps : IPlugger {

    internal static List<ListItem> ListOfSystemApps { private set; get; } = new List<ListItem>();
    internal static Settings PluginSettings { get; set; } = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Environment.CurrentDirectory
          + "\\PlugBoard\\Plugin_InstalledApps\\Plugin\\settings.json"))!;

    /// <summary>
    ///  <inheritdoc/>
    /// </summary>
    public string PluggerName { get; set; } = "InstalledApps";


    private static List<ListItem> RemoveBlacklistItems(List<ListItem> list) {
      foreach (string i in PluginSettings.BlackList) {
        list.RemoveAll(x => x.Name.Equals(i));
      }
      return list;
    }

    /// <summary>
    /// <inheritdoc/>
    /// Creates the list of all installed apps
    /// </summary>
    public void OnAppStartup() {
      // GUID taken from https://learn.microsoft.com/en-us/windows/win32/shell/knownfolderid
      var FOLDERID_AppsFolder = new Guid("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");
      ShellObject appsFolder = (ShellObject)
          KnownFolderHelper.FromKnownFolderId(FOLDERID_AppsFolder);

      foreach (var app in (IKnownFolder) appsFolder)
        ListOfSystemApps.Add(new InstalledAppsItem(app));
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="query">The app being searched for</param>
    /// <returns>List of InstalledApps that possibly match what is being searched for</returns>
    public List<ListItem> OnQueryChange(string query) {
      List<ListItem> IdentifiedApps = new();
      //filtering apps
      foreach (ListItem app in ListOfSystemApps) {
        if (
            app.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
            || ( FuzzySearch.LD(app.Name, query) < PluginSettings.FuzzySearchThreshold )
        ) {
          IdentifiedApps.Add(app);
        }
      }
      IdentifiedApps = RemoveBlacklistItems(IdentifiedApps);
      return IdentifiedApps;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="command">The 'all apps' command (Since there is only 1 special command for this plugin)</param>
    /// <returns>All Apps sorted alphabetically + a shortcut to shell:appsFolder</returns>
    public List<ListItem> OnSpecialCommand(string command) {
      List<ListItem> AllList = new(ListOfSystemApps);
      AllList = AllList.OrderBy(x => x.Name).ToList();
      AllList.Insert(0, new AllAppsItem());
      AllList = RemoveBlacklistItems(AllList);
      return AllList;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns>The AllAppsSpecialCommand from plugin settings</returns>
    public List<String> SpecialCommands() {
      List<String> SpecialCommand = new() { PluginSettings.AllAppsSpecialCommand };
      return SpecialCommand;
    }

    /// <summary>
    /// <inheritdoc/> Does Nothing.
    /// </summary>
    public void OnSearchWindowStartup() {
    }

    /// <summary>
    /// <inheritdoc/> Does Nothing.
    /// </summary>
    public void OnAppShutdown() {
    }

    /// <summary>
    /// Creates the plugin
    /// </summary>
    public InstalledApps() {
    }
  }
}