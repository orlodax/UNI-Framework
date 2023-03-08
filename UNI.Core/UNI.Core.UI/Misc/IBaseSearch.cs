using System.Windows.Input;

namespace UNI.Core.UI.Misc
{
    public interface IBaseSearch<T>
    {
        string SearchText { get; set; }

        // this signals there is a command to be satisfied (and it needs to call the Perform Search in SearchFilter)
        ICommand FilterList { get; set; }
        void SearchCommand(object parameter);
    }
}
