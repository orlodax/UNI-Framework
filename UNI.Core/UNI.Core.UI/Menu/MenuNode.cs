using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace UNI.Core.UI.Menu
{
    public class MenuNode
    {
        public string Name { get; set; }
        /// <summary>
        /// Side menu icon for single tabs
        /// </summary>
        public IconElement Icon { get; set; }
        public List<MenuNode> Children { get; set; } = new List<MenuNode>();

        /// <summary>
        /// Assign final extended VM to the View
        /// </summary>
        public Type ViewModelType { get; set; }
        /// <summary>
        /// Optional VM constructor parameters
        /// </summary>
        public object ArgumentObject { get; set; } = null;
    }
}
