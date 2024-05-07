using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Model.Models
{
    public class DocumentModel
    {
        public int Id { get; set; }

        public int BatchId { get; set; }

        public string DocumentName { get; set; } = null!;

        public string DocumentPath { get; set; } = null!;

        public int NumberOfSheets { get; set; }
    }
}
