using TaskManager.Api.Data;
using TaskManager.Api.Dtos;
using TaskManager.Api.Entities;

namespace TaskManager.Api.Services;

public class TaskService(AppDbContext dbContext) : ITaskService
{
    public TaskResponse CreateTask(CreateTaskRequest request)
    {
        var now = DateTime.UtcNow;

        var taskItem = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            CreatedAt = now,
            UpdatedAt = now,
            DueDate = request.DueDate
        };

        dbContext.Tasks.Add(taskItem);
        dbContext.SaveChanges();

        return ToResponse(taskItem);
    }

    public TaskResponse GetTaskById(Guid id)
    {
        var taskItem = dbContext.Tasks.Find(id);
        return taskItem == null
            ? throw new KeyNotFoundException($"Task with ID {id} not found.")
            : ToResponse(taskItem);
    }

    public PagedResponse<TaskResponse> GetTasks(int page, int pageSize)
    {
        var query = dbContext.Tasks.AsQueryable();
        var tasks = query
            .OrderBy(t => t.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PagedResponse<TaskResponse>
        {
            Data = [.. tasks.Select(ToResponse)],
            Page = page,
            PageSize = pageSize,
            TotalCount = query.Count()
        };
    }
    
    public TaskResponse UpdateTask(Guid taskId, UpdateTaskRequest request)
    {
        var taskItem = dbContext.Tasks.Find(taskId);
        if (taskItem == null)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found.");
        }
        taskItem.Title = request.Title;
        taskItem.Description = request.Description;
        taskItem.Status = request.Status;
        taskItem.DueDate = request.DueDate;
        taskItem.UpdatedAt = DateTime.UtcNow;
        dbContext.SaveChanges();
        return ToResponse(taskItem);
    }

    public void DeleteTask(Guid id)
    {
        var taskItem = dbContext.Tasks.Find(id);
        if (taskItem == null)
        {
            throw new KeyNotFoundException($"Task with ID {id} not found.");
        }
        dbContext.Tasks.Remove(taskItem);
        dbContext.SaveChanges();
    }

    private static TaskResponse ToResponse(TaskItem taskItem)
    {
        var response = new TaskResponse
        {
            Id = taskItem.Id,
            Title = taskItem.Title,
            Description = taskItem.Description,
            Status = taskItem.Status,
            CreatedAt = taskItem.CreatedAt,
            UpdatedAt = taskItem.UpdatedAt,
            DueDate = taskItem.DueDate
        };
        
        return response;
    }
}