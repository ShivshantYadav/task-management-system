using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.Extensions;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TaskDto>>> GetTasks([FromQuery] TaskFilterDto filter)
        {
            var tasks = await _taskService.GetTasksAsync(filter, User.GetUserId(), User.GetRole());
            return Ok(tasks);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            var task = await _taskService.GetByIdAsync(id, User.GetUserId(), User.GetRole());
            return Ok(task);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto dto)
        {
            var task = await _taskService.CreateTaskAsync(dto, User.GetUserId(), User.GetRole());
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<TaskDto>> UpdateTask(int id, UpdateTaskDto dto)
        {
            var task = await _taskService.UpdateTaskAsync(id, dto, User.GetUserId(), User.GetRole());
            return Ok(task);
        }

        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<TaskDto>> UpdateStatus(int id, UpdateTaskStatusDto dto)
        {
            var task = await _taskService.UpdateStatusAsync(id, dto, User.GetUserId(), User.GetRole());
            return Ok(task);
        }
    }
}
