using AspNetCore.Reporting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Widely.BusinessLogic.Services;
using Widely.BusinessLogic.Services.Auth;
using Widely.BusinessLogic.Services.Base;
using Widely.DataModel.ViewModels.Approles.ItemView;
using Widely.DataModel.ViewModels.Approles.ListView;
using Widely.DataModel.ViewModels.Common;
using Widely.Infrastructure.Security;

namespace Widely.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DemoReportController : ControllerBase
    {
        private readonly BaseService _baseService;
        private readonly ApprolesService _approlesService;
        private readonly Logger _logger;
        private readonly IWebHostEnvironment _env;
        public DemoReportController(ApprolesService approlesService , IWebHostEnvironment webHostEnvironment)
        {
            this._approlesService = approlesService;
            this._env = webHostEnvironment;
            //_logger = NLog.LogManager.GetCurrentClassLogger();
        }
       

        [ModulePermission("*", "*")]
        [HttpPost("getreport")]
        public async Task<IActionResult> GenerateReportAsync(SearchCriteria<AppRoleRequest> request)
        {
            

            var roles = await this._approlesService.GetList(request);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("param1", "Application Roles List");
            parameters.Add("param2", "Widely Next Company");


            string mimetype = "";
            int extension = 1;

            var rdlcpath = $"{this._env.WebRootPath}\\Reports\\DemoReport.rdlc";
            
            LocalReport localreport = new LocalReport(rdlcpath);
            localreport.AddDataSource("DataSet1", roles.Items);
            var report = localreport.Execute(RenderType.Pdf, extension, parameters, mimetype);
            var reportBytes = report.MainStream;

            var result = Convert.ToBase64String(reportBytes, 0, reportBytes.Length);

            //return Ok(File(result.MainStream, "application/pdf"));
            return Ok(result);

        }

        [ModulePermission("ROLES", "*")]
        [HttpPost("getdatasource")]
        public async Task<IActionResult> GetRoleList()
        {
            SearchCriteria<AppRoleRequest> request = new SearchCriteria<AppRoleRequest>();
            var result = await this._approlesService.GetList(request);
            return Ok(result);
        }
    }

    
}
