using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.DAL.Entities;

namespace FitnessClub.BLL
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
            CreateMap<MembershipType, MembershipTypeDto>();
            CreateMap<MembershipTypeDto, MembershipType>();
            CreateMap<Membership, MembershipDto>()
                .ForMember(dest => dest.MembershipTypeName, opt => opt.MapFrom(src => src.MembershipType.Name))
                .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.Club != null ? src.Club.Name : null));
            CreateMap<ClassSchedule, ClassScheduleDto>()
                .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.Club.Name))
                .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src => $"{src.Trainer.FirstName} {src.Trainer.LastName}"));
            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.ClassType, opt => opt.MapFrom(src => src.ClassSchedule.ClassType))
                .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.ClassSchedule.Club.Name))
                .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src => $"{src.ClassSchedule.Trainer.FirstName} {src.ClassSchedule.Trainer.LastName}"));
        }
    }
}