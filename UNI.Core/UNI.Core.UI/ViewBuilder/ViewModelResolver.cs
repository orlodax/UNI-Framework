using System;
using System.Collections.Generic;
using UNI.Core.UI.CustomControls.GridBox;
using UNI.Core.UI.CustomControls.ShowBox;
using UNI.Core.UI.NewItem;
using UNI.Core.UI.Tabs;

namespace UNI.Core.UI.ViewBuilder
{
    internal class ViewModelResolver
    {
        /// <summary>
        /// Modified on request in the final customized VM, select either injected custom VM/View of fallback to default element present in this assembly
        /// </summary>
        private readonly Dictionary<EnControlTypes, Type> viewModelsMap = new Dictionary<EnControlTypes, Type>();
        private readonly Dictionary<string, Type> viewModelsMapByPropertyName = new Dictionary<string, Type>();
        private readonly Dictionary<EnControlTypes, Type> newItemVMMap = new Dictionary<EnControlTypes, Type>();
        private readonly Dictionary<EnControlTypes, Type> editItemVMMap = new Dictionary<EnControlTypes, Type>();
        private readonly Dictionary<EnControlTypes, Type> showBoxVMMap = new Dictionary<EnControlTypes, Type>();

        #region Methods exposed in ViewBuilder
        /// Called optionally in the final VM to replace default VMs of Custom Controls with a BaseViewModel datacontext
        internal void CustomizeViewModel(EnControlTypes control, Type vm)
        {
            if (viewModelsMap.ContainsKey(control))
                viewModelsMap.Remove(control);

            viewModelsMap.Add(control, vm);
        }
        internal void CustomizeViewModelByPropertyName(string propertyName, Type vm)
        {
            if (viewModelsMapByPropertyName.ContainsKey(propertyName))
                viewModelsMapByPropertyName.Remove(propertyName);

            viewModelsMapByPropertyName.Add(propertyName, vm);
        }
        internal void CustomizeNewItemVM(EnControlTypes control, Type newItemVMType)
        {
            if (newItemVMMap.ContainsKey(control))
                newItemVMMap.Remove(control);

            newItemVMMap.Add(control, newItemVMType);
        }
        internal void CustomizeEditItemVM(EnControlTypes control, Type editItemVMType)
        {
            if (editItemVMMap.ContainsKey(control))
                editItemVMMap.Remove(control);

            editItemVMMap.Add(control, editItemVMType);
        }
        internal void CustomizeShowBoxVM(EnControlTypes control, Type showBoxVMType)
        {
            if (showBoxVMMap.ContainsKey(control))
                showBoxVMMap.Remove(control);

            showBoxVMMap.Add(control, showBoxVMType);
        }
        #endregion

        #region Get VMs (called by ControlsFactory)
        /// <summary>
        /// Retrieve customized value or provide default Core.UI type 
        /// </summary>
        /// <param name="controlType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        internal Type GetVMType(EnControlTypes controlType, string propertyName)
        {
            Type vmType = typeof(BaseTabVM<>);

            if (!viewModelsMapByPropertyName.TryGetValue(propertyName, out vmType))
            {
                switch (controlType)
                {
                    case EnControlTypes.GridBox:
                        if (!viewModelsMap.TryGetValue(EnControlTypes.GridBox, out vmType))
                            vmType = typeof(GridBoxVM<>);
                        break;

                    case EnControlTypes.GridBoxView:
                        if (!viewModelsMap.TryGetValue(EnControlTypes.GridBoxView, out vmType))
                            vmType = typeof(GridBoxViewVM<>);
                        break;

                    case EnControlTypes.GridBoxDataSet:
                        if (!viewModelsMap.TryGetValue(EnControlTypes.GridBoxDataSet, out vmType))
                            vmType = typeof(GridBoxDataSetVM<>);
                        break;

                    case EnControlTypes.GridBoxMtM:
                        if (!viewModelsMap.TryGetValue(EnControlTypes.GridBoxMtM, out vmType))
                            vmType = typeof(GridBoxMtMVM<>);
                        break;

                    case EnControlTypes.ShowBox:
                        if (!viewModelsMap.TryGetValue(EnControlTypes.ShowBox, out vmType))
                            vmType = typeof(ShowBoxVM<>);
                        break;
                }
            }
            return vmType;
        }

        /// <summary>
        /// Same as previous, retrieve customized value or retrieve default which is always NewItemVM
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        internal Type GetNewItemVMType(EnControlTypes controlType)
        {
            if (newItemVMMap.TryGetValue(controlType, out Type newItemVMType))
                return newItemVMType;
            else
                return typeof(NewItemVM<>);
        }

        /// <summary>
        /// Same as previous, retrieve customized value or retrieve default which is always EditItemVM
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        internal Type GetEditItemVMType(EnControlTypes controlType)
        {
            if (editItemVMMap.TryGetValue(controlType, out Type editItemVMType))
                return editItemVMType;
            else
                return typeof(EditItemVM<>);
        }
        /// <summary>
        /// TODO ma davero? Same as previous, retrieve customized value or retrieve default which is always ShowBoxVM
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        internal Type GetShowBoxVMType(EnControlTypes controlType)
        {
            if (showBoxVMMap.TryGetValue(controlType, out Type showBoxVMType))
                return showBoxVMType;
            else
                return typeof(ShowBoxVM<>);
        }
        #endregion
    }
}
