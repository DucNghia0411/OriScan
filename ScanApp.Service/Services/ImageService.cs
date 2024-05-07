using ScanApp.Data.Entities;
using ScanApp.Data.Infrastructure;
using ScanApp.Data.Infrastructure.Interface;
using ScanApp.Data.Repositories;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Image;
using ScanApp.Service.Constracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Service.Services
{
    public class ImageService: IImageService
    {
        private readonly IImageRepo _imageRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ImageModel? SelectedImage { get; set; }

        public ImageService(ScanContext context)
        {
            _imageRepo = new ImageRepo(context);
            _unitOfWork = new UnitOfWork(context);
        }

        public void SetImage(ImageModel image)
        {
            SelectedImage = image;
        }

        public async Task<int> Create(ImageCreateRequest request)
        {
            try
            {
                Image image = new Image()
                {
                    DocumentId = request.DocumentId,
                    ImageName = request.ImageName,
                    ImagePath = request.ImagePath,
                    CreatedDate = request.CreatedDate
                };

                await _imageRepo.AddAsync(image);
                await _unitOfWork.Save();
                _unitOfWork.ClearChangeTracker();
                return image.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
