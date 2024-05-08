using ScanApp.Data.Entities;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Service.Constracts
{
    public interface IImageService
    {
        ImageModel? SelectedImage { get; set; }

        void SetImage(ImageModel image);

        Task<IEnumerable<Image>> Get(Expression<Func<Image, bool>> predicate);

        Task<int> Create(ImageCreateRequest request);

        Task AddRange(List<Image> images);

        Task Save();
    }
}
