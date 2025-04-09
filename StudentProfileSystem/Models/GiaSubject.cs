using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class GiaSubject
{
    public int Id { get; set; }

    public int GiaSubjects { get; set; }

    public virtual Item GiaSubjectsNavigation { get; set; } = null!;

    public virtual ICollection<StudentGiaResult> StudentGiaResults { get; set; } = new List<StudentGiaResult>();
}
