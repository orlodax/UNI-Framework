namespace UNI.Core.UI.Test
{
    internal class ViewModelDerivato : ViewModelBase
    {
        public ViewModelDerivato()
        {
            VBBase = new ViewBuilderDerivato();
        }
    }
}
