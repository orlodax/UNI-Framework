using System;
using UNI.Core.Library;
using UNI.Core.UI.CustomEventArgs;
using UNI.Core.UI.MainPage;
using Windows.UI.Xaml;

namespace UNI.Core.UI.Tabs
{
    /// <summary>
    /// Shared logic for the type-agnostic part of the tabs-VMs
    /// </summary>
    public class BaseTabVMTypeAgnostic : Observable
    {
        public event EventHandler<ItemUpdatedEventArgs> ItemUpdated;
        public void OnItemUpdated(object sender, ItemUpdatedEventArgs e) => ItemUpdated?.Invoke(sender, e);

        /// <summary>
        /// The View to use for this tab
        /// </summary>
        public Type ViewType { get; set; }


        private Visibility exportButtonVisibility = Visibility.Collapsed;
        public Visibility ExportButtonVisibility { get => exportButtonVisibility; set => SetValue(ref exportButtonVisibility, value); }

        /// <summary>
        /// //Called from the content dialog manager when a content dialog is definitely closing
        /// </summary>
        internal virtual void CompleteClosing() { }
    }
}
