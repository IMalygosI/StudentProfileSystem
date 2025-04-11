using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class Class
{
    public int Id { get; set; }

    public string ClassesNumber { get; set; } = null!;

    public virtual ICollection<StudentClassHistory> StudentClassHistories { get; set; } = new List<StudentClassHistory>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
