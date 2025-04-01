using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class Item
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<GiaSubject> GiaSubjects { get; set; } = new List<GiaSubject>();

    public virtual ICollection<Olympiad> Olympiads { get; set; } = new List<Olympiad>();
}
