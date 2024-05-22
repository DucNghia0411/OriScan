using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Model.Requests.Document
{
    public class DocumentToPdfRequest
    {
        public int Id { get; set; }

        public string PdfPath { get; set; } = null!;
    }
}
