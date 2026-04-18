using System.ComponentModel.DataAnnotations;
using IGMS.Domain.Entities;
using TaskStatus   = IGMS.Domain.Entities.TaskStatus;
using TaskPriority = IGMS.Domain.Entities.TaskPriority;

namespace IGMS.Application.Common.Models;

public class TaskListDto
{
    public int          Id               { get; set; }
    public string       TitleAr          { get; set; } = string.Empty;
    public string       TitleEn          { get; set; } = string.Empty;
    public TaskStatus   Status           { get; set; }
    public TaskPriority Priority         { get; set; }
    public DateTime?    DueDate          { get; set; }
    public string?      AssignedToNameAr { get; set; }
    public string?      DepartmentNameAr { get; set; }
    public DateTime     CreatedAt        { get; set; }
    public int?         RiskId           { get; set; }
    public string?      RiskTitleAr      { get; set; }
}

public class TaskDetailDto : TaskListDto
{
    public string? DescriptionAr { get; set; }
    public int?    AssignedToId  { get; set; }
    public int?    DepartmentId  { get; set; }
}

public class SaveTaskRequest
{
    public int Id { get; set; }
    [Required] public string TitleAr { get; set; } = string.Empty;
    public string TitleEn            { get; set; } = string.Empty;
    public string?      DescriptionAr { get; set; }
    public TaskStatus   Status        { get; set; }
    public TaskPriority Priority      { get; set; } = TaskPriority.Medium;
    public DateTime?    DueDate       { get; set; }
    public int?         AssignedToId  { get; set; }
    public int?         DepartmentId  { get; set; }
    public int?         RiskId        { get; set; }
}

public class TaskQuery
{
    public int           Page     { get; set; } = 1;
    public int           PageSize { get; set; } = 20;
    public string?       Search   { get; set; }
    public TaskStatus?   Status   { get; set; }
    public TaskPriority? Priority { get; set; }
    public int?          AssignedToId { get; set; }
    public int?          RiskId       { get; set; }
}
