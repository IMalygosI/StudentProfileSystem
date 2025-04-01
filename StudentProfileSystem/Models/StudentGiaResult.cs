using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class StudentGiaResult
{
    public int Id { get; set; }

    public int IdStudents { get; set; }

    public int IdGiaSubjects { get; set; }

    public virtual GiaSubject IdGiaSubjectsNavigation { get; set; } = null!;

    public virtual Student IdStudentsNavigation { get; set; } = null!;
}
