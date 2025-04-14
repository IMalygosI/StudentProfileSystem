using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class Student
{
    public int Id { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public int ClassId { get; set; }

    public int SchoolId { get; set; }

    public int? EducationalInstitutionId { get; set; }

    public int? TypeEducation { get; set; }

    public int? ProfileId { get; set; }

    public string? NameEducationalInstitution { get; set; }

    public string? NameProfile { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual EducationalInstitution? EducationalInstitution { get; set; }

    public virtual Profile? Profile { get; set; }

    public virtual School School { get; set; } = null!;

    public virtual ICollection<StudentCertificateAndMedal> StudentCertificateAndMedals { get; set; } = new List<StudentCertificateAndMedal>();

    public virtual ICollection<StudentClassHistory> StudentClassHistories { get; set; } = new List<StudentClassHistory>();

    public virtual ICollection<StudentGiaResult> StudentGiaResults { get; set; } = new List<StudentGiaResult>();

    public virtual ICollection<StudentOlympiadParticipation> StudentOlympiadParticipations { get; set; } = new List<StudentOlympiadParticipation>();

    public virtual ICollection<StudentSchoolHistory> StudentSchoolHistories { get; set; } = new List<StudentSchoolHistory>();

    public virtual Education? TypeEducationNavigation { get; set; }

    public virtual ICollection<Staff> IdStaffs { get; set; } = new List<Staff>();
}
