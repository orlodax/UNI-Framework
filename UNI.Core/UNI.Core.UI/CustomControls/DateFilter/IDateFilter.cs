using UNI.Core.UI.Misc;

namespace UNI.Core.UI.CustomControls.DateFilter
{
    public interface IDateFilter<T> : IBaseSearch<T>
    {
        // component VM needs to instantiate
        DateFilter DateFilter { get; set; }
        DateFilterVM<T> DateFilterVM { get; set; }
    }
}
