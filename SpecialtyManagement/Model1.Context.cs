﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SpecialtyManagement
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class SpecialtyManagementEntities : DbContext
    {
        public SpecialtyManagementEntities()
            : base("name=SpecialtyManagementEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Arrears> Arrears { get; set; }
        public virtual DbSet<ArrearsLessons> ArrearsLessons { get; set; }
        public virtual DbSet<DistributionLessons> DistributionLessons { get; set; }
        public virtual DbSet<Groups> Groups { get; set; }
        public virtual DbSet<Lessons> Lessons { get; set; }
        public virtual DbSet<Specialty> Specialty { get; set; }
        public virtual DbSet<Students> Students { get; set; }
        public virtual DbSet<Teachers> Teachers { get; set; }
        public virtual DbSet<TypesArrears> TypesArrears { get; set; }
        public virtual DbSet<TypesLessons> TypesLessons { get; set; }
    }
}
