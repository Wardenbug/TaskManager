﻿using Application.DTOs;
using Application.Services;
using AutoMapper;
using Core.Domain.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Presentation.DTOs;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;
        private readonly IMapper _mapper;

        public TasksController(TaskService taskService, IMapper mapper)
        {
            _taskService = taskService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
        {
            await _taskService.AddTaskAsync(_mapper.Map<CreateTaskDto>(request));
            return Ok("Created");
        }
    }
}
