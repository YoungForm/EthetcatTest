﻿using Microsoft.EntityFrameworkCore;
using EtherCATTestApi.DataAccess.Models;

namespace EtherCATTestApi.DataAccess.DbContext
{
    public class EtherCATDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public EtherCATDbContext(DbContextOptions<EtherCATDbContext> options) : base(options)
        {
        }
        
        // DbSets for our models
        public DbSet<DeviceConfig> DeviceConfigs { get; set; }
        public DbSet<TestSession> TestSessions { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships
            modelBuilder.Entity<TestSession>()
                .HasOne(ts => ts.DeviceConfig)
                .WithMany(dc => dc.TestSessions)
                .HasForeignKey(ts => ts.DeviceConfigId);
            
            modelBuilder.Entity<TestResult>()
                .HasOne(tr => tr.TestSession)
                .WithMany(ts => ts.TestResults)
                .HasForeignKey(tr => tr.TestSessionId);
        }
    }
}