﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PAR_Site.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class NagParTestEntities : DbContext
    {
        public NagParTestEntities()
            : base("name=NagParTestEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<tblDivisionIndex> tblDivisionIndexes { get; set; }
        public virtual DbSet<tblFileLibrary> tblFileLibraries { get; set; }
        public virtual DbSet<tblProjectPhase> tblProjectPhases { get; set; }
        public virtual DbSet<tblSubjectIndex> tblSubjectIndexes { get; set; }
        public virtual DbSet<tblTicketData> tblTicketDatas { get; set; }
        public virtual DbSet<tblTicketIndex> tblTicketIndexes { get; set; }
        public virtual DbSet<tblUserIndex> tblUserIndexes { get; set; }
        public virtual DbSet<tblCategory> tblCategories { get; set; }
        public virtual DbSet<tblCustomerIndex> tblCustomerIndexes { get; set; }
        public virtual DbSet<tblPriorityIndex> tblPriorityIndexes { get; set; }
        public virtual DbSet<tblTechIndex> tblTechIndexes { get; set; }
        public virtual DbSet<tblTicketStatusIndex> tblTicketStatusIndexes { get; set; }
        public virtual DbSet<tblTicketTypeIndex> tblTicketTypeIndexes { get; set; }
        public virtual DbSet<tblTypeOfMap> tblTypeOfMaps { get; set; }
    }
}
