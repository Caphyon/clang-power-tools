using System;
using System.Collections;
using System.ComponentModel;

namespace ClangPowerTools
{
  public class ComboBoxConvertor : TypeConverter
  {
    #region Members 

    protected ArrayList mValues;

    #endregion

    #region TypeConverter Implementation

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
      return true;
    }

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
      return new StandardValuesCollection(mValues);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      if (sourceType == typeof(string))
        return true;
      return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
      string s = value as string;
      if (s == null)
        return base.ConvertFrom(context, culture, value);
      return value;
    }

    #endregion

  }

}
