using ScanApp.Data.Entities;
using ScanApp.Model.Requests.Batch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Service.Constracts
{
    public interface IBatchService
    {
        Task<int> Create(BatchCreateRequest request);
        Task<IEnumerable<Batch>> GetAll();
    }
}
