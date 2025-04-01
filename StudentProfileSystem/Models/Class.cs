using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class Class
{
    public int Id { get; set; }

    public string ClassesNumber { get; set; } = null!;

    public string? Letter { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
