/*
 * Copyright 2002 - 2011 Caphyon LTD. All rights reserved.
 *
 * mailto: eng@caphyon.com
 * http://www.caphyon.com
 *
 */
using System.Windows;
using System.Windows.Media;

namespace Caphyon.AdvInstVSIntegration.ProjectEditor.View.WpfPropertyGrid
{
  /**
   * Search in visual hierarchy a parent with a specified type
   * for an object. If a such parent cannot be found then
   * the function returns null.
   */
  public static class VisualTreeHelperExtension
  {
    public static T FindAncestor<T>(DependencyObject dependencyObject)
        where T : class
    {
      DependencyObject target = dependencyObject;
      do
      {
        target = VisualTreeHelper.GetParent(target);
      }
      while (target != null && !(target is T));
      return target as T;
    }
  }
}
