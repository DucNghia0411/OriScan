using ScanApp.Data.Entities;
using ScanApp.Data.Infrastructure;
using ScanApp.Data.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Data.Repositories
{
    public interface IImageRepo : IGenericRepository<Image> { }

    public class ImageRepo : GenericRepository<Image>, IImageRepo
    {
        public ImageRepo(ScanContext context) : base(context) { }
    }
}
