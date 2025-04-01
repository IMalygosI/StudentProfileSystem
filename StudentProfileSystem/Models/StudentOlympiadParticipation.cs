using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class StudentOlympiadParticipation
{
    public int Id { get; set; }

    public int IdStudents { get; set; }

    public int IdOlympiads { get; set; }

    public virtual Olympiad IdOlympiadsNavigation { get; set; } = null!;

    public virtual Student IdStudentsNavigation { get; set; } = null!;
}
