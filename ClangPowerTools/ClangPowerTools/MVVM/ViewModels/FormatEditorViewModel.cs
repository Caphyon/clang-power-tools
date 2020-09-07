using ClangPowerTools.Helpers;
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

    private readonly StyleFormatter formatter;
    private readonly FormatEditorView formatEditorView;
    private InputMultipleDataView inputMultipleDataView;
    private ToggleMultipleDataView toggleMultipleDataView;
    private ICommand selctCodeFileCommand;
    private ICommand createFormatFileCommand;
    private ICommand formatCodeCommand;
    private ICommand resetCommand;
    private ICommand openUri;
    private ICommand resetSearchCommand;
    private ICommand detectFormatStyle;

    private string checkSearch = string.Empty;
    private bool showOptionDescription = true;
    private IFormatOption selectedOption;
    private List<IFormatOption> formatStyleOptions;
    private List<IFormatOption> searchResultFormatStyleOptions;
    private EditorStyles selectedStyle = EditorStyles.Custom;
    private bool windowLoaded = false;
    private string nameColumnWidth;
    private string droppedFile;
    private const string nameColumnWidthMax = "340";
    public const string FileExtensionsSelectFile = "Code files (*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hh;*.hpp;*.hxx;)|*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hh;*.hpp;*.hxx";

    #endregion

    #region Constructor

    public FormatEditorViewModel(FormatEditorView formatEditorView)
    {
      formatEditorView.Loaded += EditorLoaded;
      formatEditorView.Closed += EditorClosed;
      this.formatEditorView = formatEditorView;
      formatter = new StyleFormatter();
      InitializeStyleOptions(FormatOptionsProvider.CustomOptionsData);
    }

    //Empty constructor used for XAML IntelliSense
    public FormatEditorViewModel()
    {

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
        FindFormatOptionsAsync(checkSearch).SafeFireAndForget();
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
        FindFormatOptionsAsync(checkSearch).SafeFireAndForget();
        return selectedStyle;
      }
      set
      {
        selectedStyle = value;
        ChangeControlsDependingOnStyle();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedStyle"));

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

    public bool ShowOptionDescription
    {
      get
      {
        return showOptionDescription;
      }
      set
      {
        showOptionDescription = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowOptionDescription"));
      }
    }

    #endregion


    #region Commands

    public ICommand CreateFormatFileCommand
    {
      get => createFormatFileCommand ??= new RelayCommand(() => CreateFormatFile(), () => CanExecute);
    }

    public ICommand FormatCodeCommand
    {
      get => formatCodeCommand ??= new RelayCommand(() => RunFormat(), () => CanExecute);
    }

    public ICommand OpenClangFormatUriCommand
    {
      get => openUri ??= new RelayCommand(() => OpenUri("https://clangpowertools.com/blog/getting-started-with-clang-format-style-options.html"), () => CanExecute);
    }

    public ICommand ResetCommand
    {
      get => resetCommand ??= new RelayCommand(() => ResetOptions(), () => CanExecute);
    }

    public ICommand SelctCodeFileCommand
    {
      get => selctCodeFileCommand ??= new RelayCommand(() => ReadCodeFromFile(), () => CanExecute);
    }

    public ICommand ResetSearchCommand
    {
      get => resetSearchCommand ??= new RelayCommand(() => ResetSearchField(), () => CanExecute);
    }

    public ICommand DetectFormatStyle
    {
      get => detectFormatStyle ??= new RelayCommand(() => OpenMenu(), () => CanExecute);
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
      formatEditorView.CodeEditor.Text = streamReader.ReadToEnd();
    }

    public void RunFormat()
    {
      if (windowLoaded == false) return;

      var text = formatEditorView.CodeEditor.Text;
      var formattedText = formatter.FormatText(text, formatStyleOptions, selectedStyle);
      formatEditorView.CodeEditorReadOnly.Text = formattedText;
    }

    public void OpenMultipleInput(int index)
    {
      if (windowLoaded == false) return;
      var element = FormatOptions[index];

      if (element is FormatOptionMultipleInputModel)
      {
        SelectedOption = element;
        SelectedOption.IsEnabled = true;

        OpenInputDataView();
      }
      else if (element is FormatOptionMultipleToggleModel)
      {
        SelectedOption = element;
        SelectedOption.IsEnabled = true;

        OpenToggleDataView();
      }
    }

    public bool IsAnyOptionEnabled()
    {
      foreach (var item in formatStyleOptions)
      {
        if (item.IsEnabled) return true;
      }
      return false;
    }

    #endregion


    #region Private Methods

    private void InitializeStyleOptions(FormatOptionsData formatOptionsData)
    {
      formatStyleOptions = formatOptionsData.FormatOptions;
      formatOptionsData.DisableAllOptions();
      SelectedOption = FormatOptions.FirstOrDefault();
    }

    private void ChangeControlsDependingOnStyle()
    {
      switch (selectedStyle)
      {
        case EditorStyles.Custom:
          SetStyleControls("260", "80", FormatOptionsProvider.CustomOptionsData.FormatOptions);
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
      SelectedOption = FormatOptions.FirstOrDefault();
    }

    private void OpenInputDataView()
    {
      if (!(selectedOption is FormatOptionMultipleInputModel multipleInputModel)) return;
      inputMultipleDataView = new InputMultipleDataView(multipleInputModel.MultipleInput);

      inputMultipleDataView.Closed += CloseInputDataView;
      inputMultipleDataView.Show();
    }

    private void OpenToggleDataView()
    {
      if (!(selectedOption is FormatOptionMultipleToggleModel multipleToggleModel)) return;
      toggleMultipleDataView = new ToggleMultipleDataView(multipleToggleModel.ToggleFlags);

      toggleMultipleDataView.Closed += CloseInputDataView;
      toggleMultipleDataView.Show();
    }

    private void CloseInputDataView(object sender, EventArgs e)
    {
      if (selectedOption is FormatOptionMultipleInputModel multipleInputModel
       && inputMultipleDataView.DataContext is InputMultipleDataViewModel inputMultipleDataViewModel)
      {
        multipleInputModel.MultipleInput = inputMultipleDataViewModel.Input;
        inputMultipleDataView.Closed -= CloseInputDataView;
      }
      else if (selectedOption is FormatOptionMultipleToggleModel multipleToggleModel
       && toggleMultipleDataView.DataContext is ToggleMultipleDataViewModel toggleMultipleDataViewModel)
      {
        multipleToggleModel.ToggleFlags = toggleMultipleDataViewModel.Input;
        toggleMultipleDataView.Closed -= CloseInputDataView;
      }

      RunFormat();
    }

    private void OpenUri(string uri)
    {
      Process.Start(new ProcessStartInfo(uri));
    }

    private void ReadCodeFromFile()
    {
      var filePath = OpenFile(string.Empty, ".cpp", FileExtensionsSelectFile);

      if (File.Exists(filePath))
        formatEditorView.CodeEditor.Text = File.ReadAllText(filePath);
    }

    private void ResetOptions()
    {
      if (windowLoaded == false) return;
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
        WriteContentToFile(path, FormatOptionFile.CreateOutput(formatStyleOptions, selectedStyle).ToString());
      }
    }

    private void OpenMenu()
    {
      var menuView = new DetectFormatStyleMenuView(this)
      {
        Owner = formatEditorView
      };
      menuView.Show();
    }

    public async Task DetectStyleAsync(List<string> files)
    {
      // TODO refactor entire method
      var detectingView = new DetectingView
      {
        Owner = formatEditorView
      };
      detectingView.Show();
      formatEditorView.IsEnabled = false;

      var diffController = new DiffController(CreateFormatFile);
      detectingView.Closed += diffController.CloseLoadingView;

      var (matchedStyle, matchedOptions) = await diffController.GetFormatOptionsAsync(formatEditorView.CodeEditor.Text, files);

      if (detectingView.IsLoaded == false)
      {
        formatEditorView.IsEnabled = true;
        return;
      }

      SetEditorStyleOptions(matchedStyle, matchedOptions);
      await diffController.ShowDiffAsync();

      detectingView.Closed -= diffController.CloseLoadingView;
      detectingView.Close();
      formatEditorView.IsEnabled = true;
    }

    private void SetEditorStyleOptions(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)
    {
      selectedStyle = matchedStyle;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedStyle"));
      SetStyleControls(nameColumnWidthMax, "0", matchedOptions);
      RunFormat();
    }

    private void EditorLoaded(object sender, EventArgs e)
    {
      windowLoaded = true;
      formatEditorView.Loaded -= EditorLoaded;
    }

    private void EditorClosed(object sender, EventArgs e)
    {
      var settingsPathBuilder = new SettingsPathBuilder();
      string folderPath = Path.Combine(settingsPathBuilder.GetPath(""), "Format");
      FileSystem.DeleteDirectory(folderPath);
      formatEditorView.Closed -= EditorClosed;
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

      if (ScriptConstants.kAcceptedFileExtensions.Contains(Path.GetExtension(droppedFiles[0])) == false)
        return false;

      droppedFile = droppedFiles[0];
      return true;
    }

    private void ResetSearchField()
    {
      CheckSearch = string.Empty;
      ShowOptionDescription = true;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatOptions"));
    }

    private async Task FindFormatOptionsAsync(string search)
    {
      await Task.Run(() =>
    {
      if (string.IsNullOrWhiteSpace(checkSearch)) return;

      searchResultFormatStyleOptions = formatStyleOptions.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
      SelectedOption = searchResultFormatStyleOptions.FirstOrDefault();
      ShowOptionDescription = searchResultFormatStyleOptions.Count != 0;

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatOptions"));
    });
    }

    #endregion
  }
}
