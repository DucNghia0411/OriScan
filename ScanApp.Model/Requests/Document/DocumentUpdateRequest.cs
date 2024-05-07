using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Model.Requests.Document
{
    public class DocumentUpdateRequest
    {
        public int Id { get; set; }

        public string DocumentName { get; set; } = null!;

        public string? Note { get; set; }

        public string? AgencyIdentifier { get; set; }

        public string? DocumentIdentifier { get; set; }

        public int NumberOfSheets { get; set; }

        public string? StartDate { get; set; }

        public string? EndDate { get; set; }

        public string? StoragePeriod { get; set; }
    }
}
