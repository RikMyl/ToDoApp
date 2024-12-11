using System.Collections.ObjectModel;

namespace WeatherApp;

public partial class ToDoPage : ContentPage
{
    public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();
    private TaskItem _taskBeingEdited = null; // Keeps track of the task being edited

    public ToDoPage()
    {
        InitializeComponent();
        TaskListView.ItemsSource = Tasks;
    }

    private void OnAddOrSaveTaskClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TaskEntry.Text))
        {
            DisplayAlert("Error", "Task description cannot be empty.", "OK");
            return;
        }

        if (_taskBeingEdited != null)
        {
            // Update the existing task
            _taskBeingEdited.Task = TaskEntry.Text;
            _taskBeingEdited.Date = TaskDatePicker.Date;
            _taskBeingEdited.Time = TaskTimePicker.Time;
            _taskBeingEdited.DisplayTime = $"{TaskDatePicker.Date:dd.MM.yyyy} at {TaskTimePicker.Time:hh\\:mm}";
            _taskBeingEdited = null; // Reset editing state
            AddOrSaveButton.Text = "Add Task"; // Reset button text
        }
        else
        {
            // Add a new task
            Tasks.Add(new TaskItem
            {
                Task = TaskEntry.Text,
                Date = TaskDatePicker.Date,
                Time = TaskTimePicker.Time,
                DisplayTime = $"{TaskDatePicker.Date:dd.MM.yyyy} at {TaskTimePicker.Time:hh\\:mm}"
            });
        }

        // Clear inputs after saving
        TaskEntry.Text = string.Empty;
        TaskDatePicker.Date = DateTime.Today;
        TaskTimePicker.Time = TimeSpan.Zero;
    }

    private void OnEditTaskClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is TaskItem task)
        {
            // Populate the form with existing task details
            TaskEntry.Text = task.Task;
            TaskDatePicker.Date = task.Date;
            TaskTimePicker.Time = task.Time;
            _taskBeingEdited = task; // Set the task being edited
            AddOrSaveButton.Text = "Save Changes"; // Update button text
        }
    }

    private void OnDeleteTaskClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is TaskItem task)
        {
            Tasks.Remove(task);
        }
    }
}

public class TaskItem
{
    public string Task { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public string DisplayTime { get; set; }
}
