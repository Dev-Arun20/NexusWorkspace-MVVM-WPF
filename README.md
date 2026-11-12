# NEXUS Workspace — WPF MVVM Demo

A polished task-management desktop application built with C#, .NET 8 and WPF using a simple MVVM architecture. No third-party NuGet packages are required.

## Requirements
- Windows 10/11
- Visual Studio 2022 with the **.NET desktop development** workload
- .NET 8 SDK

## Run
1. Extract the ZIP.
2. Open `NexusWorkspace.csproj` in Visual Studio 2022.
3. Restore/build the project (there are no external package dependencies).
4. Press **F5**.

Alternatively, from a Windows terminal in this folder:

```powershell
dotnet restore
dotnet build
dotnet run
```

## Features
- Dashboard counters and completion percentage
- Add task with category and priority
- Select and complete/reopen a task
- Delete selected task
- Search tasks by title or category
- Filter All / Active / Completed
- Clear all completed tasks
- Export task list to CSV using a Save File dialog
- Refresh dashboard

## MVVM layout
- `Models/TaskItem.cs` — task data and observable properties
- `ViewModels/MainViewModel.cs` — screen state, filtering and application commands
- `Views/MainWindow.xaml` — WPF presentation and bindings
- `Views/MainWindow.xaml.cs` — view initialization and ViewModel assignment only
- `Commands/RelayCommand.cs` — ICommand implementation
- `Helpers/ObservableObject.cs` — INotifyPropertyChanged base class

The sample tasks are held in memory; restarting the application resets the sample data. Persistence can be added later with SQLite or JSON.
