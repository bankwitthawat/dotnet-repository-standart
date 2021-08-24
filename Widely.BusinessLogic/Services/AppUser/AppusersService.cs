﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.BusinessLogic.Services.Base;
using Widely.DataAccess.DataContext.Entities;
using Widely.DataAccess.Repositories.Appusers;
using Widely.DataAccess.Repositories.UnitOfWork;
using Widely.DataModel.ViewModels.Appusers.ListView;
using Widely.DataModel.ViewModels.Common;

namespace Widely.BusinessLogic.Services.AppUser
{
    public class AppusersService : BaseService
    {
        private readonly IMapper _mapper;
        private readonly IAppusersRepository _appusersRepository;

        public AppusersService(
             IHttpContextAccessor httpContextAccessor
             , IUnitOfWork unitOfWork
             , IMapper mapper
             , IAppusersRepository appusersRepository
            ) : base(httpContextAccessor, unitOfWork)
        {
            _mapper = mapper;
            _appusersRepository = appusersRepository;
        }

        public async Task<GridResult<AppUserListViewResponse>> GetList(SearchCriteria<AppUserListViewRequest> filter)
        {
            //init DbSet
            var userRepository = _unitOfWork.AsyncRepository<Appusers>();

            //var filterData = await userRepository.All();
            var filterData = await _appusersRepository.GetUserAllRelated();

            if (!string.IsNullOrEmpty(filter?.criteria?.username))
            {
                filterData = filterData.Where(x => x.Username.Contains(filter.criteria.username)).ToList();
            }

            if (!string.IsNullOrEmpty(filter?.criteria?.fullName))
            {
                filterData = filterData.Where(x => ($"{x.Title} {x.Fname} {x.Lname}").Contains(filter.criteria.fullName.Trim())).ToList();
            }


            var TotalRecord = filterData == null ? 0 : filterData.Count();
            filter.gridCriteria = filter.gridCriteria == null ? new GridCriteria { page = 1, pageSize = 10 } : filter.gridCriteria;
            var TotalPage = (TotalRecord + filter.gridCriteria.pageSize - 1) / filter.gridCriteria.pageSize;
            if (filter.gridCriteria != null && filter.gridCriteria.Take > 0)
            {
                filter.gridCriteria.totalRecord = TotalRecord;
                filter.gridCriteria.totalPage = (TotalRecord + filter.gridCriteria.pageSize - 1) / filter.gridCriteria.pageSize;
                if (!string.IsNullOrEmpty(filter.gridCriteria.sortby) && (!string.IsNullOrEmpty(filter.gridCriteria.sortdir)))
                {
                    if (filter.gridCriteria.sortdir == "desc")
                        filterData = filterData.OrderByDescending(filter.gridCriteria.sortby).ToList();
                    else
                        filterData = filterData.OrderBy(filter.gridCriteria.sortby).ToList();
                }
                else
                {
                    filterData = filterData.OrderBy(x => x.Id).ToList(); //dufault initial load
                }
                filterData = filterData.Skip(filter.gridCriteria.skip).Take(filter.gridCriteria.Take).ToList();
            }


            var dtoResult = _mapper.Map<List<AppUserListViewResponse>>(filterData);

            return new GridResult<AppUserListViewResponse>()
            {
                Items = dtoResult,
                pagination = new Pagination
                {
                    page = filter.gridCriteria.page,
                    pageSize = filter.gridCriteria.pageSize,
                    totalPage = TotalPage,
                    totalRecord = TotalRecord,
                    sortby = filter.gridCriteria.sortby,
                    sortdir = filter.gridCriteria.sortdir,
                }
            };

        }
    }
}
