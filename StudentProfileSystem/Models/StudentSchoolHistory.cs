using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class StudentSchoolHistory
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int SchoolId { get; set; }

    public DateOnly ChangeDate { get; set; }

    public string? School { get; set; }

    public virtual School SchoolNavigation { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
