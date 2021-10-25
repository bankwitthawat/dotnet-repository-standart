using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Widely.DataAccess.DataContext.Entities;
using System.Security.Cryptography;

namespace Widely.DataAccess.DataContext
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            //Appmodule
            modelBuilder.Entity<Appmodule>()
                .HasData(
                   new Appmodule
                   {
                       Id = 1,
                       Title = "Administrator",
                       Subtitle = "Application Management",
                       Type = "group",
                       IsActive = true,
                       Sequence = 0
                   },
                   new Appmodule
                   {
                       Id = 2,
                       Title = "System Management",
                       Type = "collapsable",
                       Icon = "feather:settings",
                       IsActive = true,
                       Sequence = 10,
                       ParentId = 1
                   },
                   new Appmodule
                   {
                       Id = 3,
                       Title = "Users",
                       Subtitle = "Application Users",
                       Type = "basic",
                       Icon = "feather:users",
                       Path = "/app-user/users",
                       IsActive = true,
                       Sequence = 20,
                       ParentId = 2
                   },
                   new Appmodule
                   {
                       Id = 4,
                       Title = "Roles",
                       Subtitle = "Application Roles",
                       Type = "basic",
                       Icon = "heroicons_outline:user-group",
                       Path = "/app-role/roles",
                       IsActive = true,
                       Sequence = 30,
                       ParentId = 2
                   },
                   new Appmodule
                   {
                       Id = 5,
                       Title = "Modules",
                       Subtitle = "Application Module",
                       Type = "basic",
                       Icon = "feather:list",
                       Path = "/app-module/modules",
                       IsActive = true,
                       Sequence = 40,
                       ParentId = 2
                   }
                );

            // Approles
            modelBuilder.Entity<Approles>()
                .HasData(
                    new Approles { Id = 1, Name = "SUPERADMIN", Description = "Super Admin", CreatedBy = "System", CreatedDate = DateTime.Now }
                );

            // Apppermission
            modelBuilder.Entity<Apppermission>()
                .HasData(
                    new Apppermission { RoleId = 1, ModuleId = 3, IsAccess = true, IsCreate = true, IsEdit = true, IsView = true, IsDelete = true, ModifiedBy = "System", ModifiedDate = DateTime.Now },
                    new Apppermission { RoleId = 1, ModuleId = 4, IsAccess = true, IsCreate = true, IsEdit = true, IsView = true, IsDelete = true, ModifiedBy = "System", ModifiedDate = DateTime.Now },
                    new Apppermission { RoleId = 1, ModuleId = 5, IsAccess = true, IsCreate = true, IsEdit = true, IsView = true, IsDelete = true, ModifiedBy = "System", ModifiedDate = DateTime.Now }
                );

            // Appusers
            string username = "admin";
            string password = "P@ssw0rd";
            CreatePasswordHash(username, password, out byte[] passwordHash, out byte[] passwordSalt);
            modelBuilder.Entity<Appusers>()
                .HasData(
                    new Appusers { Id = 1, Username = username, PasswordHash = passwordHash, PasswordSalt = passwordSalt, IsActive = true, LoginAttemptCount = 0, CreatedBy = "System", CreatedDate = DateTime.Now }
                );
        }

        public static void CreatePasswordHash(string username, string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(username + password));
            }
        }
    }
}
