using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Model.Requests.Image
{
    public class ImageCreateRequest
    {
        public int DocumentId { get; set; }

        public string ImageName { get; set; } = null!;

        public string ImagePath { get; set; } = null!;

        public string CreatedDate { get; set; } = null!;
    }
}
