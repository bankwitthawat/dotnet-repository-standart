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
            else if (user.LoginAttemptCount > 5)
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
                response.Message = "Successfully. !!";
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
            userdto.AppModule = await GetRootModules(userdto.RoleId.Value);
            userdto.RefreshToken = refreshToken.Token;
            userdto.TokenExpire = DateTime.Now.AddMinutes(tokenExpire);
            userdto.TokenTimeoutMins = ((int)userdto.TokenExpire.Subtract(DateTime.Now).TotalMinutes) - 1;
            userdto.Token = _jwtManager.CreateToken(userdto);
            return userdto;
        }

        public async Task<List<AppModule>> GetRootModules(int roleId)
        {
            //init DbSet
            var moduleRepo = _unitOfWork.AsyncRepository<Appmodule>();
            var permissionRepo = _unitOfWork.AsyncRepository<Apppermission>();

            //query
            //#1 ค้นหาทุก module และ permission ที่ Role นี้มีสิทธิ์
            var modules = await _authRepository.GetModulePermissionByRole(roleId);

            //#2 ค้นหาที่ตาราง permission ที่ role นี้มีสิทธิ์
            //** ตาราง permission จะเก็บ module ที่เป้น type = "basic" เท่านั้น
            var modulePermission = await permissionRepo.ListAsync(x => x.RoleId == roleId);

            //#3 ancentor is all parent related
            var ancentor = new List<int>();

            //#4 นำค่าที่ได้จากตาราง permission มาหา parent (จากล่างขึ้นบนสุดตาม tree structure)
            foreach (var item in modulePermission)
            {
                List<int> ancestors = await _authRepository.FindAncestorById(item.ModuleId);

                //#5 เมื่อเจอทุก parent และตัวเอง จะถูกเก็บในตัวแปร ancentor ไว้ก่อน
                ancentor.AddRange(ancestors);
            }

            //#6 หาก children อยู่ใน tier เดียวกัน parent จะต้องซ้ำกันเลยต้องกรอง parent ที่ซ้ำออก
            ancentor = ancentor.Distinct().ToList();

            //#7 ใช้ตาราง Appmodules ตั้งเพื่อหาทุก module โดยจะกรองจะตัวแปร ancentor
            //**ทีนี้เราจะรู้แล้วว่า parent of children ตามสิทธิ์มีอะไรบ้าง
            var rootNode = modules.Where(x => ancentor.Any(r => r == x.ID)).ToList();

            //#8 ใช้ recursion มาทำงานเพื่อสร้าง tree structure และส่งออกไปเพื่อ render menu บนหน้าจอ
            //** ให้ front end ใช้ function recursive หาตาม tree นี้เองว่า menu ไหนมีสิทธิ์อะไรบ้างเช่น create/edit/view/delete
            var result = await this.GetModuleTreeList(null, rootNode);
            return result;

        }

        public async Task<List<AppModule>> GetModuleTreeList(AppModule appModule, List<AppModule> rootNode)
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
                item.Children = await GetModuleTreeList(item, rootNode);
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

            var username = GetUserID();
            if (string.IsNullOrEmpty(username))
                return response; 

            var user = await _authRepository.GetUserRelatedByToken(oldToken);
            if (user == null) 
                return response;

            response.Data = await this.GetLoginUserInfo(user);
            response.Success = true;
            response.Message = "Successfully. !!";
           
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
   
        public async Task<ServiceResponse<bool>> Logout(string oldtoken)
        {
            ServiceResponse<bool> response = new ServiceResponse<bool>() { Success = false, Data = false };
            var authTokenRepository = _unitOfWork.AsyncRepository<Authtokens>();

            var token = await authTokenRepository.GetAsync(_ => _.Token == oldtoken);
            if (token == null)
            {
                return response;
            }

            await authTokenRepository.RemoveAsync(token);
            await _unitOfWork.CommitAsync();

            response.Data = true;
            response.Success = true;
            response.Message = "Successfully. !!";

            return response;
        }

    }
}
