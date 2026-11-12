using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using Microsoft.Win32;
using NexusWorkspace.Commands;
using NexusWorkspace.Helpers;
using NexusWorkspace.Models;
namespace NexusWorkspace.ViewModels;
public class MainViewModel : ObservableObject
{
    private string _searchText = "";
    private string _newTaskTitle = "";
    private string _selectedPriority = "Medium";
    private string _selectedCategory = "Development";
    private string _selectedFilter = "All tasks";
    private TaskItem? _selectedTask;
    private string _statusMessage = "You're all set. Add a task to get started.";
    public ObservableCollection<TaskItem> Tasks { get; } = new();
    public ICollectionView FilteredTasks { get; }
    public string[] Priorities { get; } = ["Low", "Medium", "High"];
    public string[] Categories { get; } = ["Development", "Design", "Meeting", "Personal", "General"];
    public string[] Filters { get; } = ["All tasks", "Active", "Completed"];
    public RelayCommand AddTaskCommand { get; }
    public RelayCommand ToggleTaskCommand { get; }
    public RelayCommand DeleteTaskCommand { get; }
    public RelayCommand ClearCompletedCommand { get; }
    public RelayCommand ExportCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand SetFilterCommand { get; }
    public string SearchText { get => _searchText; set { if (SetProperty(ref _searchText, value)) FilteredTasks.Refresh(); } }
    public string NewTaskTitle { get => _newTaskTitle; set { if (SetProperty(ref _newTaskTitle, value)) AddTaskCommand.RaiseCanExecuteChanged(); } }
    public string SelectedPriority { get => _selectedPriority; set => SetProperty(ref _selectedPriority, value); }
    public string SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }
    public string SelectedFilter { get => _selectedFilter; set { if (SetProperty(ref _selectedFilter, value)) FilteredTasks.Refresh(); } }
    public TaskItem? SelectedTask { get => _selectedTask; set { if (SetProperty(ref _selectedTask, value)) { ToggleTaskCommand.RaiseCanExecuteChanged(); DeleteTaskCommand.RaiseCanExecuteChanged(); } } }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public int TotalCount => Tasks.Count;
    public int CompletedCount => Tasks.Count(t => t.IsCompleted);
    public int ActiveCount => TotalCount - CompletedCount;
    public double CompletionPercent => TotalCount == 0 ? 0 : Math.Round((double)CompletedCount / TotalCount * 100);
    public string CompletionLabel => $"{CompletionPercent:0}% completed";
    public MainViewModel()
    {
        FilteredTasks = CollectionViewSource.GetDefaultView(Tasks);
        FilteredTasks.Filter = FilterTask;
        AddTaskCommand = new RelayCommand(_ => AddTask(), _ => !string.IsNullOrWhiteSpace(NewTaskTitle));
        ToggleTaskCommand = new RelayCommand(_ => ToggleSelected(), _ => SelectedTask != null);
        DeleteTaskCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedTask != null);
        ClearCompletedCommand = new RelayCommand(_ => ClearCompleted(), _ => CompletedCount > 0);
        ExportCommand = new RelayCommand(_ => ExportCsv(), _ => Tasks.Count > 0);
        RefreshCommand = new RelayCommand(_ => Refresh());
        SetFilterCommand = new RelayCommand(p => SelectedFilter = p?.ToString() ?? "All tasks");
        Tasks.CollectionChanged += TasksChanged;
        AddSampleTasks(); Refresh();
    }
    private void AddSampleTasks()
    {
        Tasks.Add(new TaskItem { Title = "Review application architecture", Category = "Development", Priority = "High", CreatedAt = DateTime.Today });
        Tasks.Add(new TaskItem { Title = "Polish dashboard layout", Category = "Design", Priority = "Medium", CreatedAt = DateTime.Today });
        Tasks.Add(new TaskItem { Title = "Prepare sprint discussion", Category = "Meeting", Priority = "Low", IsCompleted = true, CreatedAt = DateTime.Today.AddDays(-1) });
        Tasks.Add(new TaskItem { Title = "Update project documentation", Category = "Development", Priority = "Medium", CreatedAt = DateTime.Today.AddDays(-1) });
    }
    private bool FilterTask(object item)
    {
        if (item is not TaskItem task) return false;
        bool filterMatch = SelectedFilter switch { "Active" => !task.IsCompleted, "Completed" => task.IsCompleted, _ => true };
        return filterMatch && (string.IsNullOrWhiteSpace(SearchText) || task.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || task.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
    }
    private void AddTask()
    {
        var task = new TaskItem { Title = NewTaskTitle.Trim(), Priority = SelectedPriority, Category = SelectedCategory };
        task.PropertyChanged += TaskChanged; Tasks.Insert(0, task); SelectedTask = task; NewTaskTitle = ""; SelectedFilter = "All tasks"; StatusMessage = "Task added successfully."; Refresh();
    }
    private void ToggleSelected() { if (SelectedTask == null) return; SelectedTask.IsCompleted = !SelectedTask.IsCompleted; StatusMessage = SelectedTask.IsCompleted ? "Task marked as completed." : "Task moved back to active."; Refresh(); }
    private void DeleteSelected() { if (SelectedTask == null) return; Tasks.Remove(SelectedTask); SelectedTask = null; StatusMessage = "Task deleted."; Refresh(); }
    private void ClearCompleted()
    {
        var completed = Tasks.Where(t => t.IsCompleted).ToList(); foreach (var task in completed) Tasks.Remove(task);
        SelectedTask = null; StatusMessage = $"Removed {completed.Count} completed task(s)."; Refresh();
    }
    private void ExportCsv()
    {
        var dialog = new SaveFileDialog { Filter = "CSV file (*.csv)|*.csv", FileName = "Nexus-Tasks.csv" };
        if (dialog.ShowDialog() != true) return;
        var lines = new List<string> { "Title,Category,Priority,Status,Created" };
        lines.AddRange(Tasks.Select(t => string.Join(",", Csv(t.Title), Csv(t.Category), Csv(t.Priority), Csv(t.Status), Csv(t.CreatedAt.ToString("yyyy-MM-dd")))));
        File.WriteAllLines(dialog.FileName, lines); StatusMessage = "Tasks exported successfully.";
    }
    private static string Csv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
    private void Refresh() { FilteredTasks.Refresh(); OnPropertyChanged(nameof(TotalCount)); OnPropertyChanged(nameof(CompletedCount)); OnPropertyChanged(nameof(ActiveCount)); OnPropertyChanged(nameof(CompletionPercent)); OnPropertyChanged(nameof(CompletionLabel)); ClearCompletedCommand.RaiseCanExecuteChanged(); ExportCommand.RaiseCanExecuteChanged(); }
    private void TasksChanged(object? sender, NotifyCollectionChangedEventArgs e) { if (e.NewItems != null) foreach (TaskItem task in e.NewItems) task.PropertyChanged += TaskChanged; if (e.OldItems != null) foreach (TaskItem task in e.OldItems) task.PropertyChanged -= TaskChanged; Refresh(); }
    private void TaskChanged(object? sender, PropertyChangedEventArgs e) { if (e.PropertyName == nameof(TaskItem.IsCompleted)) { FilteredTasks.Refresh(); Refresh(); } }
}
