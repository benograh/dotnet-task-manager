using System.ComponentModel.DataAnnotations;
using DefaultNamespace;

namespace TaskManager.Api.Dtos;

public class UpdateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
    
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime DueDate { get; set; }
}