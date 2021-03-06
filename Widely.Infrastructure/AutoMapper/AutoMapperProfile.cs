using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.DataContext.Entities;
using Widely.DataModel.ViewModels.Approles.ListView;
using Widely.DataModel.ViewModels.Appusers.ItemView;
using Widely.DataModel.ViewModels.Appusers.ListView;
using Widely.DataModel.ViewModels.Auth.LogIn;
using Widely.DataModel.ViewModels.Auth.Token;

namespace Widely.Infrastructure.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Authtokens, RefreshToken>();
            //.ForMember(dest => dest.IsExpired, src => src.MapFrom(s => s.ExpiresOn >= DateTime.UtcNow));
            CreateMap<RefreshToken, Authtokens>();
            CreateMap<Appusers, LogInResponse>()
                .ForMember(dest => dest.UserId, src => src.MapFrom(s => s.Id))
                .ForMember(dest => dest.RoleId, src => src.MapFrom(s => s.Role == null ? (int?)null : s.Role.Id))
                .ForMember(dest => dest.RoleName, src => src.MapFrom(s => s.Role == null ? string.Empty : s.Role.Name))
                .ForMember(dest => dest.RoleDescription, src => src.MapFrom(s => s.Role == null ? string.Empty : s.Role.Description))
                ;

            // App role
            CreateMap<Approles, AppRoleResponse>()
                .ForMember(dest => dest.CreatedDate, src => src.MapFrom(s => s.CreatedDate == null ? string.Empty : s.CreatedDate.Value.ToString("dd/MM/yyyy")))
                .ForMember(dest => dest.ModifiedDate, src => src.MapFrom(s => s.ModifiedDate == null ? string.Empty : s.ModifiedDate.Value.ToString("dd/MM/yyyy")))
                ;

            // App user
            CreateMap<Appusers, AppUserListViewResponse>()
                .ForMember(dest => dest.RoleId, src => src.MapFrom(s => s.Role == null ? (int?)null : s.Role.Id))
                .ForMember(dest => dest.RoleName, src => src.MapFrom(s => s.Role == null ? string.Empty : s.Role.Name))
                .ForMember(dest => dest.RoleDescription, src => src.MapFrom(s => s.Role == null ? string.Empty : s.Role.Description))
                .ForMember(dest => dest.FullName, src => src.MapFrom(s => $"{s.Title}{s.Fname} {s.Lname}"))
                ;

            CreateMap<Appusers, AppUserItemViewResponse>()
                .ForMember(dest => dest.RoleId, src => src.MapFrom(s => s.Role == null ? (int?)null : s.Role.Id))
                .ForMember(dest => dest.RoleName, src => src.MapFrom(s => s.Role == null ? string.Empty : s.Role.Name))
                .ForMember(dest => dest.RoleDescription, src => src.MapFrom(s => s.Role == null ? string.Empty : s.Role.Description))
                //.ForMember(dest => dest.BirthDate, src => src.MapFrom(s => s.BirthDate == null ? string.Empty : s.BirthDate.Value.ToString("dd/MM/yyyy")))
                ;

        }
    }
}
