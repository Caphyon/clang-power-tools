using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Process = System.Diagnostics.Process;

namespace ClangPowerTools
{
  public class FormatEditorViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private readonly FormatEditorController formatEditorController;
    private readonly FormatEditorView formatOptionsView;
    private InputMultipleDataView inputMultipleDataView;
    private ICommand selctCodeFileCommand;
    private ICommand createFormatFileCommand;
    private ICommand formatCodeCommand;
    private ICommand resetCommand;
    private ICommand openUri;
    private ICommand openMultipleInputCommand;
    private ICommand resetSearchCommand;

    private string checkSearch = string.Empty;
    private IFormatOption selectedOption;
    private List<IFormatOption> formatStyleOptions;
    private List<IFormatOption> searchResultFormatStyleOptions;
    private EditorStyles editorStyle = EditorStyles.Custom;
    private bool windowLoaded = false;
    private string nameColumnWidth;
    private string droppedFile;
    private const string autoSize = "auto";
    private const string nameColumnWidthMax = "340";
    public const string FileExtensionsSelectFile = "Code files (*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hh;*.hpp;*.hxx;)|*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hh;*.hpp;*.hxx";

    #endregion

    #region Constructor

    public FormatEditorViewModel(FormatEditorView formatOptionsView)
    {
      formatOptionsView.Loaded += EditorLoaded;
      this.formatOptionsView = formatOptionsView;
      formatEditorController = new FormatEditorController();
      InitializeStyleOptions(FormatOptionsProvider.CustomOptionsData);
    }

    #endregion

    #region Properties

    public List<IFormatOption> FormatOptions
    {
      get
      {
        if (string.IsNullOrWhiteSpace(checkSearch))
        {
          return formatStyleOptions;
        }
        return searchResultFormatStyleOptions;
      }
      set
      {
        formatStyleOptions = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatOptions"));
      }
    }

    public string CheckSearch
    {
      get
      {
        return checkSearch;
      }
      set
      {
        checkSearch = value;
        FindFormatStyleOptionsAsync(checkSearch).SafeFireAndForget();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CheckSearch"));
      }
    }

    public IFormatOption SelectedOption
    {
      get
      {
        return selectedOption;
      }
      set
      {
        selectedOption = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedOption"));
      }
    }

    public IEnumerable<EditorStyles> Styles
    {
      get
      {
        return Enum.GetValues(typeof(EditorStyles)).Cast<EditorStyles>();
      }
    }

    public IEnumerable<ToggleValues> BooleanComboboxValues
    {
      get
      {
        return Enum.GetValues(typeof(ToggleValues)).Cast<ToggleValues>();
      }
    }

    public EditorStyles SelectedStyle
    {
      get
      {
        FindFormatStyleOptionsAsync(checkSearch).SafeFireAndForget();
        return editorStyle;
      }
      set
      {
        editorStyle = value;
        ChangeControlsDependingOnStyle();

        RunFormat();
      }
    }

    public string NameColumnWidth
    {
      get
      {
        return nameColumnWidth;
      }
      set
      {
        nameColumnWidth = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NameColumnWidth"));
      }
    }

