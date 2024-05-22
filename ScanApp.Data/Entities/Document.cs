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

    public string? AgencyIdentifier { get; set; }

    public string? DocumentIdentifier { get; set; }

    public int NumberOfSheets { get; set; }

    public string? StartDate { get; set; }

    public string? EndDate { get; set; }

    public string? StoragePeriod { get; set; }

    public string? PdfPath { get; set; }

    public virtual Batch Batch { get; set; } = null!;

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();
}
