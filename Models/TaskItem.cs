using NexusWorkspace.Helpers;
namespace NexusWorkspace.Models;
public class TaskItem : ObservableObject
{
    private string _title = "";
    private string _category = "General";
    private string _priority = "Medium";
    private bool _isCompleted;
    private DateTime _createdAt = DateTime.Now;
    public string Title { get => _title; set => SetProperty(ref _title, value); }
    public string Category { get => _category; set => SetProperty(ref _category, value); }
    public string Priority { get => _priority; set => SetProperty(ref _priority, value); }
    public bool IsCompleted { get => _isCompleted; set { if (SetProperty(ref _isCompleted, value)) OnPropertyChanged(nameof(Status)); } }
    public DateTime CreatedAt { get => _createdAt; set => SetProperty(ref _createdAt, value); }
    public string Status => IsCompleted ? "Completed" : "In progress";
}
