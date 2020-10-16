using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Views;
using System;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public abstract class CommonFormatEditorFunctionality : CommonSettingsFunctionality
  {
    #region Members

    protected event EventHandler CloseMultipleInput;
    protected List<IFormatOption> formatStyleOptions;
    protected IFormatOption selectedOption;
    protected EditorStyles selectedStyle = EditorStyles.Custom;

    #endregion

    #region Protected Methods

    protected void OpenMultipleInput(IFormatOption selectedOption)
    {
      if (selectedOption is FormatOptionMultipleInputModel multiInputModel)
      {
        OpenInputDataView(multiInputModel);
      }
      else if (selectedOption is FormatOptionMultipleToggleModel multiToggelModel)
      {
        OpenToggleDataView(multiToggelModel);
      }
    }

    #endregion

    #region Private Methods

    private void OpenInputDataView(FormatOptionMultipleInputModel multipleInputModel)
    {
      var inputMultipleDataView = new InputMultipleDataView(multipleInputModel.MultipleInput);
      inputMultipleDataView.Closed += CloseInputDataView;
      inputMultipleDataView.Show();
    }

    private void OpenToggleDataView(FormatOptionMultipleToggleModel multipleToggleModel)
    {
      var toggleMultipleDataView = new ToggleMultipleDataView(multipleToggleModel.ToggleFlags);
      toggleMultipleDataView.Closed += CloseToggleDataView;
      toggleMultipleDataView.Show();
    }

    private void CloseInputDataView(object sender, EventArgs e)
    {
      var multipleInputModel = (FormatOptionMultipleInputModel)selectedOption;
      var inputMultipleDataView = (InputMultipleDataView)sender;
      multipleInputModel.MultipleInput = ((InputMultipleDataViewModel)inputMultipleDataView.DataContext).Input;
      inputMultipleDataView.Closed -= CloseInputDataView;
      if (CloseMultipleInput != null)
      {
        CloseMultipleInput.Invoke(sender, e);
      }
    }

    private void CloseToggleDataView(object sender, EventArgs e)
    {
      var multipleToggleModel = (FormatOptionMultipleToggleModel)selectedOption;
      var toggleMultipleDataView = (ToggleMultipleDataView)sender;
      multipleToggleModel.ToggleFlags = ((ToggleMultipleDataViewModel)toggleMultipleDataView.DataContext).Input;
      toggleMultipleDataView.Closed -= CloseInputDataView;
      if (CloseMultipleInput != null)
      {
        CloseMultipleInput.Invoke(sender, e);
      }
    }

    #endregion
  }
}
