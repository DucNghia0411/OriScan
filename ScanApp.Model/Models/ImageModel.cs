using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Model.Models
{
    public class ImageModel
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }

        public string ImagePath { get; set; } = null!;
    }
}
