using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.BusinessLogic.Services.Base;
using Widely.BusinessLogic.Utilities;
using Widely.DataAccess.DataContext.Entities;
using Widely.DataAccess.Repositories.Appusers;
using Widely.DataAccess.Repositories.UnitOfWork;
using Widely.DataModel.ViewModels.Appusers.ListView;
using Widely.DataModel.ViewModels.Common;
using Widely.Infrastructure.Exceptions;

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

            if (filter?.criteria?.roleId != null)
            {
                filterData = filterData.Where(x => x.RoleId == filter.criteria.roleId.Value).ToList();
            }

            if (filter?.criteria?.isActive != null)
            {
                filterData = filterData.Where(x => x.IsActive == filter.criteria.isActive.Value).ToList();
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
                    filterData = filterData.OrderByDescending(x => x.Id).ToList(); //dufault initial load
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
    
        public async Task<ServiceResponse<List<OptionItems>>> GetRoleList()
        {
            // init DbSet
            var roleRepo = _unitOfWork.AsyncRepository<Approles>();

            // init response
            ServiceResponse<List<OptionItems>> response = new ServiceResponse<List<OptionItems>>();

            var roleList = await roleRepo.All();
            var result = (from q in roleList
                          select new OptionItems 
                          { 
                            id = q.Id,
                            value = $"{q.Id}",
                            label = $"{q.Name}"
                          }).ToList();

            response.Data = result;
            response.Success = true;
            response.Message = "OK";

            return response;
        }

        public async Task<ServiceResponse<bool>> Create(AppUserCreateRequest request)
        {
            var transactionDate = DateTime.Now;
            ServiceResponse<bool> response = new ServiceResponse<bool>();

            //init DbSet
            var userRepository = _unitOfWork.AsyncRepository<Appusers>();

            var isDuplicate = await userRepository.GetAsync(x => x.Username.ToLower() == request.username.ToLower());
            if (isDuplicate != null)
            {
                throw new AppException("This username is duplicate.");
            }

            PasswordHashUtility.CreatePasswordHash(request.username, request.password, out byte[] passwordHash, out byte[] passwordSalt);


            var newUser = new Appusers()
            {
                Username = request.username.Trim(),
                RoleId = request.roleId,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Fname = !string.IsNullOrEmpty(request.fName) ? request.fName.Trim() : null,
                Lname = !string.IsNullOrEmpty(request.lName) ? request.lName.Trim() : null,
                MobilePhone = !string.IsNullOrEmpty(request.mobilePhone) ? request.mobilePhone.Trim() : null,
                IsActive = request.isActive,
                IsForceChangePwd = request.forceChangePassword,

                CreatedBy = GetUserName(),
                CreatedDate = transactionDate
            };

            await userRepository.AddAsync(newUser);
            await _unitOfWork.CommitAsync();

            response.Data = true;
            response.Success = true;
            response.Message = "Create Successfully. !!";

            return response;
        }
    }
}
