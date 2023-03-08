using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace UNI.API.GenericControllerMapping;

public class ControllerMap : IApplicationFeatureProvider<ControllerFeature>
{
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        DirectoryInfo baseDirectory = new(AppContext.BaseDirectory);
        IEnumerable<FileInfo>? dlls = baseDirectory.GetFiles().Where(file => file.Extension == ".dll"
                                                                    && file.Name.Contains("Library")
                                                                    && file.Name != "UNI.Core.Library.dll");
        foreach (FileInfo file in dlls)
        {
            Assembly assembly = Assembly.Load(new AssemblyName() { Name = Path.GetFileNameWithoutExtension(file.Name) });

            foreach (Type type in assembly.GetExportedTypes().Where(t => t.IsSubclassOf(typeof(UNI.Core.Library.BaseModel))))
            {
                feature.Controllers.Add(typeof(Controllers.v1.GenericControllerV1<>).MakeGenericType(type).GetTypeInfo());
                feature.Controllers.Add(typeof(Controllers.v2.GenericControllerV2<>).MakeGenericType(type).GetTypeInfo());
            }
        }
    }
}