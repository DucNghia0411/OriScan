using System;
using System.Collections.Generic;

namespace ScanApp.Data.Entities;

public partial class Document
{
    public int Id { get; set; }

    public int BatchId { get; set; }

    public string DocumentName { get; set; } = null!;

    public string DocumentPath { get; set; } = null!;

    public string? Note { get; set; }

    public string CreatedDate { get; set; } = null!;

    public virtual Batch Batch { get; set; } = null!;

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();
}
