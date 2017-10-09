using System;
using System.ComponentModel;
using System.Globalization;

namespace ClangPowerTools
{
  public class StringArrayConverter : TypeConverter
  {
    #region Members

    private const string kDelimiter = ";";

    #endregion

    #region Public Methods

    public override bool CanConvertFrom(ITypeDescriptorContext aContext, Type aSourceType) =>
      typeof(string) == aSourceType || base.CanConvertFrom(aContext, aSourceType);
    
    public override bool CanConvertTo(ITypeDescriptorContext aContext, Type aDestinationType) =>
      typeof(string[]) == aDestinationType || base.CanConvertTo(aContext, aDestinationType);
    
    public override object ConvertFrom(ITypeDescriptorContext aContext, CultureInfo aCulture, object aValue)
    {
      string valueStr = aValue as string;
      return null == valueStr ? base.ConvertFrom(aContext, aCulture, aValue) : valueStr.Split(new[] { kDelimiter }, StringSplitOptions.RemoveEmptyEntries);
    }

    public override object ConvertTo(ITypeDescriptorContext aContext, CultureInfo aCulture, object aValue, Type aDestinationType)
    {
      string[] values = aValue as string[];
      if (typeof(string) != aDestinationType || null == values)
        return base.ConvertTo(aContext, aCulture, aValue, aDestinationType);
      return string.Join(kDelimiter, values);
    }

    #endregion

  }
}
