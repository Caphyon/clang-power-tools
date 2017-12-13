using System;
using System.Collections;
using System.ComponentModel;

namespace ClangPowerTools
{
  public class TidyModeConvertor : TypeConverter
  {
    #region Members 

    protected ArrayList values;

    #endregion

    #region Public Methods

    public TidyModeConvertor()
    {
      values = new ArrayList(new string[] 
      {
        TidyModeConstants.kCustomChecks,
        TidyModeConstants.kPredefinedChecks,
        TidyModeConstants.kTidyFile
      });
    }

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
      return true;
    }

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
      return new StandardValuesCollection(values);
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
