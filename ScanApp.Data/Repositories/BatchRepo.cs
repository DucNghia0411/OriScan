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
    public interface IBatchRepo : IGenericRepository<Batch> { }

    public class BatchRepo : GenericRepository<Batch>, IBatchRepo
    {
        public BatchRepo(ScanContext context) : base(context) { }
    }
}
