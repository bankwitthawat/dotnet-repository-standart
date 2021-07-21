using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Widely.DataAccess.DataContext.Entities;
using Widely.DataModel.ViewModels.Auth.LogIn;
using Widely.DataModel.ViewModels.Auth.Token;

namespace Widely.API.Infrastructure.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Authtokens, RefreshToken>();
            //.ForMember(dest => dest.IsExpired, src => src.MapFrom(s => s.ExpiresOn >= DateTime.UtcNow));
            CreateMap<RefreshToken, Authtokens>();
            CreateMap<Appusers, LogInResponse>()
                .ForMember(dest => dest.UserId, src => src.MapFrom(s => s.Id));
        }
    }
}
