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

    public virtual Class Class { get; set; } = null!;

    public virtual School School { get; set; } = null!;

    public virtual ICollection<StudentGiaResult> StudentGiaResults { get; set; } = new List<StudentGiaResult>();

    public virtual ICollection<StudentOlympiadParticipation> StudentOlympiadParticipations { get; set; } = new List<StudentOlympiadParticipation>();
}
