using TaskManager.Api.Dtos;
using TaskManager.Api.Entities;

namespace TaskManager.Api.Services;

public interface ITaskService
{
    TaskResponse CreateTask(CreateTaskRequest request);
    TaskResponse UpdateTask(Guid taskId, UpdateTaskRequest request);
    TaskResponse GetTaskById(Guid id);
    PagedResponse<TaskResponse> GetTasks(int page, int pageSize);
    void DeleteTask(Guid id);
}