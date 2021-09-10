# WidelyNext Standart - Backend API
This is backend API standart for development.

## Table of contents
- [Prerequisites](#prerequisites)
- [Solution Architecture](#solution-architecture)
- [Project Structure](#project-structure)
- [Database Configuration](#database-configuration)
- [Security](#security)
    - [Jason Web Token](#json-web-token)
    - [Authorize Attributes](#authorize-attributes)
- [Dependencies Injection](#dependencies-injection)
- [Repositories](#repositories)
  - [GenericRepository Class (Common)](#generic-repository)
  - [Repository Class (By Module)](#repository-class)
- [Services](#services)
  - [BaseService Class (Common)](#baseservice-class-common)
  - [Service Class (By Module)](#service-class-by-module)
- [Swagger](#swagger)
- [Logging](#logging)
- [Error Handling](#error-handling)
- [AutoMapper](#automapper)


# Contents
## Prerequisites
- .Net 5.x [*Download*](https://dotnet.microsoft.com/download/dotnet/5.0)
- Visual Studio 2019 (v16.11 >=) [*Download*](https://visualstudio.microsoft.com/downloads/)
- MySQL 8.0
<br /><br />

## Solution Architecture
> ใช้สถาปัตยกรรมแบบ **N-Tier/N-Layer Architecture** เพื่อแยกการแสดงผล(Response) การประมวลผล(Processing) และการจัดการข้อมูล(Data Management) ออกจากกัน

<details>
<summary>Show instructions</summary>
<p align="center">
  <img src="https://www.img.in.th/images/7ccc84f7f505d7d0b9553ff8da747454.md.png" alt="7ccc84f7f505d7d0b9553ff8da747454.png" border="0" />
</p>
</details>
<br /><br />

## Project Structure
<details>
<summary>Widely.API</summary>

- Controllers
  > การตั้งชื่อไฟล์ให้ระบุชื่อของ Module นั้นๆแล้วตามด้วย Controller เช่น ExampleController.cs
- Extensions
  > ใช้เก็บไฟล์ที่เกี่ยวกับการตั้งค่าในระดับ Middlware
</details>

<details>
<summary>Widely.BusinessLogic</summary>

- Services  
  >การตั้งชื่อไฟล์ให้ระบุชื่อของ Module นั้นๆแล้วตามด้วย Service เช่น ExampleService.cs
- Utillities
  > ใช้เก็บไฟล์คลาสส่วนกลางเพื่อให้ส่วนอื่นๆสามารถเรียกใช้ได้ง่าย
</details>

<details>
<summary>Widely.DataAccess</summary>

- DataContext  
  > ใช้เก็บไฟล์ Context และ Model ที่ใช้สำหรับ mapping กับ database (for ef core)
- Repositories
  > ใช้เก็บ module repository เช่น interface, class
</details>

<details>
<summary>Widely.DataModel</summary>

> ใช้เก็บไฟล์ viewmodel เพื่อการรับส่งข้อมูลจาก frontend
</details>

<details>
<summary>Widely.Infrastructure</summary>

> ใช้เก็บไฟล์ config สำหรับThird party library
</details>
<br /><br />

## Database Configuration
- Setting connectionstring in `Widely.API` > `appsettings.json`
- Go to destination  `Widely.API` > `Extensions` > `ServiceCollectionExtensions.cs`
  ```C#
  public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
  {
      return services.AddDbContext<WidelystandartContext>(options =>
      {
          var connetionString = configuration.GetConnectionString("DefaultConnection");
          options.UseMySql(connetionString, ServerVersion.AutoDetect(connetionString));
      });
  }
  ```
  > This is example for MySQL. For more examples, please refer to the [*Documentation*](https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- Go to `Startup.cs`
  ```C#
  //add application repositories
  services
     .AddHttpContext()
     .AddDatabase(Configuration) // <-- Add this.
     .AddUnitOfWork()
     .AddRepositories()
     .AddBusinessServices()
     .AddAutoMapper()
     ;
  ```
<br /><br />

## Security
### Json Web Token
Attributes: `[Authorize]`\
Example: 
```C#
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //<-- Add this
public class ExampleController : ControllerBase 
{

}
```
> สามารถวางในระดับ Action ได้เช่นเดียวกัน

<br /><br />

### Authorize Attributes
Attributes: `[ModulePermission]`  
Parameter: ("string", "string")
- Parameter 1: **Module**
  | input |  Description  | Remark |
  |:-----:|:--------------|--------|
  |*| Accept all module |  |
  |ModuleName| Accept some module | ModuleName อ้างอิงจาก Database Table: 'appmodules', column: Title |

- Parameter 2: **Permission**
  | input |  Description  | Remark |
  |:-----:|:--------------|--------|
  |*|Accept all permission |   |
  | CREATE |to see **Add new** button in the list| |
  | EDIT |to see **Edit** button in the table for every entry| |
  | VIEW |to see **View** button in the table for every entry| |
  | DELETE |to see **Delete** button in the table for every entry| |


Example 1: 
> สามารถเรียกใช้งาน method นี้ได้โดยที่คำขอนี้ไม่จำเป็นต้องมีสิทธิ์
```C#
[ModulePermission("*", "*")] //<--- Add here
[HttpPost("list")]
public async Task<IActionResult> GetList(Model request)
{
    var result = await this._appusersService.GetList(request);
    return Ok(result);
}
```

Example 2: 
> จะเรียกใช้ method นี้ได้คำขอต้องมีสิทธิ์เข้าถึงโมดูลที่ชื่อว่า **USERS** และเข้าถึงได้ทุก permission
```C#
[ModulePermission("USERS", "*")] //<--- Add here
[HttpPost("list")]
public async Task<IActionResult> Create(Model request)
{
    var result = await this._appusersService.Create(request);
    return Ok(result);
}
```

Example 3: 
> จะเรียกใช้ method นี้ได้คำขอต้องมีสิทธิ์เข้าถึงโมดูลที่ชื่อว่า **USERS**, Permission = CREATE
```C#
[ModulePermission("USERS", "CREATE")] //<--- Add here
[HttpPost("list")]
public async Task<IActionResult> Create(Model request)
{
    var result = await this._appusersService.Create(request);
    return Ok(result);
}
```

Example 4: 
> จะเรียกใช้ method นี้ได้คำขอต้องมีสิทธิ์เข้าถึงโมดูลที่ชื่อว่า **USERS** หรือ **ROLES**, Permission = CREATE
```C#
[ModulePermission("USERS,ROLES", "*")] //<--- Add here
[HttpPost("list")]
public async Task<IActionResult> GetList(Model request)
{
    var result = await this._appusersService.GetList(request);
    return Ok(result);
}
```
> Permission สามารถเลือกใส่ได้อย่างเดียวเท่านั้น เช่น *, CREATE, VIEW, EDIT, DELETE

<br /><br />

## Dependencies Injection
ในการตั้งค่าทุกอย่างที่เกี่ยวกับ DI สามารถทำได้ที่ไฟล์ `ServiceCollectionExtensions.cs` และลงทะเบียนที่ไฟล์ `Startup.cs`

```C#
//Startup.cs
 services
    .AddHttpContext() // registration for httpcontext
    .AddDatabase(Configuration) // registration for database
    .AddUnitOfWork() // registration for unit of work
    .AddRepositories() // registration for repository
    .AddBusinessServices() // registration for service
    .AddAutoMapper() // registration for automapper file
    ;
```

Example: Add new a service file
```C#
//ServiceCollectionExtensions.cs
public static IServiceCollection AddBusinessServices(this IServiceCollection services)
{
    return services
        .AddScoped<JwtManager>()
        .AddScoped<BaseService>()
        .AddScoped<AuthService>()
        .AddScoped<ApprolesService>()
        .AddScoped<AppusersService>()
        .AddScoped<UserProfileService>()
        .AddScoped<ExampleService>() //<----- add new service here.
        ;
}
```

Example: Add new a repository file
```C#
//ServiceCollectionExtensions.cs
public static IServiceCollection AddRepositories(this IServiceCollection services)
{
    return services
        .AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
        .AddScoped<IAuthRepository, AuthRepository>()
        .AddScoped<IAppusersRepository, AppusersRepository>()
        .AddScoped<IApprolesRepository, ApprolesRepository>()
        .AddScoped<IUserProfileRepository, UserProfileRepository>()
        .AddScoped<IExampleRepository, ExampleRepository>() //<----- add new repository here.
        ;
}
```
<br /><br />

## Repositories
The Repository Design Pattern in C# Mediates between the domain and the data mapping layers using a collection-like interface for accessing the domain objects.  
<br />`A generic repository is often used with the entity framework to speed up the process of creating a data layer. In most cases this is a generalization too far and it can be a trap for lazy developers.` [*Ben Morris*](https://www.ben-morris.com/)

### Generic Repository
A Generic Repository Pattern in C# typically does at least 8 operations are as follows
| Operation         |  Input               |  Output     |  Remark  |
|:------------------|:---------------------|:------------|:---------|
| AddAsync          | <T\>                 | <T\>        | Insert a single record |
| AddRangeAsync     | IEnumerable<T\>      | boolean     | Insert all record of collection |
| UpdateAsync       | <T\>                 | <T\>        | Update a single record |
| RemoveAsync       | <T\>                 | boolean     | Remove a single record |
| RemoveRangeAsync  | IEnumerable<T\>      | boolean     | Remove all record of collection |
| GetAsync | Expression<Func<T, bool>>, <br />params Expression<Func<T, object>>[] | <T\> | Selecting a single record based on its primary key |
| ListAsync | Expression<Func<T, bool>>,<br /> params Expression<Func<T, object>>[] | List<T\> | Selecting any records from a table |
| All | params Expression<Func<T, object>>[] | List<T\> | Selecting all records from a table |

 #### Implementation
```C#
public async Task<ServiceResponse<AppUserItemViewResponse>> GetUserById(int id)
{
    ServiceResponse<AppUserItemViewResponse> response = ServiceResponse<AppUserItemViewResponse>();
    
    // init DbSet
    // # เรียกใช้ AsyncRepository จาก unit of work โดยต้องกำหนด model เพื่อรับส่งข้อมูลจาก database
    var userRepository = _unitOfWork.AsyncRepository<Appusers>();

    // call userRepository mathod เพื่อเรียกใช้ GetAsync สำหรับการ query data
    // # i => i.Role คือการขอข้อมูลที่เกี่ยวข้องกับตาราง Role มาด้วย (include)
    var user = await userRepository.GetAsync(x => x.Id == id, i => i.Role);

    if (user != null)
    {
        var dtoResult = _mapper.Map<AppUserItemViewResponse>(user);
        response.Data = dtoResult;
        response.Success = true;
        response.Message = "OK";
    }

    return response;
}
```

### Repository class
ในกรณีที่เราจำเป็นต้องเรียกใช้งานจากข้อมูลหลายๆตาราง (join table multiple) เราสามารถสร้าง repository class ของตัวเองเพื่อเก็บ method ที่จำเป็นภายในนั้นได้

#### Getting Started
1. สร้าง repository ของตัวเองได้ที่ `Widely.DataAccess` > `Repositories`
2. สร้าง folder โดยใช้ชื่อตาม module ของตัวเอง
3. สร้าง interface โดยใช้ชื่อตาม module ของตัวเองเช่น `IExampleRepository.cs`
4. สร้าง class โดยใช้ชื่อตาม module ของตัวเองเช่น `ExampleRepository.cs`
5. กำหนด [Dependencies Injection](#dependencies-injection) 

#### Implementation
```C#
// repository interface
 public interface IExampleRepository : IGenericRepository<Widely.DataAccess.DataContext.Entities.Appusers>
{
    Task<List<Widely.DataAccess.DataContext.Entities.Appusers>> GetUserAllRelated();
}
```

```C#
// repository class
 public class ExampleRepository : GenericRepository<Widely.DataAccess.DataContext.Entities.Appusers>, IAppusersRepository
{
    private readonly WidelystandartContext _context;
    public ExampleRepository(WidelystandartContext context) : base(context)
    {
        _context = context;
    
    public async Task<List<DataContext.Entities.Appusers>> GetUserAllRelated()
    {
        var result = await _context.Appusers
            .Include(i => i.Role)
            .ToListAsync()
        return result;
    }
}
```

```C#
// service file
public class ExampleService
{
    private readonly IExampleRepository _exampleRepository; // #1. add this

    public ExampleService(IExampleRepository exampleRepository)  // #2. add this
    {
        this._exampleRepository = exampleRepository; // #3. add this
    }

    public async Task<ServiceResponse<AppUserItemViewResponse>> GetUser()
    {

        ServiceResponse<AppUserItemViewResponse> response = new ServiceResponse<AppUserItemViewResponse>();

        // init DbSet
        var userRepository = _unitOfWork.AsyncRepository<Appusers>();

        // call here
        var result = await _appusersRepository.GetUserAllRelated();

        if (result != null)
        {
            var dtoResult = _mapper.Map<AppUserItemViewResponse>(user);
            response.Data = dtoResult;
            response.Success = true;
            response.Message = "OK";
        }
        return response;
    }
}
```
<br /><br />

## Services
### BaseService Class (Common)
BaseService คือ class ตรงกลางที่ให้ไฟล์ service สามารถเรียกใช้เพื่อทราบข้อมูลเกี่ยวกับ http context นั้น ๆ request เข้ามาเช่น userid, username, ip address ต่าง ๆ

### Service Class (By Module)
#### Getting Started
1. สร้างไฟล์ service ของตัวเองได้ที่ `Widely.BusinessLogic` > `Services`
2. สร้าง folder โดยใช้ชื่อตาม module ของตัวเอง
3. สร้าง service class โดยใช้ชื่อตาม module ของตัวเองเช่น `ExampleService.cs`
4. กำหนด [Dependencies Injection](#dependencies-injection) 

#### Implementation
```C#
// controller file
public class ExampleController : ControllerBase
{
    private readonly ExampleService _exampleService; // #1 add this

    public ExampleController(ExampleService exampleService) // #2 add this
    {
        this._exampleService = exampleService;  // #3 add this
    }

    [ModulePermission("USERS", "*")]
    [HttpPost("list")]
    public async Task<IActionResult> GetUserList(SearchCriteria<AppUserListViewRequest> request)
    {
        var result = await this._appusersService.GetList(request); // call this
        return Ok(result);
    }
}
```
<br /><br />

## Swagger
swagger คือ api document โดยใช้ภาษา xml ในการเขียน document เพิ่มเติมได้ และสามารถทดสอบการ input, output ของข้อมูลได้ [*Documentation*](https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-5.0&tabs=visual-studio)

> ในกรณีที่ controller หรือ action ถูกคุมด้วย header attributes `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` จำเป็นต้อง input access token (jwt) ผ่านทางหน้า api document (รูปกุญแจ)

<br /><br />

## Logging
การ logging เราใช้ [**NLog**](https://nlog-project.org/) ในการ logging ภายในต่าง ๆ เช่น exception, auth [*Documentation*](https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-5)

- สามารถ config ได้ที่ `Widely.API` > `nlog.config`
- กรณีเกิดการ exception จากระบบจะถูก logging โดยการเขียนเป็น txt file และบน database

<br /><br />

## Error Handling
ทุกครั้งที่เกิด error จะถูกจัดการโดย middleware ที่ `WidelyAPI` > `Extensions` > `ExceptionMiddlewareExtensions.cs`

| Excaption |  Status Code |
|:----------|:------------:|
| AppException | 400 |
| UnauthorizeException | 401 |
| KeyNotFoundException | 404 |
| Exception (default) | 500 |

> Tips: กรณีต้องการเพิ่ม Exception อื่น ๆ เอง สามารถสร้างเองได้ที่ `Widely.Infrastructure` > `Exception` และนำมาเพิ่มที่ไฟล์ `ExceptionMiddlewareExtensions.cs`


### Implementation
Example 1:
```C#
// service file
public async Task<ServiceResponse<bool>> Update(AppUserUpdateRequest request)
{
      var userRepository = _unitOfWork.AsyncRepository<Appusers>();
      var user = await userRepository.GetAsync(x => x.Id == request.id);

      if (user == null)
      {
          throw new AppException("User not found."); // <--- throw here.
      }

      // else bla bla bla
}
```

Example 2:
```C#
// service file
public async Task<ServiceResponse<bool>> Create(AppUserUpdateRequest request)
{
      try
      {

      }
      catch(Exception ex)
      {
          throw new AppException(ex.Message); // <--- throw here.
      }
}
```
> Tips: ไม่จำเป็นต้องใช้ try catch เสมอไป ตัว middleware จะจัดการ error ให้เองโดยอัตโนมัติ

<br /><br />

## AutoMapper
หากต้องการ mapping data ระหว่าง model-model สามารถใช้ [**AutoMapper**](https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-5.0&tabs=visual-studio) เป็นทางเลือกและสร้างตัวจัดการได้ที่ `Widely.Infrastructure` > `AutoMapper` > `AutoMapperProfile.cs`

### Implementation
Example:
กรณีนี้เราต้องการ map model จาก `Appusers` ซึ่งเป็นข้อมูลจาก database ให้เป็น `AppUserItemViewResponse` เพื่อส่งข้อมูลออกไป

```C#
namespace Widely.Infrastructure.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Appusers, AppUserItemViewResponse>()
                .ForMember(dest => dest.RoleId, src => src.MapFrom(s => s.Role == null ? (int?)null : s.Role.Id))
                .ForMember(dest => dest.RoleName, src => src.MapFrom(s => s.Role == null ? string.Empty : s.Role.Name))
                .ForMember(dest => dest.RoleDescription, src => src.MapFrom(s => s.Role == null ? string.Empty : s.Role.Description))
                ;

        }
    }
}

```

```C#
// service file
public async Task<ServiceResponse<AppUserItemViewResponse>> GetUserById(int id)
{
    ServiceResponse<AppUserItemViewResponse> response = ServiceResponse<AppUserItemViewResponse>();
    var userRepository = _unitOfWork.AsyncRepository<Appusers>();
    var user = await userRepository.GetAsync(x => x.Id == id, i => i.Rol
    if (user != null)
    {
        var dtoResult = _mapper.Map<AppUserItemViewResponse>(user); // add here
        response.Data = dtoResult;
        response.Success = true;
        response.Message = "OK";

    return response;
}
```
