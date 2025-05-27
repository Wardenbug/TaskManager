using Application.DTOs.TaskDtos;
using Application.DTOs.User;
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
            CreateMap<TaskEntity, TaskItem>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.Parse(src.UserId)));
            CreateMap<TaskItem, TaskDto>();
            CreateMap<UpdateTaskRequest, UpdateTaskDto>();
            CreateMap<UpdateTaskDto, TaskItem>();
            CreateMap<RegisterUserDto, User>();
            CreateMap<User, UserDto>();
            CreateMap<User, ApplicationUser>();
            CreateMap<ApplicationUser, User>();
            CreateMap<RegisterDto, RegisterUserDto>();
            CreateMap<LoginRequestDto, LoginDto>();
        }
    }
}
