using ScanApp.Data.Entities;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Batch;
using ScanApp.Model.Requests.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Service.Constracts
{
    public interface IBatchService
    {
        BatchModel? SelectedBatch { get; set; }

        void SetBatch(BatchModel batch);

        void ClearSelectedBatch();

        Task<Batch?> FirstOrDefault(Expression<Func<Batch, bool>> predicate);

        Task<IEnumerable<Batch>> Get(Expression<Func<Batch, bool>> predicate);

        Task<int> Create(BatchCreateRequest request);

        Task<IEnumerable<Batch>> GetAll();

        Task<int> Update(BatchUpdateRequest request);

        Task<bool> Delete(int id);

        Task<bool> CheckExisted(string batchName);
    }
}
