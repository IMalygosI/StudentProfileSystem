using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class OlympiadsType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Olympiad> Olympiads { get; set; } = new List<Olympiad>();
}
