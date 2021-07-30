using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.BusinessLogic.Services.Base;
using Widely.BusinessLogic.Utilities;
using Widely.DataAccess.DataContext.Entities;
using Widely.DataAccess.Repositories.Auth;
using Widely.DataAccess.Repositories.UnitOfWork;
using Widely.DataModel.ViewModels.Auth.LogIn;
using Widely.DataModel.ViewModels.Auth.Register;
using Widely.DataModel.ViewModels.Auth.Token;
using Widely.DataModel.ViewModels.Common;

namespace Widely.BusinessLogic.Services.Auth
{
    public class AuthService : BaseService
    {
        private readonly IMapper _mapper;
        private readonly JwtManager _jwtManager;
        public readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        public List<AppModule> treeNode = new List<AppModule>();

        public AuthService(
            IHttpContextAccessor httpContextAccessor
            , IUnitOfWork unitOfWork
            , IAuthRepository authRepository
            , JwtManager jwtManager
            , IMapper mapper
            , IConfiguration configuration
            ) : base(httpContextAccessor, unitOfWork)
        {
            var testHttpContext = httpContextAccessor;

            _configuration = configuration;
            _mapper = mapper;
            _jwtManager = jwtManager;
            _authRepository = authRepository;

        }

        public async Task<ServiceResponse<LogInResponse>> Login(LogInRequest request)
        {
            ServiceResponse<LogInResponse> response = new ServiceResponse<LogInResponse>();
            var userRepository = _unitOfWork.AsyncRepository<Appusers>();
            var authTokenRepository = _unitOfWork.AsyncRepository<Authtokens>();

            //var user = await userRepository.GetAsync(_ => _.Username.ToLower() == request.Username.ToLower());
            var user = await _authRepository.GetUserRelated(request.Username);

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
            }
            else if (user.IsActive == false)
            {
                response.Success = false;
                response.Message = "This account has been suspended, please contract adminstrator.";
            }
            else if (user.LoginAttemptCount >= 5)
            {
                response.Success = false;
                response.Message = "This account has been locked, please contract adminstrator.";
            }
            else if (!PasswordHashUtility.VerifyPasswordHash(request.Username, request.Password, user.PasswordHash, user.PasswordSalt))
            {
                response.Success = false;
                response.Message = "Wrong password.";

                user.LoginAttemptCount++;
                await userRepository.UpdateAsync(user);
            }
            else
            {

                //update user
                user.LastLogin = DateTime.Now;
                user.LoginAttemptCount = 0;
                await userRepository.UpdateAsync(user);


                response.Data = await this.GetLoginUserInfo(user);
                response.Success = true;
                response.Message = "Login success. !!";
            }

            //save all changes to db
            await _unitOfWork.CommitAsync();



            return response;
        }

        private async Task<LogInResponse> GetLoginUserInfo(Appusers user)
        {
            Int32.TryParse(_configuration.GetSection("JwtSetting:ExpireMins").Value, out int tokenExpire);
            var authTokenRepository = _unitOfWork.AsyncRepository<Authtokens>();

            //remove old refresh token
            var oldRefreshToken = await authTokenRepository.ListAsync(_ => _.UserId == user.Id);
            await authTokenRepository.RemoveRangeAsync(oldRefreshToken);

            //save new refresh token
            var refreshToken = _mapper.Map<Authtokens>(_jwtManager.GenerateRefreshToken(GetIpAddress(), GetUserAgnet(), GetMachineName()));
            refreshToken.UserId = user.Id;
            await authTokenRepository.AddAsync(refreshToken);

            var userdto = _mapper.Map<LogInResponse>(user);
            userdto.AppModule = await GetModuleTreeList(null, userdto.RoleId.Value);
            userdto.RefreshToken = refreshToken.Token;
            userdto.TokenExpire = DateTime.Now.AddMinutes(tokenExpire);
            userdto.TokenTimeoutMins = ((int)userdto.TokenExpire.Subtract(DateTime.Now).TotalMinutes) - 1;
            userdto.Token = _jwtManager.CreateToken(userdto);
            return userdto;
        }


        public async Task<List<AppModule>> GetModuleTreeList(AppModule appModule, int roleId)
        {
            var rootNode = await _authRepository.GetModulePermissionByRole(roleId);

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
                item.Children = await GetModuleTreeList(item, roleId);
            }

