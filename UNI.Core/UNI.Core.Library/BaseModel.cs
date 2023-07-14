using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UNI.Core.Library.AttributesMetadata;

namespace UNI.Core.Library
{
    [Serializable]
    public class BaseModel : Observable
    {
        #region DATA FIELDS

        [ValueInfo(SQLName = "id", IsVisible = false)]
        public int ID { get; set; }

        [ValueInfo(SQLName = "created", IsVisible = false)]
        public DateTime Created { get; set; }

        [ValueInfo(SQLName = "lastmodify", IsVisible = false)]
        public DateTime LastModify { get; set; }

        [ValueInfo(SQLName = "idMtm", IsVisible = false)]
        public int IdMtm { get; set; }

        [ValueInfo(SQLName = "interfacerefertable", IsVisible = false)]
        public string InterfaceReferTable { get; set; }

        [ValueInfo(SQLName = "interfacereferid", IsVisible = false)]
        public string InterfaceReferId { get; set; }

        #endregion

        #region METADATA
        /// <summary>
        /// TODO this will replace the attributes
        /// </summary>
        public BaseModelMetadata Metadata { get; set; } = new BaseModelMetadata();

        protected void AddDataAttribute(string propertyName, DataAttributes dataAttributes)
        {
            if (!Metadata.DataAttributes.ContainsKey(propertyName))
                Metadata.DataAttributes.Add(propertyName, dataAttributes);
        }
        protected void AddGraphicAttribute(string propertyName, GraphicAttributes dataAttribute)
        {
            if (!Metadata.GraphicAttributes.ContainsKey(propertyName))
                Metadata.GraphicAttributes.Add(propertyName, dataAttribute);
        }
        protected void AddClassAttribute(ClassAttributes classAttributes)
        {
            Metadata.ClassAttributes = classAttributes;
        }

        /// Enforce population of attributes with following 3 methods - TODO decomment when ready to move to new attribute system
        public virtual BaseModel InitAttributes() { return this; }
        //protected abstract void AddClassAttributes();
        //protected abstract void AddDataAttributes();
        //protected abstract void AddGraphicAttributes();
        #endregion

        #region STATE FUNCTIONS
        /// <summary>
        /// TODO: muovi questo nei costruttori dei basemodel? O magari no?
        /// Called when a base model is loaded, also when is part of another object. Please do not use heavy calculation in this method
        /// </summary>
        /// <param name="parentItem"></param>
        public virtual BaseModel Loaded(BaseModel parentItem = null)
        {
            Logic();
            InitModel(parentItem);
            NotifyPropertyChangedGlobal();

            return this;
        }

        /// <summary>
        /// Called when a base model is updated in a view, the main function is to reset the object
        /// </summary>
        /// <param name="parentItem"></param>
        public virtual void Updated(BaseModel parentItem = null)
        {
            Reset();
            Loaded(parentItem);
        }

        public void InitModel(BaseModel parentItem = null)
        {
            if (parentItem != null)
            {
                foreach (var prop in GetType().GetProperties())
                {
                    List<Type> extendedTypes = UtilityMethods.FindAllParentsTypes(parentItem.GetType());
                    extendedTypes.Add(parentItem.GetType());

                    foreach (var type in extendedTypes)
                        if (prop.Name.Equals($"Id{type.Name}"))
                            prop.SetValue(this, parentItem.ID);

                    var valueInfo = (ValueInfo)prop.GetCustomAttribute(typeof(ValueInfo));
                    if (valueInfo == null || string.IsNullOrWhiteSpace(valueInfo.ParentPropertyDependendency))
                        continue;

                    var parentProperty = parentItem.GetType().GetProperties().First(p => p.Name == valueInfo.ParentPropertyDependendency);
                    var value = parentProperty?.GetValue(parentItem);
                    if (value != null)
                        prop.SetValue(this, value);
                }
            }

            if (GetType().GetCustomAttribute(typeof(ClassInfo)) is ClassInfo classInfo)
            {
                if (!string.IsNullOrWhiteSpace(classInfo.ClassType))
                {
                    var classInfoProperty = GetType().GetProperties().FirstOrDefault(p => p.Name == "ClassInfo");
                    classInfoProperty?.SetValue(this, classInfo.ClassType);
                }
            }

            this?.Logic();
        }

        /// <summary>
        /// Assign the basemodel id to its id-named properties 
        /// </summary>
        /// <returns> this (chainable) </returns>
        private BaseModel Logic()
        {
            var properties = GetType().GetProperties();
            foreach (var pro in properties)
            {
                if (pro.PropertyType.IsGenericType || !pro.PropertyType.IsSubclassOf(typeof(BaseModel)))
                    continue;

                BaseModel baseModel = (BaseModel)pro.GetValue(this);
                if (baseModel == null)
                    continue;

                baseModel.Logic();
                var idPro = properties.First(p => p.Name == $"Id{pro.Name}");
                idPro?.SetValue(this, baseModel?.ID ?? 0);
            }
            return this;
        }

        private BaseModel Reset()
        {
            if (this == null)
                return this;

            try
            {
                BaseModel obj = (BaseModel)Activator.CreateInstance(GetType());

                foreach (var property in GetType().GetProperties())
                {
                    if (property.PropertyType.IsGenericType && !property.PropertyType.Name.Contains("UniDataSet"))
                    {
                        if (property.PropertyType.GenericTypeArguments[0].IsSubclassOf(typeof(BaseModel)))
                        {
                            IList value = (IList)property.GetValue(this);
                            if (value != null)
                                foreach (var entry in value)
                                    (entry as BaseModel)?.Reset();
                        }
                        else
                        {
                            IList value = (IList)property.GetValue(obj);
                            property.SetValue(this, value);
                        }

                    }
                    else if (property.PropertyType.IsSubclassOf(typeof(BaseModel)))
                    {
                        var value = property.GetValue(this);
                        (value as BaseModel)?.Reset();
                    }
                    else
                    {
                        if (property.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                        {
                            if (valueInfo.IsReadOnly)
                            {
                                var value = property.GetValue(obj);
                                property.SetValue(this, value);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return this;
        }
    }
    #endregion
}
