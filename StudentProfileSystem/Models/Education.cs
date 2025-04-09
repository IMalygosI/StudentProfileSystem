using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class Education
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
