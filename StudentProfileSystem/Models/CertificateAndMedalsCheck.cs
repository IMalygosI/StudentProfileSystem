using System;
using System.Collections.Generic;

namespace StudentProfileSystem.Models;

public partial class CertificateAndMedalsCheck
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<CertificateAndMedalsFact> CertificateAndMedalsFacts { get; set; } = new List<CertificateAndMedalsFact>();
}
