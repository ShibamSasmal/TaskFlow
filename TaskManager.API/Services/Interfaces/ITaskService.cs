using System;
using System.Threading.Tasks;
using TaskManager.API.DTOs.Tasks;
using TaskManager.API.Models;

namespace TaskManager.API.Services.Interfaces
{
    public interface ITaskService
    {
        Task<PagedResult<TaskResponseDto>> GetTasksAsync(Guid userId, TaskFilterDto filter);
        Task<TaskResponseDto?> GetTaskByIdAsync(Guid userId, Guid taskId);
        Task<TaskResponseDto> CreateTaskAsync(Guid userId, CreateTaskDto dto);
        Task<TaskResponseDto?> UpdateTaskAsync(Guid userId, Guid taskId, UpdateTaskDto dto);
        Task<bool> UpdateStatusAsync(Guid userId, Guid taskId, TaskStatus status);
        Task<bool> DeleteTaskAsync(Guid userId, Guid taskId);
        Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId);
    }
}
