using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace UNI.API.GenericControllerMapping;

public class ControllerRouteMap : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (controller.ControllerType.IsGenericType)
            controller.ControllerName = controller.ControllerType.GenericTypeArguments[0].Name;

        // if ever needed, use custom attribute (to be created in library) to customize names
        //var genericType = controller.ControllerType.GenericTypeArguments[0];
        //var customNameAttribute = genericType.GetCustomAttribute<GeneratedControllerAttribute>();

        //if (customNameAttribute?.Route != null)
        //{
        //    controller.Selectors.Add(new SelectorModel
        //    {
        //        AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(customNameAttribute.Route)),
        //    });
        //}
        //else
        //{
        //controller.ControllerName = genericType.Name;
        //}
    }
}
