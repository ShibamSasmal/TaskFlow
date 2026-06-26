using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs.Tasks;
using TaskManager.API.Models;
using TaskManager.API.Services.Interfaces;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private Guid CurrentUserId
        {
            get
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                {
                    throw new UnauthorizedAccessException("User identification is missing or invalid.");
                }
                return userId;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] TaskFilterDto filter)
        {
            var result = await _taskService.GetTasksAsync(CurrentUserId, filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(Guid id)
        {
            var task = await _taskService.GetTaskByIdAsync(CurrentUserId, id);
            return task is null ? NotFound(new { message = "Task not found." }) : Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            var task = await _taskService.CreateTaskAsync(CurrentUserId, dto);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskDto dto)
        {
            var task = await _taskService.UpdateTaskAsync(CurrentUserId, id, dto);
            return task is null ? NotFound(new { message = "Task not found or unauthorized." }) : Ok(task);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] TaskStatus status)
        {
            var success = await _taskService.UpdateStatusAsync(CurrentUserId, id, status);
            return success ? NoContent() : NotFound(new { message = "Task not found or unauthorized." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var success = await _taskService.DeleteTaskAsync(CurrentUserId, id);
            return success ? NoContent() : NotFound(new { message = "Task not found or unauthorized." });
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var stats = await _taskService.GetDashboardStatsAsync(CurrentUserId);
            return Ok(stats);
        }
    }
}
