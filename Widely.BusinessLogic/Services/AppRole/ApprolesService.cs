using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.BusinessLogic.Services.Base;
using Widely.DataAccess.DataContext.Entities;
using Widely.DataAccess.Repositories.Approles;
using Widely.DataAccess.Repositories.UnitOfWork;
using Widely.DataModel.ViewModels.Approles.ItemView;
using Widely.DataModel.ViewModels.Approles.ListView;
using Widely.DataModel.ViewModels.Auth.LogIn;
using Widely.DataModel.ViewModels.Common;
using Widely.Infrastructure.Exceptions;

namespace Widely.BusinessLogic.Services.AppRole
{
    public class ApprolesService : BaseService
    {
        private readonly IMapper _mapper;
        private readonly IApprolesRepository _approlesRepository;
        public ApprolesService(
            IHttpContextAccessor httpContextAccessor
            , IUnitOfWork unitOfWork
            , IMapper mapper
            , IApprolesRepository approlesRepository
            ) : base(httpContextAccessor, unitOfWork)
        {
            _mapper = mapper;
            _approlesRepository = approlesRepository;
        }

        public async Task<GridResult<AppRoleResponse>> GetList(SearchCriteria<AppRoleRequest> filter)
        {
            var roleRepository = _unitOfWork.AsyncRepository<Approles>();
            var filterData = await roleRepository.All();

            //TODO
            //#1 filterData by search criteria
            //#2 set pagination

            //#1
            if (!string.IsNullOrEmpty(filter?.criteria?.name))
            {
                filterData = filterData.Where(x => x.Name.Contains(filter.criteria.name)).ToList();
            }

            if (!string.IsNullOrEmpty(filter?.criteria?.description))
            {
                filterData = filterData.Where(x => x.Description.Contains(filter.criteria.description)).ToList();
            }

            //#2
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

            var dtoResult = _mapper.Map<List<AppRoleResponse>>(filterData);

            return new GridResult<AppRoleResponse>()
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

        public async Task<ServiceResponse<AppRoleItemViewResponse>> GetModuleList(string roleId)
        {
            ServiceResponse<AppRoleItemViewResponse> response = new ServiceResponse<AppRoleItemViewResponse>();

            try
            {
                int.TryParse(roleId, out int id);
                var rootNode = new AppRoleItemViewResponse();

                if (id > 0)
                {
                    rootNode = await _approlesRepository.GetModulePermissionByRole(id);
                }
                else
                {
                    rootNode = await _approlesRepository.GetModulePermission();
                }

                response.Data = new AppRoleItemViewResponse();
                response.Data.id = rootNode.id;
                response.Data.name = rootNode.name;
                response.Data.description = rootNode.description;
                response.Data.moduleList = await this.GetModuleTreeList(null, roleId, rootNode.moduleList);
               
                response.Success = true;
                response.Message = "Ok";
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return response;
        }

        private async Task<List<AppModule>> GetModuleTreeList(AppModule appModule, string roleId, List<AppModule> rootNode)
        {

            List<AppModule> mList = (from r in rootNode
                                     where appModule == null ? r.ParentID == null : r.ParentID == appModule.ID
                                     select new AppModule()
                                     {
                                         ID = r.ID,
                                         Title = r.Title,
                                         Subtitle = r.Subtitle,
                                         Type = r.Type,
                                         Icon = r.Icon,
                                         Path = r.Path,
                                         Sequence = r.Sequence,
                                         ParentID = r.ParentID,

                                         IsAccess = r.IsAccess,
                                         IsCreate = r.IsCreate,
                                         IsView = r.IsView,
                                         IsEdit = r.IsEdit,
                                         IsDelete = r.IsDelete,
                                         IsActive = r.IsActive,

                                     }).ToList();

            foreach (var item in mList)
            {
                item.Children = await GetModuleTreeList(item, roleId, rootNode);
            }

            return mList;
        }

        public async Task<ServiceResponse<bool>> Create(AppRoleCreateRequest request)
        {
            var transactionDate = DateTime.Now;
            ServiceResponse<bool> response = new ServiceResponse<bool>();

            var roleRepository = _unitOfWork.AsyncRepository<Approles>();
            var permissionRepository = _unitOfWork.AsyncRepository<Apppermission>();

            //จะเก็บเฉพาะ type ที่เป็น basic เท่านั้น
            request.moduleList = request.moduleList.Where(x => x.type == "basic" && x.isAccess).ToList();

            var newRole = new Approles()
            {
                Name = request.name,
                Description = request.description,
                CreatedBy = GetUserName(),
                CreatedDate = transactionDate,
            };

            newRole.Apppermission = (from q in request.moduleList
                                     select new Apppermission
                                     {
                                         RoleId = newRole.Id,
                                         ModuleId = q.id,
                                         IsAccess = q.isAccess,
                                         IsCreate = q.isCreate,
                                         IsEdit = q.isEdit,
                                         IsView = q.isView,
                                         IsDelete = q.isDelete,
                                         ModifiedBy = GetUserName(),
                                         ModifiedDate = transactionDate
                                     }).ToList();

            await roleRepository.AddAsync(newRole);
            await _unitOfWork.CommitAsync();

            response.Data = true;
            response.Success = true;
            response.Message = "Successfully";

            return response;
        }

        public async Task<ServiceResponse<bool>> Update(AppRoleUpdateRequest request)
        {
            var transactionDate = DateTime.Now;
            ServiceResponse<bool> response = new ServiceResponse<bool>();

            var roleRepository = _unitOfWork.AsyncRepository<Approles>();
            var permissionRepository = _unitOfWork.AsyncRepository<Apppermission>();

            //จะเก็บเฉพาะ type ที่เป็น basic เท่านั้น
            request.moduleList = request.moduleList.Where(x => x.type == "basic" && x.isAccess).ToList();


            var role = await roleRepository.GetAsync(x => x.Id == request.id);
            var permission = await permissionRepository.ListAsync(x => x.RoleId == request.id);

            if (role == null)
            {
                throw new AppException("This role not found.");
            }

            await permissionRepository.RemoveRangeAsync(permission);

            role.Name = request.name;
            role.Description = request.description;
            role.CreatedBy = GetUserName();
            role.CreatedDate = transactionDate;
            role.Apppermission = (from q in request.moduleList
                                  select new Apppermission
                                  {
                                      RoleId = role.Id,
                                      ModuleId = q.id,
                                      IsAccess = q.isAccess,
                                      IsCreate = q.isCreate,
                                      IsEdit = q.isEdit,
                                      IsView = q.isView,
                                      IsDelete = q.isDelete,
                                      ModifiedBy = GetUserName(),
                                      ModifiedDate = transactionDate
                                  }).ToList();


            await roleRepository.UpdateAsync(role);
            await _unitOfWork.CommitAsync();

            response.Data = true;
            response.Success = true;
            response.Message = "Successfully";

            return response;
        }

        public async Task<ServiceResponse<bool>> Delete(AppRoleDeleteRequest request)
        {
            //init dbSet
            var roleRepository = _unitOfWork.AsyncRepository<Approles>();
            var userRepository = _unitOfWork.AsyncRepository<Appusers>();

            ServiceResponse<bool> response = new ServiceResponse<bool>();

            var role = await roleRepository.GetAsync(x => x.Id == request.id);
            var user = await userRepository.ListAsync(x => x.RoleId == request.id);

            if (role == null)
            {
                throw new AppException("This role not found.");
            }
            else if (user.Count > 0)
            {
                throw new AppException("Sorry, The role is currently turned on.");
            }
            else
            {
                //delete with casecade table apppermission
                await roleRepository.RemoveAsync(role);
                await _unitOfWork.CommitAsync();
            }

            response.Data = true;
            response.Success = true;
            response.Message = "Delete successfully.";
            return response;
        }
    }
}
