using TaskieAPI.Models;

namespace TaskieAPI.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskItem>> GetAllTasksAsync();
    Task<TaskItem?> GetTaskByIdAsync(int id);
    Task<TaskItem> CreateTaskAsync(TaskItem task);
    Task<bool> UpdateTaskAsync(int id, TaskItem task);
    Task<bool> DeleteTaskAsync(int id);
}
