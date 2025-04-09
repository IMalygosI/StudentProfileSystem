using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class Staff
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Student> IdStudents { get; set; } = new List<Student>();
}
