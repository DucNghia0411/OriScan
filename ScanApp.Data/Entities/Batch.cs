using System;
using System.Collections.Generic;

namespace ScanApp.Data.Entities;

public partial class Batch
{
    public int Id { get; set; }

    public string BatchName { get; set; } = null!;

    public string BatchPath { get; set; } = null!;

    public string? Note { get; set; }

    public string CreatedDate { get; set; } = null!;

    public string? NumberingFont { get; set; }

    public string? DocumentRack { get; set; }

    public string? DocumentShelf { get; set; }

    public string? NumericalTableOfContents { get; set; }

    public string? FileCabinet { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
