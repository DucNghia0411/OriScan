using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Model.Requests.Batch
{
    public class BatchCreateRequest
    {
        public string BatchName { get; set; } = null!;

        public string BatchPath { get; set; } = null!;

        public string? Note { get; set; }

        public string CreatedDate { get; set; } = null!;
    }
}
