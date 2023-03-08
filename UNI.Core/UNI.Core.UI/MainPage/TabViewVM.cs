using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using UNI.Core.UI.Menu;
using UNI.Core.UI.Menu.Settings;
using UNI.Core.UI.Misc;
using UNI.Core.UI.NewItem;
using UNI.Core.UI.Tabs;
using Windows.ApplicationModel.Resources;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace UNI.Core.UI.MainPage
{
    public class TabViewVM : BaseTabVMTypeAgnostic
    {
        #region PROPERTIES

        /// <summary>
        /// All of the tabs ever opened will feed this + hidden ones 
        /// </summary>
        private ObservableCollection<TabViewItem> tabs;
        public ObservableCollection<TabViewItem> Tabs { get => tabs; set => SetValue(ref tabs, value); }
        private readonly List<TabViewItem> HiddenTabs = new List<TabViewItem>();

        /// <summary>
        /// The currently displayed tab 
        /// </summary>
        private TabViewItem selectedTab;
        public TabViewItem SelectedTab { get => selectedTab; set => SetValue(ref selectedTab, value); }

        /// <summary>
        /// Hide the TabView when it's empty
        /// </summary>
        private bool visibility;
        public bool Visibility { get => visibility; set => SetValue(ref visibility, value); }
        private readonly AppWindow NewWindow;
        #endregion

        public TabViewVM(AppWindow newWindow = null)
        {
            NewWindow = newWindow;

            Tabs = new ObservableCollection<TabViewItem>();
            Tabs.CollectionChanged += Tabs_CollectionChanged;

            allTabviews.Add(this);

            BindCommands();
        }

        /// <summary>
        /// Hide everything if empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tabs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Tabs.Any())
                Visibility = true;
            else
            {
                Visibility = false;

                // if it's a secondary tabview in a different window, close it when last tab is closed
                if (NewWindow != null)
                    _ = NewWindow.CloseAsync();
            }
        }

        #region COMMANDS

        /// <summary>
        /// Last of the tab movement events to be called, either creates new window or assign tab to other tabview according to the static fields information
        /// </summary>
        public ICommand TabDroppedOutside { get; private set; }

        /// <summary>
        /// Close the tab (click on its X button)
        /// </summary>
        public ICommand TabCloseRequested { get; set; }

        /// <summary>
        /// The click on the treeview item. Opens a new tab
        /// </summary>
        public ICommand NavigationMenuItemInvoked { get; private set; }

        /// <summary>
        /// When moving tab hovers on another tabview strip, enables the drop to the new tabview
        /// </summary>
        public ICommand TabStripDragOver { get; private set; }

        /// <summary>
        /// Called after movement ends but BEFORE TabDroppedOutside. If signaled by TabStripDragOver, removes tab from origin tabview and prepares flag for next event TabDroppedOutside
        /// </summary>
        public ICommand TabDragCompleted { get; private set; }
        /// <summary>
        /// Used to reset bool when tab movement starts
        /// </summary>
        public ICommand TabDragStarting { get; private set; }

        /// Static fields shared by all tabview instances used to comunicate tab movement
        static bool enableTabDock;
        static bool wasDroppedOnAnotherTabView;
        static TabViewItem movingTab = null;
        static TabViewVM tabViewOfDestination = null;
        static List<TabViewVM> allTabviews = new List<TabViewVM>();

        void BindCommands()
        {
            TabStripDragOver = new RelayCommand((parameter) =>
            {
                var args = parameter as DragEventArgs;
                tabViewOfDestination = (args.OriginalSource as Microsoft.UI.Xaml.Controls.Primitives.TabViewListView).DataContext as TabViewVM;
                enableTabDock = true;
            });

            TabDragCompleted = new RelayCommand((parameter) =>
            {
                var args = parameter as TabViewTabDragCompletedEventArgs;
                if (enableTabDock)
                {
                    Tabs.Remove(args.Tab);
                    movingTab = args.Tab;

                    enableTabDock = false;
                    wasDroppedOnAnotherTabView = true;
                }
            });

            TabDragStarting = new RelayCommand((parameter) => { wasDroppedOnAnotherTabView = false; });

            TabDroppedOutside = new RelayCommand(async (parameter) =>
            {
                if (!wasDroppedOnAnotherTabView)
                {
                    var tab = (parameter as TabViewTabDroppedOutsideEventArgs).Tab;

                    AppWindow newWindow = await AppWindow.TryCreateAsync();
                    newWindow.Closed += NewWindow_Closed;

                    // create new tabview and move tab to new VM
                    var tabViewVM = new TabViewVM(newWindow);
                    Tabs.Remove(tab);
                    tabViewVM.Tabs.Add(tab);
                    tabViewVM.SelectedTab = tab;

                    var newTabView = new TabView() { DataContext = tabViewVM };

                    ElementCompositionPreview.SetAppWindowContent(newWindow, newTabView);
                    await newWindow.TryShowAsync();
                }
                else
                {
                    if (tabViewOfDestination != null)
                    {
                        tabViewOfDestination.Tabs.Add(movingTab);
                        tabViewOfDestination.SelectedTab = movingTab;
                    }
                }
            });

            TabCloseRequested = new RelayCommand((parameter) =>
            {
                var tab = (parameter as TabViewTabCloseRequestedEventArgs).Tab;
                Tabs.Remove(tab);
                if (!HiddenTabs.Contains(tab))
                    HiddenTabs.Add(tab);
            });

            NavigationMenuItemInvoked = new RelayCommand((parameter) =>
            {
                var eventArgs = parameter as Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;

                // represents the tab to open
                var node = new MenuNode();

                string settingsLabel = ResourceLoader.GetForCurrentView().GetString("settings");
                if (eventArgs.InvokedItem.ToString() == "Settings" || eventArgs.InvokedItem.ToString() == settingsLabel)
                {
                    node.Name = string.IsNullOrWhiteSpace(settingsLabel) ? "Settings" : settingsLabel;
                    node.ViewModelType = typeof(SettingsPageVM);
                }
                else
                    node = eventArgs.InvokedItemContainer.DataContext as MenuNode;

                if (node.ViewModelType == null)
                    return;

                try
                {
                    // reopen hidden tab saves api calls but it's detrimental during debug and troubleshoot
                    #if !DEBUG
                    // if tab already exists among the closed ones, retrieve it
                    TabViewItem tabIsHidden = HiddenTabs.FirstOrDefault(t => t.Header.ToString() == node.Name);
                    if (tabIsHidden != null)
                    {
                        HiddenTabs.Remove(tabIsHidden);

                        if (!Tabs.Contains(tabIsHidden))
                        {
                            SelectedTab = tabIsHidden;
                            Tabs.Add(SelectedTab);
                        }
                    }
                    else
                    {
                        OpenNewTab(node.Name, node.ViewModelType, new object[] { node.ArgumentObject });
                    }
                    #endif
                    #if DEBUG
                    OpenNewTab(node.Name, node.ViewModelType, new object[] { node.ArgumentObject });
                    #endif
                }
                catch (Exception e)
                {
                    ShowContentDialog(new ContentDialog() { Title = "Error", Content = $"{e.Message}", CloseButtonText = "OK" });
                }
            });
        }

        private void NewWindow_Closed(AppWindow sender, AppWindowClosedEventArgs args)
        {
        }
#endregion

        /// <summary>
        /// Creates and opens a tab not tied to left side menu (i.e. Report)
        /// </summary>
        public TabViewItem OpenNewTab(string name, Type viewModelType, object[] args = null)
        {
            // Check if it isn't already open
            TabViewItem tabIsOpen = Tabs.FirstOrDefault(t => t.Header?.ToString() == name);

            // If it's open, select it
            if (tabIsOpen != null)
                SelectedTab = tabIsOpen;

            // create VM
            object anyArgument = args?.FirstOrDefault(a => a != null);
            object vm = null;

            if (anyArgument == null)
                vm = Activator.CreateInstance(viewModelType);
            else
            {
                System.Reflection.ConstructorInfo ctor = viewModelType.GetConstructors().FirstOrDefault(c => c.GetParameters().Count() == args.Length);
                if (ctor != null)
                    vm = ctor.Invoke(args);
            }

            if (vm != null)
            {
                // create View
                var baseVM = vm as dynamic;
                if (baseVM.ViewType != null)
                {
                    var view = Activator.CreateInstance(baseVM.ViewType) as FrameworkElement;
                    view.DataContext = baseVM;

                    // display in TabView
                    var tab = new TabViewItem
                    {
                        Header = name,
                        Content = view
                    };
                    Tabs.Add(tab);
                    SelectedTab = tab;

                }
                else
                    ShowContentDialog(new ContentDialog() { Title = "Error", Content = "Please assign a ViewType to the requested Tab", CloseButtonText = "OK" });
            }

            return SelectedTab;
        }

#region ContentDialogs stack
        /// <summary>
        /// There can be multiple content dialogs. Only ever show the last one, keep the other hidden below, show next at pop.
        /// </summary>
        static List<ContentDialog> ContentDialogs { get; set; } = new List<ContentDialog>();
        static ContentDialog ContentDialogShown = null;
        static ContentDialog ContentDialogBackup = null;

        /// <summary>
        /// Push ContentDialog to the dialogs stack and show it
        /// </summary>
        /// <param name="requestedContentDialog"></param>
        public static async void ShowContentDialog(ContentDialog requestedContentDialog)
        {
            if (!ContentDialogs.Contains(requestedContentDialog))
            {
                ContentDialogs.Add(requestedContentDialog);
                requestedContentDialog.Closed += ContentDialog_Closed;
            }

            if (ContentDialogShown == null)
            {
                ContentDialogShown = requestedContentDialog;
                await requestedContentDialog.ShowAsync();
            }
            else
            {
                ContentDialogBackup = ContentDialogShown;
                ContentDialogShown.Hide();

                ContentDialogShown = requestedContentDialog;
            }
        }

        /// <summary>
        /// Pop ContentDialog from stack (and close it)
        /// </summary>
        /// <param name="sender"> the closing content dialog</param>
        /// <param name="args"> event arguments </param>
        private static async void ContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            if (ContentDialogBackup != sender)
            {
                if (ContentDialogs.Contains(sender))
                {
                    ContentDialogs.Remove(sender);

                    if (sender.DataContext is BaseTabVMTypeAgnostic vm)
                        vm.CompleteClosing();
                }
            }
            else
            {
                ContentDialogBackup = null;
            }

            if (ContentDialogs.Any())
            {
                try
                {
                    ContentDialogShown = ContentDialogs.Last();
                    await ContentDialogShown.ShowAsync();
                }
                catch (Exception)
                {
                    foreach (var cd in ContentDialogs)
                        if (cd.DataContext is BaseTabVMTypeAgnostic vm)
                            vm.CompleteClosing();

                    ContentDialogs.Clear();
                    ContentDialogShown = null;
                }
            }
            else
            {
                ContentDialogShown = null;
            }
        }
#endregion
    }
}
