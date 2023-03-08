namespace UNI.Core.UI.Misc.SearchFilter
{
    public interface ISearchFilter<T> : IBaseSearch<T>
    {
        SearchFilter<T> SearchFilter { get; set; }
    }
}
