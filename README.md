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
- Dependencies Injection
    - Repositories
      - GenericRepository Class (Common)
      - Repository Class (By Module)
    - Services
      - BaseService Class (Common)
      - Service Class (By Module)
- Swagger
- Logger
- Error Handling
- AutoMapper
- Example

# Contents
## Prerequisites
- .Net 5.x [*Download*](https://dotnet.microsoft.com/download/dotnet/5.0)
- Visual Studio 2019 (v16.11 >=) [*Download*](https://visualstudio.microsoft.com/downloads/)
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
     .AddDatabase(Configuration) // <-- Add here.
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
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //<-- Add here
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



