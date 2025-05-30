using TaskieAPI.Models;
using TaskieAPI.Repositories;

namespace TaskieAPI.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<TaskItem?> GetTaskByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        return await _repository.CreateAsync(task);
    }

    public async Task<bool> UpdateTaskAsync(int id, TaskItem updatedTask)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        existing.Title = updatedTask.Title;
        existing.Description = updatedTask.Description;
        existing.IsComplete = updatedTask.IsComplete;

        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        var task = await _repository.GetByIdAsync(id);
        if (task == null) return false;

        await _repository.DeleteAsync(task);
        return true;
    }
}
