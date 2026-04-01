using AutoMapper;
using SkipTheLine.DTOs;
using SkipTheLine.Models;
using SkipTheLine.Enums;

namespace SkipTheLine
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Restaurant, RestaurantDto>();
            CreateMap<CreateRestaurantDto, Restaurant>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateRestaurantDto, Restaurant>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<Reservation, ReservationDto>()
                .ForMember(dest => dest.RestaurantName,
                    opt => opt.MapFrom(src => src.Restaurant.Name))
                .ForMember(dest => dest.RestaurantAddress,
                    opt => opt.MapFrom(src => src.Restaurant.Address))
                .ForMember(dest => dest.RestaurantCity,
                    opt => opt.MapFrom(src => src.Restaurant.City))
                .ForMember(dest => dest.RestaurantPhone,
                    opt => opt.MapFrom(src => src.Restaurant.PhoneNumber))
                .ForMember(dest => dest.TableNumber,
                    opt => opt.MapFrom(src => src.Table.TableNumber));

            CreateMap<CreateReservationDto, Reservation>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ReservationStatus.Pending));

            CreateMap<User, UserDto>();
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateProfileDto, User>();
        }
    }
}