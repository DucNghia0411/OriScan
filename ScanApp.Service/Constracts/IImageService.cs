using ScanApp.Model.Models;
using ScanApp.Model.Requests.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Service.Constracts
{
    public interface IImageService
    {
        ImageModel? SelectedImage { get; set; }

        void SetImage(ImageModel image);

        Task<int> Create(ImageCreateRequest request);

        Task<bool> DeleteByDocument(int documentId);
    }
}