            return mList;
        }

        #region recursive old code

        //public async Task<List<AppModule>> GetTreeList(int roleId)
        //{
        //    var appmoduleRepo = _unitOfWork.AsyncRepository<Appmodule>();
        //    var approleRepo = _unitOfWork.AsyncRepository<Approles>();
        //    var appermissionRepo = _unitOfWork.AsyncRepository<Apppermission>();


        //    //var rootNode = await appmoduleRepo.ListAsync(_ => _.IsActive == true && _.ParentId == null);
        //    var rootNode = await _authRepository.GetModulePermissionByRole(roleId);

        //    List<AppModule> mList = (from r in rootNode
        //                             where r.ParentID == null
        //                             select new AppModule()
        //                             {
        //                                 ID = r.ID,
        //                                 Title = r.Title,
        //                                 Subtitle = r.Subtitle,
        //                                 Type = r.Type,
        //                                 Icon = r.Icon,
        //                                 Path = r.Path,
        //                                 Sequence = r.Sequence,
        //                                 ParentID = r.ParentID,

        //                                 IsAccess = r.IsAccess,
        //                                 IsCreate = r.IsCreate,
        //                                 IsView = r.IsView,
        //                                 IsEdit = r.IsEdit,
        //                                 IsDelete = r.IsDelete,
        //                                 IsActive = r.IsActive,

        //                             }).ToList();

        //    foreach (var item in mList)
        //    {
        //        //Recursive
        //        item.Children = await GetNodesList(item, roleId);
        //    }

        //    return mList;
        //}

        //public async Task<List<AppModule>> GetNodesList(AppModule appModule, int roleId)
        //{
        //    var appmoduleRepo = _unitOfWork.AsyncRepository<Appmodule>();

        //    //var node = await appmoduleRepo.ListAsync(_ => _.IsActive == true && _.ParentId == appModule.ID);
        //    var node = await _authRepository.GetModulePermissionByRole(roleId);

        //    List<AppModule> mList = (from q in node
        //                             where q.ParentID == appModule.ID
        //                             select new AppModule()
        //                             {
        //                                 ID = q.ID,
        //                                 Title = q.Title,
        //                                 Subtitle = q.Subtitle,
        //                                 Type = q.Type,
        //                                 Icon = q.Icon,
        //                                 Path = q.Path,
        //                                 Sequence = q.Sequence,
        //                                 ParentID = q.ParentID,

        //                                 IsAccess = q.IsAccess,
        //                                 IsCreate = q.IsCreate,
        //                                 IsView = q.IsView,
        //                                 IsEdit = q.IsEdit,
        //                                 IsDelete = q.IsDelete,
        //                                 IsActive = q.IsActive,

        //                             }).ToList();

        //    foreach (var item in mList)
        //    {
        //        //Recursive
        //        item.Children = await GetNodesList(item, roleId);
        //    }

        //    return mList.OrderBy(x => x.Sequence).ToList();
        //}


        //private async void ModuleTree(AppModule parentNode)
        //{
        //    var appmoduleRepo = _unitOfWork.AsyncRepository<Appmodule>();
        //    var allModule = await appmoduleRepo.ListAsync(_ => _.IsActive == true);

        //    var rootNode = allModule.Where(x => parentNode == null ? x.ParentId == null : x.ParentId == parentNode.ID).ToList();

        //    foreach (var item in rootNode)
        //    {
        //        AppModule newNode = new AppModule();
        //        newNode.ID = item.Id;
        //        newNode.Title = item.Title;
        //        newNode.Subtitle = item.Subtitle;
        //        newNode.Type = item.Type;
        //        newNode.Icon = item.Icon;
        //        newNode.Path = item.Path;
        //        newNode.Sequence = item.Sequence;



        //        if (parentNode == null)
        //        {
        //            treeNode.Add(newNode);
        //        }
        //        else
        //        {
        //            parentNode.Children.Add(newNode);
        //        }

        //        this.ModuleTree(newNode);

        //    }
        //}
        #endregion

        public async Task<ServiceResponse<LogInResponse>> RefreshToken(string oldToken)
        {
            ServiceResponse<LogInResponse> response = new ServiceResponse<LogInResponse>();
            var tokenAge = Convert.ToInt32(_configuration.GetSection("JwtSetting:ExpireMins").Value);
            var tokenExpire = DateTime.Now.AddMinutes(tokenAge);

            var username = GetUserID();
            if (string.IsNullOrEmpty(username))
                return response;

            var userRepository = _unitOfWork.AsyncRepository<Appusers>();
            var authTokenRepository = _unitOfWork.AsyncRepository<Authtokens>();

            var user = await _authRepository.GetUserRelatedByToken(oldToken);
            if (user == null)
                return response;

            var refreshToken = user.Authtokens.Single(x => x.Token == oldToken);
            if (refreshToken == null)
                return response;

            var newRefreshToken = _mapper.Map<Authtokens>(_jwtManager.GenerateRefreshToken(GetIpAddress(), GetUserAgnet(), GetMachineName()));
            user.Authtokens.Add(newRefreshToken);
            await userRepository.UpdateAsync(user);
            await authTokenRepository.RemoveAsync(refreshToken);


            response.Data = await this.GetLoginUserInfo(user);
            response.Data.RefreshToken = newRefreshToken.Token;
            response.Data.TokenExpire = tokenExpire;
            response.Data.TokenTimeoutMins = ((int)tokenExpire.Subtract(DateTime.Now).TotalMinutes) - 1;

            response.Success = true;
            response.Message = "Success.";

            await _unitOfWork.CommitAsync();

            return response;
        }

        public async Task<ServiceResponse<bool>> Register(RegisterRequest registerRequest)
        {
            ServiceResponse<bool> response = new ServiceResponse<bool>();

            try
            {
                var userRepository = _unitOfWork.AsyncRepository<Appusers>();

                var user = await userRepository.GetAsync(_ => _.Username.ToLower() == registerRequest.username.ToLower());

                if (user != null)
                {
                    response.Data = false;
                    response.Success = false;
                    response.Message = "มีชื่อผู้ใช้งานนี้แล้วในระบบ";
                }

                PasswordHashUtility.CreatePasswordHash(registerRequest.username, registerRequest.password, out byte[] passwordHash, out byte[] passwordSalt);

                var newUser = new Appusers()
                {
                    Username = registerRequest.username,
                    PasswordSalt = passwordSalt,
                    PasswordHash = passwordHash,
                    Fname = registerRequest.fName,
                    Lname = registerRequest.lName,
                    CreatedDate = DateTime.Now,
                    CreatedBy = "System",
                    IsActive = true
                };

                await userRepository.AddAsync(newUser);
                await _unitOfWork.CommitAsync();

                response.Data = true;
                response.Success = true;
                response.Message = "ลงทะเบียนสำเร็จ !!";
            }
            catch (Exception)
            {
                throw;
            }

            return response;
        }
    }
}
