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
        private readonly ScanContext _context;


        public BatchService() 
        {
            _batchRepo = new BatchRepo();
            _context = new ScanContext();
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

                await _context.Batches.AddAsync(batch);
                await _context.SaveChangesAsync();
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