    public string EnableOptionColumnWidth
    {
      get
      {
        return nameColumnWidth;
      }
      set
      {
        nameColumnWidth = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EnableOptionColumnWidth"));
      }
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion


    #region Commands

    public ICommand CreateFormatFileCommand
    {
      get => createFormatFileCommand ?? (createFormatFileCommand = new RelayCommand(() => CreateFormatFile(), () => CanExecute));
    }

    public ICommand FormatCodeCommand
    {
      get => formatCodeCommand ?? (formatCodeCommand = new RelayCommand(() => RunFormat(), () => CanExecute));
    }

    public ICommand OpenClangFormatUriCommand
    {
      get => openUri ?? (openUri = new RelayCommand(() => OpenUri("https://clang.llvm.org/docs/ClangFormatStyleOptions.html"), () => CanExecute));
    }

    public ICommand ResetCommand
    {
      get => resetCommand ?? (resetCommand = new RelayCommand(() => ResetOptions(), () => CanExecute));
    }

    public ICommand SelctCodeFileCommand
    {
      get => selctCodeFileCommand ?? (selctCodeFileCommand = new RelayCommand(() => ReadCodeFromFile(), () => CanExecute));
    }

    public ICommand OpenMultipleInputCommand
    {
      get => openMultipleInputCommand ?? (openMultipleInputCommand = new RelayCommand(() => OpenInputDataView(), () => CanExecute));
    }

    public ICommand ResetSearchCommand
    {
      get => resetSearchCommand ?? (resetSearchCommand = new RelayCommand(() => ResetSearchField(), () => CanExecute));
    }

    #endregion


    #region Public Methods

    public void PreviewDragOver(DragEventArgs e)
    {
      e.Handled = DropFileValidation(e, out string filePath);
      droppedFile = filePath;
    }

    public void PreviewDrop(DragEventArgs e)
    {
      if (droppedFile == null) return;

      using StreamReader streamReader = new StreamReader(droppedFile);
      formatOptionsView.CodeEditor.Text = streamReader.ReadToEnd();
    }

    public void RunFormat()
    {
      if (windowLoaded == false) return;

      var text = formatOptionsView.CodeEditor.Text;
      var formattedText = formatEditorController.FormatText(text, formatStyleOptions, SelectedStyle);
      formatOptionsView.CodeEditorReadOnly.Text = formattedText;
    }

    #endregion


    #region Private Methods

    private void InitializeStyleOptions(FormatOptionsData formatOptionsData)
    {
      formatStyleOptions = formatOptionsData.FormatOptions;
      formatOptionsData.DisableAllOptions();
    }

    private void ChangeControlsDependingOnStyle()
    {
      switch (editorStyle)
      {
        case EditorStyles.Custom:
          SetStyleControls(autoSize, autoSize, FormatOptionsProvider.CustomOptionsData.FormatOptions);
          break;
        case EditorStyles.LLVM:
          SetStyleControls(nameColumnWidthMax, "0", FormatOptionsProvider.LlvmOptionsData.FormatOptions);
          break;
        case EditorStyles.Google:
          SetStyleControls(nameColumnWidthMax, "0", FormatOptionsProvider.GoogleOptionsData.FormatOptions);
          break;
        case EditorStyles.Chromium:
          SetStyleControls(nameColumnWidthMax, "0", FormatOptionsProvider.ChromiumOptionsData.FormatOptions);
          break;
        case EditorStyles.Mozilla:
          SetStyleControls(nameColumnWidthMax, "0", FormatOptionsProvider.MozillaOptionsData.FormatOptions);
          break;
        case EditorStyles.WebKit:
          SetStyleControls(nameColumnWidthMax, "0", FormatOptionsProvider.WebkitOptionsData.FormatOptions);
          break;
        case EditorStyles.Microsoft:
          SetStyleControls(nameColumnWidthMax, "0", FormatOptionsProvider.MicrosoftOptionsData.FormatOptions);
          break;
      }
    }

    private void SetStyleControls(string nameColumnWidth, string enableOptionColumnWidth, List<IFormatOption> options)
    {
      NameColumnWidth = nameColumnWidth;
      EnableOptionColumnWidth = enableOptionColumnWidth;
      FormatOptions = options;
    }

    private void OpenInputDataView()
    {
      if (!(selectedOption is FormatOptionMultipleInputModel multipleInputModel)) return;
      inputMultipleDataView = new InputMultipleDataView(multipleInputModel.MultipleInput);

      inputMultipleDataView.Closed += CloseInputDataView;
      inputMultipleDataView.Show();
    }

    private void CloseInputDataView(object sender, EventArgs e)
    {
      if (selectedOption is FormatOptionMultipleInputModel multipleInputModel
       && inputMultipleDataView.DataContext is InputMultipleDataViewModel inputMultipleDataViewModel)
      {
        multipleInputModel.MultipleInput = inputMultipleDataViewModel.Input;
      }

      inputMultipleDataView.Closed -= CloseInputDataView;
    }

    private void OpenUri(string uri)
    {
      Process.Start(new ProcessStartInfo(uri));
    }

    private void ReadCodeFromFile()
    {
      var filePath = OpenFile(string.Empty, ".cpp", FileExtensionsSelectFile);

      if (File.Exists(filePath))
        formatOptionsView.CodeEditor.Text = File.ReadAllText(filePath);
    }

    private void ResetOptions()
    {
      FormatOptionsProvider.ResetOptions();
      InitializeStyleOptions(FormatOptionsProvider.CustomOptionsData);
      SelectedStyle = EditorStyles.Custom;

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedStyle"));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatOptions"));
    }

    private void CreateFormatFile()
    {
      string fileName = ".clang-format";
      string defaultExt = ".clang-format";
      string filter = "Configuration files (.clang-format)|*.clang-format";

      string path = SaveFile(fileName, defaultExt, filter);
      if (string.IsNullOrEmpty(path) == false)
      {
        WriteContentToFile(path, FormatOptionFile.CreateOutput(formatStyleOptions, SelectedStyle).ToString());
      }
    }

    private void EditorLoaded(object sender, EventArgs e)
    {
      windowLoaded = true;
      formatOptionsView.Loaded -= EditorLoaded;
    }

    private bool DropFileValidation(DragEventArgs e, out string droppedFile)
    {
      droppedFile = null;
      string[] droppedFiles = null;
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
      {
        droppedFiles = e.Data.GetData(DataFormats.FileDrop, true) as string[];
      }

      if (droppedFiles == null || droppedFiles.Length != 1)
        return false;

      if (!ScriptConstants.kAcceptedFileExtensions.Contains(Path.GetExtension(droppedFiles[0])))
        return false;

      droppedFile = droppedFiles[0];
      return true;
    }

    private void ResetSearchField()
    {
      CheckSearch = string.Empty;
    }

    private async Task FindFormatStyleOptionsAsync(string search)
    {
      if (string.IsNullOrWhiteSpace(search)) return;

      await Task.Run(() =>
    {
      searchResultFormatStyleOptions = formatStyleOptions.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatOptions"));
    });
    }

    #endregion
  }
}
