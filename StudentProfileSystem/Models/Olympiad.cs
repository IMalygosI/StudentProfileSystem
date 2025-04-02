using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class Olympiad
{
    public int Id { get; set; }

    public int Olympiads { get; set; }

    public int OlympiadsItems { get; set; }

    public virtual Item OlympiadsItemsNavigation { get; set; } = null!;

    public virtual OlympiadsType OlympiadsNavigation { get; set; } = null!;

    public virtual ICollection<StudentOlympiadParticipation> StudentOlympiadParticipations { get; set; } = new List<StudentOlympiadParticipation>();
}
