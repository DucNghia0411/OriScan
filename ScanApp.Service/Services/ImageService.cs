using Microsoft.EntityFrameworkCore;
using ScanApp.Data.Entities;
using ScanApp.Data.Infrastructure;
using ScanApp.Data.Infrastructure.Interface;
using ScanApp.Data.Repositories;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Image;
using ScanApp.Service.Constracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
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

        public async Task<IEnumerable<Image>> Get(Expression<Func<Image, bool>> predicate)
        {
            return await _imageRepo.GetAsync(predicate);
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
                    CreatedDate = FormatDateTime(request.CreatedDate),
                    Order = request.Order
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

        public async Task AddRange(List<Image> images)
        {
            await _imageRepo.AddRangeAsync(images);
        }

        public async Task Save()
        {
            await _unitOfWork.Save();
            _unitOfWork.ClearChangeTracker();
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                await _imageRepo.DeleteAsync(id);
                await _unitOfWork.Save();

                return true;
            }
            catch (Exception)
            {
                throw;

            }
        }

        public async Task DeleteMultiById(List<int> listIds)
        {
            try
            {
                IEnumerable<Image> images = await Get(x => listIds.Contains(x.Id));
                _imageRepo.DeleteRange(images);
                await _unitOfWork.Save();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteByDocument(int documentId)
        {
            try
            {
                var images = await _imageRepo.GetAsync(x => x.DocumentId == documentId);

                if (images == null || !images.Any())
                {
                    return false;
                }

                _imageRepo.DeleteRange(images);
                await _unitOfWork.Save();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CountByDocument(int documentId)
        {
            try
            {
                int countResult = await _imageRepo.CountAsync(e => e.DocumentId == documentId);
                return countResult;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ReSort(long? idImage)
        {
            var image = await _imageRepo.FirstOrDefaultAsync(e => e.Id == idImage);
            if (image == null)
                return false;

            IEnumerable<Image> images = await Get(e => e.DocumentId == image.DocumentId);
            List<Image> listImage = images.OrderBy(x => x.Order).ToList();

            int orderToRemove = (int)image.Order;

            for (int i = orderToRemove; i < listImage.Count(); i++)
            {
                listImage[i].Order = listImage[i].Order - 1;
                _imageRepo.Update(listImage[i]);
            }

            try
            {
                await _unitOfWork.Save();
                _unitOfWork.ClearChangeTracker();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string FormatDateTime(string? inputDate)
        {
            if (inputDate == null) return "";
            string formattedDate = "";
            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            DateTimeFormatInfo dateTimeFormat = currentCulture.DateTimeFormat;
            string[] allDatePatterns = dateTimeFormat.GetAllDateTimePatterns();

            bool isConverted = false;

            foreach (string format in allDatePatterns)
            {
                DateTime createdDate;

                if (DateTime.TryParseExact(inputDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out createdDate))
                {
                    formattedDate = createdDate.ToString("dd/MM/yyyy h:mm:ss tt");
                    isConverted = true;
                }
            }

            if (!isConverted)
            {
                formattedDate = inputDate;
            }

            return formattedDate;
        }
    }
}
