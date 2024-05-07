using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Model.Requests.Batch
{
    public class BatchUpdateRequest
    {
        public int Id { get; set; }

        public string BatchName { get; set; } = null!;

        public string? Note { get; set; }

        public string? NumberingFont { get; set; }

        public string? DocumentRack { get; set; }

        public string? DocumentShelf { get; set; }

        public string? NumericalTableOfContents { get; set; }

        public string? FileCabinet { get; set; }
    }
}
