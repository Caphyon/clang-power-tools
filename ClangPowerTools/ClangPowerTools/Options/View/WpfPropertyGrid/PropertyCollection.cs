using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace Caphyon.AdvInstVSIntegration.ProjectEditor.View.WpfPropertyGrid
{
  class PropertyCollection: ObservableCollection<PropertyData>
  {
    public PropertyCollection(object obj)
    {
      // Get the type
      System.Type objType = obj.GetType();
      // Get the property attributes
      PropertyDescriptorCollection propDescriptions = TypeDescriptor.GetProperties(obj);
      foreach (PropertyDescriptor propDescript in propDescriptions)
      {
        if (propDescript.IsBrowsable)
        {
          // Get the property info by reflection
          PropertyInfo propInfo = objType.GetProperty(propDescript.Name);
          if (null != propInfo)
          {
            PropertyData prop = new PropertyData(obj, propInfo, propDescript);
            // Add the pair (element, property) to collection
            Add(prop);
          }
        }
      }
    }
  }
}
