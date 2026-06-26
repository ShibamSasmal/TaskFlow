using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs.Tasks;
using TaskManager.API.Models;
using TaskManager.API.Repositories.Interfaces;

namespace TaskManager.API.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskItem?> GetByIdAsync(Guid id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<TaskItem?> GetByIdAndUserIdAsync(Guid id, Guid userId)
        {
            return await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }

        public async Task<PagedResult<TaskItem>> GetTasksAsync(Guid userId, TaskFilterDto filter)
        {
            var query = _context.Tasks.Where(t => t.UserId == userId);

            // Filtering
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchLower = filter.Search.ToLower();
                query = query.Where(t => t.Title.ToLower().Contains(searchLower) ||
                                         (t.Description != null && t.Description.ToLower().Contains(searchLower)));
            }

            if (filter.Priority.HasValue)
            {
                query = query.Where(t => t.Priority == filter.Priority.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(t => t.Status == filter.Status.Value);
            }

            if (filter.DueBefore.HasValue)
            {
                query = query.Where(t => t.DueDate <= filter.DueBefore.Value);
            }

            if (filter.DueAfter.HasValue)
            {
                query = query.Where(t => t.DueDate >= filter.DueAfter.Value);
            }

            // Sorting
            query = filter.SortBy.ToLower() switch
            {
                "duedate" => filter.SortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
                "priority" => filter.SortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
                "title" => filter.SortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                _ => filter.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<TaskItem>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<IEnumerable<TaskItem>> GetUpcomingTasksAsync(Guid userId, int count)
        {
            var utcNow = DateTime.UtcNow;
            return await _context.Tasks
                .Where(t => t.UserId == userId && t.Status != TaskStatus.Done)
                .OrderBy(t => t.DueDate.HasValue ? 0 : 1) // Tasks with due date first
                .ThenBy(t => t.DueDate)
                .ThenByDescending(t => t.Priority)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(Guid userId, TaskStatus? status = null, Priority? priority = null, bool? isOverdue = null)
        {
            var query = _context.Tasks.Where(t => t.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            if (isOverdue.HasValue)
            {
                var utcNow = DateTime.UtcNow;
                if (isOverdue.Value)
                {
                    query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value < utcNow && t.Status != TaskStatus.Done);
                }
                else
                {
                    query = query.Where(t => !t.DueDate.HasValue || t.DueDate.Value >= utcNow || t.Status == TaskStatus.Done);
                }
            }

            return await query.CountAsync();
        }

        public async Task AddAsync(TaskItem task)
        {
            await _context.Tasks.AddAsync(task);
        }

        public void Update(TaskItem task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            _context.Tasks.Update(task);
        }

        public void Delete(TaskItem task)
        {
            _context.Tasks.Remove(task);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
