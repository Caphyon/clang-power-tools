using ClangPowerTools;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Caphyon.AdvInstVSIntegration.ProjectEditor.View.WpfPropertyGrid
{
  /// <summary>
  /// Stores a pair composed from an object and one of its properties
  /// </summary>
  public class PropertyData: INotifyPropertyChanged
  {
    #region Private members
    /// <summary>
    /// The object that has the property
    /// (used to get/set the value of the property)
    /// </summary>
    private object mOwner;
    /// <summary>
    /// A description of the property
    /// </summary>
    private System.ComponentModel.PropertyDescriptor mPropertyDescript;
    /// <summary>
    /// Reflection information about property
    /// </summary>
    private System.Reflection.PropertyInfo mPropertyInfo;
    #endregion

    /// <summary>
    ///
    /// </summary>
    /// <param name="owner">Object used to get/set the property value</param>
    /// <param name="propInfo">Information about the property</param>
    public PropertyData(object owner, PropertyInfo propInfo, PropertyDescriptor propDescript)
    {
      mOwner = owner;
      mPropertyInfo = propInfo;
      mPropertyDescript = propDescript;
      // Track changes in owner if possible
      INotifyPropertyChanged propChangeSource = mOwner as INotifyPropertyChanged;
      if (null != propChangeSource)
      {
        propChangeSource.PropertyChanged += PropChangeSource_PropertyChanged;
      }
    }

    ~PropertyData()
    {
      INotifyPropertyChanged propChangeSource = mOwner as INotifyPropertyChanged;
      if (null != propChangeSource)
      {
        propChangeSource.PropertyChanged -= PropChangeSource_PropertyChanged;
      }
    }

    void PropChangeSource_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (mPropertyDescript.Name == e.PropertyName)
      {
        OnPropertyChanged("Value");
      }
    }

    /// <summary>
    /// Provides the type of the property from pair
    /// </summary>
    public Type PropertyType
    {
      get
      {
        return mPropertyInfo.PropertyType;
      }
    }

    

    /// <summary>
    /// Check if the property has a certain attribute
    /// </summary>
    public bool HasTextBoxAndBrowseAttribute
    {
      get
      {
        object[] propAttrs = mPropertyInfo.GetCustomAttributes(false);
        object clangCheckAttr = propAttrs.FirstOrDefault(attr => typeof(ClangFormatPathAttribute) == attr.GetType());
        object displayNameAttrObj = propAttrs.FirstOrDefault(attr => typeof(DisplayNameAttribute) == attr.GetType());

        if (null == clangCheckAttr || null == displayNameAttrObj)
          return false;

        return true;
      }
    }



    /// <summary>
    /// Category where the property is placed
    /// </summary>
    public virtual string Category
    {
      get
      {
        return mPropertyDescript.Category;
      }
    }

    /// <summary>
    /// Indicates if the property can be modified from
    /// property viewer or not
    /// </summary>
    public bool IsReadOnly
    {
      get
      {
        return mPropertyDescript.IsReadOnly || !mPropertyInfo.CanWrite;
      }
    }

    /// <summary>
    /// The name that will be displayed in the property window
    /// </summary>
    public string DisplayName
    {
      get
      {
        return mPropertyDescript.DisplayName;
      }
    }

    /// <summary>
    /// Description provided for the property
    /// </summary>
    public string Description
    {
      get
      {
        return mPropertyDescript.Description;
      }
    }

    /// <summary>
    /// Name of the template used to edit the string
    /// </summary>
    public string EditorName
    {
      get
      {
        string editorName = "";
        try
        {
          EditorAttribute editAttribute = (EditorAttribute)mPropertyDescript.Attributes[typeof(EditorAttribute)];
          if (null != editAttribute)
          {
            editorName = editAttribute.EditorTypeName;
          }
          else
          {
            editorName = "";
          }
        }
        catch (Exception)
        {
        	editorName = "";
        }
        return editorName;
      }
    }

    /// <summary>
    /// Getter/Setter for the value of a property
    /// </summary>
    public object Value
    {
      get
      {
        return mPropertyInfo.GetValue(mOwner, null);
      }
      set
      {
        mPropertyInfo.SetValue(mOwner, value);
        OnPropertyChanged("Value");
      }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      if (null != PropertyChanged)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }
}
