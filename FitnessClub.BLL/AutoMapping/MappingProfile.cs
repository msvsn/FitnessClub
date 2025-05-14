using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.Entities;

namespace FitnessClub.BLL.AutoMapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
            CreateMap<Club, ClubDto>();
            CreateMap<ClubDto, Club>();
            CreateMap<Trainer, TrainerDto>();
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
            CreateMap<ClassScheduleDto, ClassSchedule>()
                .ForMember(dest => dest.BookedPlaces, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Club, opt => opt.Ignore())
                .ForMember(dest => dest.Trainer, opt => opt.Ignore())
                .ForMember(dest => dest.Bookings, opt => opt.Ignore());
            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.GuestName, opt => opt.MapFrom(src => src.GuestName))
                .ForMember(dest => dest.ClassType, opt => opt.MapFrom(src => src.ClassSchedule != null ? src.ClassSchedule.ClassType : null))
                .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.ClassSchedule != null && src.ClassSchedule.Club != null ? src.ClassSchedule.Club.Name : null))
                .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src => src.ClassSchedule != null && src.ClassSchedule.Trainer != null ? $"{src.ClassSchedule.Trainer.FirstName} {src.ClassSchedule.Trainer.LastName}" : null))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.ClassSchedule != null ? src.ClassSchedule.StartTime : default(TimeSpan)))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.ClassSchedule != null ? src.ClassSchedule.EndTime : default(TimeSpan)));
        }
    }
}