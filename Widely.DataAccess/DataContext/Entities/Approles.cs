﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

#nullable disable

namespace Widely.DataAccess.DataContext.Entities
{
    public partial class Approles
    {
        public Approles()
        {
            Apppermission = new HashSet<Apppermission>();
            Appusers = new HashSet<Appusers>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<Apppermission> Apppermission { get; set; }
        public virtual ICollection<Appusers> Appusers { get; set; }
    }
}