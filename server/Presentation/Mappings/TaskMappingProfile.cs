using Application.DTOs;
using AutoMapper;
using Core.Domain.Entities;
using Infrastructure.Data.Entities;
using Presentation.DTOs;

namespace Presentation.Mappings
{
    public class TaskMappingProfile : Profile
    {
        public TaskMappingProfile()
        {
            CreateMap<CreateTaskRequest, CreateTaskDto>();
            CreateMap<CreateTaskDto, TaskItem>();
            CreateMap<TaskItem, TaskEntity>();
            CreateMap<TaskItem, TaskDto>();
        }
    }
}
