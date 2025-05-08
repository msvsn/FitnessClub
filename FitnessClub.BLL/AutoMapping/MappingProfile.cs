using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.DAL.Entities;

namespace FitnessClub.BLL.AutoMapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
            CreateMap<Club, ClubDto>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Address));
            CreateMap<ClubDto, Club>();
            CreateMap<Trainer, TrainerDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
            CreateMap<TrainerDto, Trainer>();
            CreateMap<MembershipType, MembershipTypeDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.DurationDays, opt => opt.MapFrom(src => src.DurationDays))
                .ForMember(dest => dest.IsNetwork, opt => opt.MapFrom(src => src.IsNetwork))
                .ForMember(dest => dest.IsSingleVisit, opt => opt.MapFrom(src => src.IsSingleVisit));
            CreateMap<MembershipTypeDto, MembershipType>();
            CreateMap<Membership, MembershipDto>()
                .ForMember(dest => dest.MembershipTypeName, opt => opt.MapFrom(src => src.MembershipType.Name))
                .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.Club != null ? src.Club.Name : null))
                .ForMember(dest => dest.IsSingleVisit, opt => opt.MapFrom(src => src.MembershipType != null && src.MembershipType.IsSingleVisit))
                .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.IsUsed));
            CreateMap<ClassSchedule, ClassScheduleDto>()
                .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.Club.Name))
                .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src => $"{src.Trainer.FirstName} {src.Trainer.LastName}"));
            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.ClassType, opt => opt.MapFrom(src => src.ClassSchedule.ClassType))
                .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.ClassSchedule.Club.Name))
                .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src => $"{src.ClassSchedule.Trainer.FirstName} {src.ClassSchedule.Trainer.LastName}"))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.ClassSchedule.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.ClassSchedule.EndTime));
        }
    }
}