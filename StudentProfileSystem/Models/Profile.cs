using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class Profile
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
