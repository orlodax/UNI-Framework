using System;
using System.Collections.Generic;

namespace UNI.Core.Library
{
    public static class UtilityMethods
    {
        public static List<Type> FindAllParentsTypes(Type typeToIterate)
        {
            var types = new List<Type>();
            bool iterate = true;
            do
            {
                Type baseType = typeToIterate.BaseType;
                if (baseType != null)
                {
                    types.Add(baseType);
                    typeToIterate = baseType;
                }
                else
                    iterate = false;
            }
            while (iterate);

            return types;
        }
    }
}
