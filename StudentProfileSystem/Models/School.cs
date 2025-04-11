using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class School
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string SchoolNumber { get; set; } = null!;

    public string? Password { get; set; }

    public virtual ICollection<StudentSchoolHistory> StudentSchoolHistories { get; set; } = new List<StudentSchoolHistory>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
