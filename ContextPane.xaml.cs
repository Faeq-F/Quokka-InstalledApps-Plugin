using Quokka;
using Quokka.ListItems;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Plugin_InstalledApps {

  /// <summary>
  ///  The context pane for an InstalledAppsItem. Launches installed apps settings if the AllAppsItem was selected
  /// </summary>
  public partial class ContextPane : ItemContextPane {

    private readonly InstalledAppsItem? Item;

    /// <summary>
    /// Grabs details about the selected app and launches settings if the selected item was the AllAppsItem
    /// </summary>
    public ContextPane() {
      InitializeComponent();

      try {
        Item = (InstalledAppsItem) ( (SearchWindow) Application.Current.MainWindow ).SelectedItem!;
      } catch (InvalidCastException) { //Used to handle the AllAppsItem
        Process.Start("ms-settings:appsfeatures");
        App.Current.MainWindow.Close();
        return;
      }
      DetailsImage.Source = Item.Icon;
      NameText.Text = Item.Name;
      DescText.Text = Item.Description;
      ExtraDetails.Text = Item.ExtraDetails;
    }

    private void OpenApp(object sender, RoutedEventArgs e) {
      Item!.Execute();
    }

    private void OpenContainingFolder(object sender, RoutedEventArgs e) {
      try {
        using Process folderopener = new();
        folderopener.StartInfo.FileName = (string) App.Current.Resources["FileManager"];
        folderopener.StartInfo.Arguments = '"' + Item!.Path?.Remove(Item.Path.LastIndexOf('\\')) + '"';
        folderopener.Start();
        App.Current.MainWindow.Close();
      } catch (Exception ex) { App.ShowErrorMessageBox(ex, "Containing folder could not be opened - the app may not be compatible with this action (if it is a UWP app)"); }
    }

    /// <summary>
    /// <inheritdoc/><br />
    /// Up and down keys select list items and the enter key executes the item's action
    /// </summary>
    /// <param name="sender"><inheritdoc/></param>
    /// <param name="e"><inheritdoc/></param>
    protected override void Page_KeyDown(object sender, KeyEventArgs e) {
      ButtonsListView.Focus();
      switch (e.Key) {
        case Key.Enter:
          if (( ButtonsListView.SelectedIndex == -1 )) ButtonsListView.SelectedIndex = 0;
          Grid CurrentItem = (Grid) ButtonsListView.SelectedItem;
          Button CurrentButton = (Button) ( (Grid) CurrentItem.Children[1] ).Children[0];
          CurrentButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
          break;

        case Key.Down:
          if (( ButtonsListView.SelectedIndex == -1 )) {
            ButtonsListView.SelectedIndex = 1;
          } else if (ButtonsListView.SelectedIndex == ButtonsListView.Items.Count - 1) {
            ButtonsListView.SelectedIndex = 0;
          } else {
            ButtonsListView.SelectedIndex++;
          }
          ButtonsListView.ScrollIntoView(ButtonsListView.SelectedItem);
          break;

        case Key.Up:
          if (( ButtonsListView.SelectedIndex == -1 ) || ( ButtonsListView.SelectedIndex == 0 )) {
            ButtonsListView.SelectedIndex = ButtonsListView.Items.Count - 1;
          } else {
            ButtonsListView.SelectedIndex--;
          }
          ButtonsListView.ScrollIntoView(ButtonsListView.SelectedItem);
          break;

        case Key.Apps: //This is the menu key
          base.ReturnToSearch();
          break;

        default:
          return;
      }
      e.Handled = true;
    }

    private void RunAsAdmin(object sender, RoutedEventArgs e) {
      try {
        Process proc = new Process();
        proc.StartInfo.FileName = Item!.Description;
        proc.StartInfo.UseShellExecute = true;
        proc.StartInfo.Verb = "runas";
        proc.Start();
        App.Current.MainWindow.Close();
      } catch (Exception ex) { App.ShowErrorMessageBox(ex, "App could not be launched as admin - the app may not be compatible with this action (if it is a UWP app)"); }
    }
  }
}