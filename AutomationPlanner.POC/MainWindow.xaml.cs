using System.Windows;
using System.Windows.Controls;
using AutomationPlanner.POC.Models.Settings;
using AutomationPlanner.POC.ViewModels;

namespace AutomationPlanner.POC;

public partial class MainWindow : System.Windows.Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private MainViewModel ViewModel => (MainViewModel)DataContext;

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is null) DataContext = ((App)System.Windows.Application.Current).MainViewModel;
        ViewModel.SetAskUserResponder(ShowAskUserDialogAsync);
    }

    private Task<string> ShowAskUserDialogAsync(string question, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Dispatcher.CheckAccess())
        {
            return Dispatcher.InvokeAsync(() => ShowAskUserDialogAsync(question, cancellationToken)).Task.Unwrap();
        }

        var answerBox = new TextBox
        {
            Text = ViewModel.AskUserDefaultResponse,
            MinWidth = 420,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MinHeight = 80
        };
        var useAnswerButton = new Button { Content = "Use Answer", IsDefault = true, MinWidth = 96, Margin = new Thickness(0, 0, 8, 0) };
        var useDefaultButton = new Button { Content = "Use Default", IsCancel = true, MinWidth = 96 };
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 0, 0),
            Children = { useAnswerButton, useDefaultButton }
        }.Dock(Dock.Bottom);
        var content = new DockPanel
        {
            LastChildFill = true,
            Margin = new Thickness(16),
            Children =
            {
                new TextBlock
                {
                    Text = "The planner is asking for clarification:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 8)
                }.Dock(Dock.Top),
                new TextBlock
                {
                    Text = question,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 12)
                }.Dock(Dock.Top),
                new TextBlock
                {
                    Text = "Answer",
                    Margin = new Thickness(0, 0, 0, 4)
                }.Dock(Dock.Top),
                buttonPanel,
                answerBox
            }
        };
        var dialog = new Window
        {
            Title = "ask_user",
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.CanResize,
            MinWidth = 520,
            MinHeight = 260,
            Content = content
        };
        useAnswerButton.Click += (_, _) => dialog.DialogResult = true;
        useDefaultButton.Click += (_, _) => dialog.DialogResult = false;

        var result = dialog.ShowDialog();
        var answer = result == true ? answerBox.Text : ViewModel.AskUserDefaultResponse;
        ViewModel.AskUserDefaultResponse = answer;
        return Task.FromResult(answer);
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog { Description = "Select planner package folder" };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) ViewModel.SelectedPlannerRoot = dialog.SelectedPath;
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SettingsViewModel.Settings = CloneSettings(ViewModel.Settings);
        var window = new SettingsWindow { Owner = this, DataContext = ViewModel.SettingsViewModel };
        if (window.ShowDialog() == true)
        {
            ViewModel.Settings = ViewModel.SettingsViewModel.Settings;
            if (!string.IsNullOrWhiteSpace(ViewModel.Settings.PlannerPackagePath))
            {
                ViewModel.SelectedPlannerRoot = ViewModel.Settings.PlannerPackagePath;
            }
        }
    }

    private static AppSettings CloneSettings(AppSettings settings) => new()
    {
        OpenAiApiKey = settings.OpenAiApiKey,
        Model = settings.Model,
        Temperature = settings.Temperature,
        MaxTokens = settings.MaxTokens,
        RequestTimeoutSeconds = settings.RequestTimeoutSeconds,
        PlannerPackagePath = settings.PlannerPackagePath,
        MockDataBasePath = settings.MockDataBasePath,
        LastPlannerPackagePath = settings.LastPlannerPackagePath
    };
}

internal static class WpfDialogExtensions
{
    public static T Dock<T>(this T element, Dock dock) where T : UIElement
    {
        DockPanel.SetDock(element, dock);
        return element;
    }
}
