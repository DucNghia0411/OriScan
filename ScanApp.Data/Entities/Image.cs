using System;
using System.Collections.Generic;

namespace ScanApp.Data.Entities;

public partial class Image
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public string ImageName { get; set; } = null!;

    public string ImagePath { get; set; } = null!;

    public string CreatedDate { get; set; } = null!;

    public int Order { get; set; }

    public virtual Document Document { get; set; } = null!;
}
