using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class StudentCertificateAndMedal
{
    public int Id { get; set; }

    public int StudentsId { get; set; }

    public int CertificateAndMedalsFactId { get; set; }

    public virtual CertificateAndMedalsFact CertificateAndMedalsFact { get; set; } = null!;

    public virtual Student Students { get; set; } = null!;
}
