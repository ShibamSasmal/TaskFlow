using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.API.Models;
using TaskManager.API.DTOs.Tasks;

namespace TaskManager.API.Repositories.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem?> GetByIdAsync(Guid id);
        Task<TaskItem?> GetByIdAndUserIdAsync(Guid id, Guid userId);
        Task<PagedResult<TaskItem>> GetTasksAsync(Guid userId, TaskFilterDto filter);
        Task<IEnumerable<TaskItem>> GetUpcomingTasksAsync(Guid userId, int count);
        Task<int> GetCountAsync(Guid userId, TaskStatus? status = null, Priority? priority = null, bool? isOverdue = null);
        Task AddAsync(TaskItem task);
        void Update(TaskItem task);
        void Delete(TaskItem task);
        Task<bool> SaveChangesAsync();
    }
}
