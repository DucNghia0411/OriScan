using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Model.Requests.Document
{
    public class DocumentCreateRequest
    {
        public int BatchId { get; set; }

        public string DocumentName { get; set; } = null!;

        public string DocumentPath { get; set; } = null!;

        public string? Note { get; set; }

        public string CreatedDate { get; set; } = null!;
    }
}
