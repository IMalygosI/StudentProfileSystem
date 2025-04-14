using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class CertificateAndMedalsFact
{
    public int Id { get; set; }

    public int CertificateAndMedalsId { get; set; }

    public int CertificateAndMedalsCheckId { get; set; }

    public virtual CertificateAndMedal CertificateAndMedals { get; set; } = null!;

    public virtual CertificateAndMedalsCheck CertificateAndMedalsCheck { get; set; } = null!;

    public virtual ICollection<StudentCertificateAndMedal> StudentCertificateAndMedals { get; set; } = new List<StudentCertificateAndMedal>();
}
