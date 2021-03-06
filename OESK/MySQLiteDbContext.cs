﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace OESK
{
    public class MySQLiteDbContext : DbContext
    {
        public MySQLiteDbContext() : base(new SQLiteConnection()
        {
            ConnectionString = new SQLiteConnectionStringBuilder()
            {
                DataSource = "db.db",
                ForeignKeys = true
            }.ConnectionString
        }, true)
        { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TableCPU> TableCPU { get; set; }
        public DbSet<TableFunction> TableFunction { get; set; }
        public DbSet<TablePC> TablePC { get; set; }
        public DbSet<TableRAM> TableRAM { get; set; }
        public DbSet<TableTest> TableTest { get; set; }
        public DbSet<TableText> TableText { get; set; }
    }
}
