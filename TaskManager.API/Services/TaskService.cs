using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.API.DTOs.Tasks;
using TaskManager.API.Models;
using TaskManager.API.Repositories.Interfaces;
using TaskManager.API.Services.Interfaces;

namespace TaskManager.API.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<PagedResult<TaskResponseDto>> GetTasksAsync(Guid userId, TaskFilterDto filter)
        {
            var pagedTasks = await _taskRepository.GetTasksAsync(userId, filter);
            var responseItems = pagedTasks.Items.Select(MapToResponseDto);

            return new PagedResult<TaskResponseDto>
            {
                Items = responseItems,
                TotalCount = pagedTasks.TotalCount,
                Page = pagedTasks.Page,
                PageSize = pagedTasks.PageSize
            };
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(Guid userId, Guid taskId)
        {
            var task = await _taskRepository.GetByIdAndUserIdAsync(taskId, userId);
            return task == null ? null : MapToResponseDto(task);
        }

        public async Task<TaskResponseDto> CreateTaskAsync(Guid userId, CreateTaskDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                Status = TaskStatus.Todo,
                DueDate = dto.DueDate?.ToUniversalTime(),
                UserId = userId
            };

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();

            return MapToResponseDto(task);
        }

        public async Task<TaskResponseDto?> UpdateTaskAsync(Guid userId, Guid taskId, UpdateTaskDto dto)
        {
            var task = await _taskRepository.GetByIdAndUserIdAsync(taskId, userId);
            if (task == null) return null;

            if (dto.Title != null) task.Title = dto.Title;
            if (dto.Description != null) task.Description = dto.Description;
            if (dto.Priority.HasValue) task.Priority = dto.Priority.Value;
            if (dto.Status.HasValue) task.Status = dto.Status.Value;
            
            // Allow setting DueDate to null or a new value
            task.DueDate = dto.DueDate?.ToUniversalTime();

            _taskRepository.Update(task);
            await _taskRepository.SaveChangesAsync();

            return MapToResponseDto(task);
        }

        public async Task<bool> UpdateStatusAsync(Guid userId, Guid taskId, TaskStatus status)
        {
            var task = await _taskRepository.GetByIdAndUserIdAsync(taskId, userId);
            if (task == null) return false;

            task.Status = status;
            _taskRepository.Update(task);
            return await _taskRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteTaskAsync(Guid userId, Guid taskId)
        {
            var task = await _taskRepository.GetByIdAndUserIdAsync(taskId, userId);
            if (task == null) return false;

            _taskRepository.Delete(task);
            return await _taskRepository.SaveChangesAsync();
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId)
        {
            var totalTasks = await _taskRepository.GetCountAsync(userId);
            var todoCount = await _taskRepository.GetCountAsync(userId, status: TaskStatus.Todo);
            var inProgressCount = await _taskRepository.GetCountAsync(userId, status: TaskStatus.InProgress);
            var doneCount = await _taskRepository.GetCountAsync(userId, status: TaskStatus.Done);
            var overdueCount = await _taskRepository.GetCountAsync(userId, isOverdue: true);
            var highPriorityCount = await _taskRepository.GetCountAsync(userId, priority: Priority.High);

            double completionRate = totalTasks > 0 
                ? Math.Round(((double)doneCount / totalTasks) * 100, 2) 
                : 0.0;

            var upcomingTasks = await _taskRepository.GetUpcomingTasksAsync(userId, 5);
            var upcomingTasksDtos = upcomingTasks.Select(MapToResponseDto);

            return new DashboardStatsDto
            {
                TotalTasks = totalTasks,
                TodoCount = todoCount,
                InProgressCount = inProgressCount,
                DoneCount = doneCount,
                OverdueCount = overdueCount,
                HighPriorityCount = highPriorityCount,
                CompletionRate = completionRate,
                UpcomingTasks = upcomingTasksDtos
            };
        }

        private TaskResponseDto MapToResponseDto(TaskItem task)
        {
            var utcNow = DateTime.UtcNow;
            bool isOverdue = task.DueDate.HasValue && task.DueDate.Value < utcNow && task.Status != TaskStatus.Done;

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority.ToString(),
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                IsOverdue = isOverdue,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }
}
