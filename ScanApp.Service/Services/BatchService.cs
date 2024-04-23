using ScanApp.Data.Entities;
using ScanApp.Data.Infrastructure;
using ScanApp.Data.Infrastructure.Interface;
using ScanApp.Data.Repositories;
using ScanApp.Model.Requests.Batch;
using ScanApp.Service.Constracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Service.Services
{
    public class BatchService : IBatchService
    {
        private readonly IBatchRepo _batchRepo;
        private readonly IUnitOfWork _unitOfWork;

        public BatchService(ScanContext context) 
        {
            _batchRepo = new BatchRepo(context);
            _unitOfWork = new UnitOfWork(context);
        }

        public async Task<int> Create(BatchCreateRequest request)
        {
            try
            {
                Batch batch = new Batch()
                {
                    BatchName = request.BatchName,
                    BatchPath = request.BatchPath,
                    Note = request.Note,
                    CreatedDate = request.CreatedDate
                };

                await _batchRepo.AddAsync(batch);
                await _unitOfWork.Save();

                return batch.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Batch>> GetAll()
        {
            return await _batchRepo.GetAllAsync();
        }
    }
}
