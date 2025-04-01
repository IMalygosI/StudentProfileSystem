using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class Olympiad
{
    public int Id { get; set; }

    public string Olympiads { get; set; } = null!;

    public int OlympiadsItems { get; set; }

    public virtual Item OlympiadsItemsNavigation { get; set; } = null!;

    public virtual ICollection<StudentOlympiadParticipation> StudentOlympiadParticipations { get; set; } = new List<StudentOlympiadParticipation>();
}
