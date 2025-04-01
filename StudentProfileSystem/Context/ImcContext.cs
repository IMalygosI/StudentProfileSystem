using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StudentProfileSystem.Models;

namespace StudentProfileSystem.Context;

public partial class ImcContext : DbContext
{
    public ImcContext()
    {
    }

    public ImcContext(DbContextOptions<ImcContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Olympiad> Olympiads { get; set; }

    public virtual DbSet<School> Schools { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=4cdk.ru:5432;Database=imc;Username=imc_dmitry;password=31415926");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasNoKey();
        });

        modelBuilder.Entity<Olympiad>(entity =>
        {
            entity.HasNoKey();
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasNoKey();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasNoKey();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
