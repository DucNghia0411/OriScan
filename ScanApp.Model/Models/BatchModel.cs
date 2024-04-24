using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Model.Models
{
    public class BatchModel
    {
        public int Id { get; set; }

        public string BatchName { get; set; } = null!;

        public string BatchPath { get; set; } = null!;
    }
}
