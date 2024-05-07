using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginalScan.Models
{
    public class ImageViewModel
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }

        public string ImagePath { get; set; } = null!;

        public bool IsSelected { get; set; }
    }
}
