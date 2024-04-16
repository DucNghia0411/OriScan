using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Model.Requests
{
    public class RequestResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
    }
}
