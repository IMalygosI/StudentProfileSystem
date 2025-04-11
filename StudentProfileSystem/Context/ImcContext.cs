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

    public virtual DbSet<Education> Educations { get; set; }

    public virtual DbSet<EducationalInstitution> EducationalInstitutions { get; set; }

    public virtual DbSet<GiaSubject> GiaSubjects { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Olympiad> Olympiads { get; set; }

    public virtual DbSet<OlympiadsType> OlympiadsTypes { get; set; }

    public virtual DbSet<Profile> Profiles { get; set; }

    public virtual DbSet<School> Schools { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentClassHistory> StudentClassHistories { get; set; }

    public virtual DbSet<StudentGiaResult> StudentGiaResults { get; set; }

    public virtual DbSet<StudentOlympiadParticipation> StudentOlympiadParticipations { get; set; }

    public virtual DbSet<StudentSchoolHistory> StudentSchoolHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=4cdk.ru:5432;Database=imc;Username=imc_dmitry;password=31415926");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("classes_pk");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.ClassesNumber)
                .HasMaxLength(10)
                .HasColumnName("Classes_Number");
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("education_pk");

            entity.ToTable("Education");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasColumnType("character varying");
        });

        modelBuilder.Entity<EducationalInstitution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("educational_institution_pk");

            entity.ToTable("Educational_Institution");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<GiaSubject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("gia_subjects_pk");

            entity.ToTable("Gia_Subjects");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.GiaSubjects).HasColumnName("Gia_Subjects");

            entity.HasOne(d => d.GiaSubjectsNavigation).WithMany(p => p.GiaSubjects)
                .HasForeignKey(d => d.GiaSubjects)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("gia_subjects_items_fk");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("items_pk");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Olympiad>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("olympiads_pk");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.OlympiadsItems).HasColumnName("Olympiads_Items");

            entity.HasOne(d => d.OlympiadsNavigation).WithMany(p => p.Olympiads)
                .HasForeignKey(d => d.Olympiads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("olympiads_olympiads_type_fk");

            entity.HasOne(d => d.OlympiadsItemsNavigation).WithMany(p => p.Olympiads)
                .HasForeignKey(d => d.OlympiadsItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("olympiads_items_fk");
        });

        modelBuilder.Entity<OlympiadsType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("olympiads_type_pk");

            entity.ToTable("Olympiads_Type");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("profile_pk");

            entity.ToTable("Profile");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasColumnType("character varying");
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("schools_pk");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.SchoolNumber)
                .HasMaxLength(30)
                .HasColumnName("School_Number");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("staff_pk");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasColumnType("character varying");

            entity.HasMany(d => d.IdStudents).WithMany(p => p.IdStaffs)
                .UsingEntity<Dictionary<string, object>>(
                    "User",
                    r => r.HasOne<Student>().WithMany()
                        .HasForeignKey("IdStudent")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("users_students_fk"),
                    l => l.HasOne<Staff>().WithMany()
                        .HasForeignKey("IdStaff")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("users_staff_fk"),
                    j =>
                    {
                        j.HasKey("IdStaff", "IdStudent").HasName("users_pk");
                        j.ToTable("Users");
                        j.IndexerProperty<int>("IdStaff").HasColumnName("ID_Staff");
                        j.IndexerProperty<int>("IdStudent").HasColumnName("ID_Student");
                    });
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("students_pk");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.ClassId).HasColumnName("Class_ID");
            entity.Property(e => e.EducationalInstitutionId).HasColumnName("Educational_Institution_ID");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("First_Name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("Last_Name");
            entity.Property(e => e.Patronymic).HasMaxLength(50);
            entity.Property(e => e.ProfileId).HasColumnName("Profile_ID");
            entity.Property(e => e.SchoolId).HasColumnName("School_ID");
            entity.Property(e => e.TypeEducation).HasColumnName("Type_Education");

            entity.HasOne(d => d.Class).WithMany(p => p.Students)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("students_classes_fk");

            entity.HasOne(d => d.EducationalInstitution).WithMany(p => p.Students)
                .HasForeignKey(d => d.EducationalInstitutionId)
                .HasConstraintName("students_educational_institution_fk");

            entity.HasOne(d => d.Profile).WithMany(p => p.Students)
                .HasForeignKey(d => d.ProfileId)
                .HasConstraintName("students_profile_fk");

            entity.HasOne(d => d.School).WithMany(p => p.Students)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("students_schools_fk");

            entity.HasOne(d => d.TypeEducationNavigation).WithMany(p => p.Students)
                .HasForeignKey(d => d.TypeEducation)
                .HasConstraintName("students_education_fk");
        });

        modelBuilder.Entity<StudentClassHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("student_class_history_pk");

            entity.ToTable("Student_Class_History");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.Class).HasColumnType("character varying");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.ClassNavigation).WithMany(p => p.StudentClassHistories)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_class_history_classes_fk");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentClassHistories)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_class_history_students_fk");
        });

        modelBuilder.Entity<StudentGiaResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("student_gia_results_pk");

            entity.ToTable("Student_Gia_Results");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.IdGiaSubjects).HasColumnName("ID_Gia_Subjects");
            entity.Property(e => e.IdStudents).HasColumnName("ID_Students");

            entity.HasOne(d => d.IdGiaSubjectsNavigation).WithMany(p => p.StudentGiaResults)
                .HasForeignKey(d => d.IdGiaSubjects)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_gia_results_gia_subjects_fk");

            entity.HasOne(d => d.IdStudentsNavigation).WithMany(p => p.StudentGiaResults)
                .HasForeignKey(d => d.IdStudents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_gia_results_students_fk");
        });

        modelBuilder.Entity<StudentOlympiadParticipation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("student_olympiad_participations_pk");

            entity.ToTable("Student_Olympiad_Participations");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.IdOlympiads).HasColumnName("ID_Olympiads");
            entity.Property(e => e.IdStudents).HasColumnName("ID_Students");

            entity.HasOne(d => d.IdOlympiadsNavigation).WithMany(p => p.StudentOlympiadParticipations)
                .HasForeignKey(d => d.IdOlympiads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_olympiad_participations_olympiads_fk");

            entity.HasOne(d => d.IdStudentsNavigation).WithMany(p => p.StudentOlympiadParticipations)
                .HasForeignKey(d => d.IdStudents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_olympiad_participations_students_fk");
        });

        modelBuilder.Entity<StudentSchoolHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("student_school_history_pk");

            entity.ToTable("Student_School_History");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID");
            entity.Property(e => e.School).HasColumnType("character varying");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.SchoolNavigation).WithMany(p => p.StudentSchoolHistories)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_school_history_schools_fk");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentSchoolHistories)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_school_history_students_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
