using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Dtos;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskController(ITaskService taskService) : ControllerBase
{
    
    [HttpPost]
    public TaskResponse CreateTask(CreateTaskRequest request)
    {
        return taskService.CreateTask(request);
    }
    
    [HttpGet("{id:guid}")]
    public TaskResponse GetTaskById(Guid id)
    {
        return taskService.GetTaskById(id);
    }
    
    [HttpGet]
    public PagedResponse<TaskResponse> GetTasks(int page, int pageSize)
    {
        return taskService.GetTasks(page, pageSize);
    }
    
    [HttpPut("{taskId:guid}")]
    public TaskResponse UpdateTask(Guid taskId, UpdateTaskRequest request)
    {
        return taskService.UpdateTask(taskId, request);
    }
    
    [HttpDelete("{id:guid}")]
    public void DeleteTask(Guid id)
    {
        taskService.DeleteTask(id);
    }
}