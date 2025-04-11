using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class StudentClassHistory
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int ClassId { get; set; }

    public DateOnly ChangeDate { get; set; }

    public string? Class { get; set; }

    public virtual Class ClassNavigation { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
