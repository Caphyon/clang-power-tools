using System;

namespace ClangPowerTools
{
  [AttributeUsage(AttributeTargets.Property)]
  public class TextBoxAndBrowseAttribute : Attribute
  {
    public bool Activate { get; set; }

    public TextBoxAndBrowseAttribute(bool aActivate)
    {
      Activate = aActivate;
    }
  }
}
