using Microsoft.EntityFrameworkCore;
using ScanApp.Data.Entities;
using ScanApp.Data.Infrastructure;
using ScanApp.Data.Infrastructure.Interface;
using ScanApp.Data.Repositories;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Batch;
using ScanApp.Service.Constracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Service.Services
{
    public class BatchService : IBatchService
    {
        private readonly IBatchRepo _batchRepo;
        private readonly IUnitOfWork _unitOfWork;

        public BatchModel? SelectedBatch { get; set; }

        public BatchService(ScanContext context) 
        {
            _batchRepo = new BatchRepo(context);
            _unitOfWork = new UnitOfWork(context);
        }

        public void SetBatch(BatchModel batch)
        {
            SelectedBatch = batch;
        }

        public async Task<IEnumerable<Batch>> Get(Expression<Func<Batch, bool>> predicate)
        {
            return await _batchRepo.GetAsync(predicate);
        }

        public async Task<Batch?> FirstOrDefault(Expression<Func<Batch, bool>> predicate)
        {
            return await _batchRepo.FirstOrDefaultAsync(predicate);
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

                _unitOfWork.ClearChangeTracker();

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

        public async Task<int> Update(BatchUpdateRequest request)
        {
            try
            {
                var editBatch = await _batchRepo.GetByIdAsync(request.Id);

                if (editBatch == null)
                {
                    return 0;
                }

                editBatch.BatchName = request.BatchName;
                editBatch.Note = request.Note;

                _batchRepo.Update(editBatch);
                await _unitOfWork.Save();

                _unitOfWork.ClearChangeTracker();

                return editBatch.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                await _batchRepo.DeleteAsync(id);
                await _unitOfWork.Save();

                return true;
            }
            catch (Exception)
            {
                throw;

            }
        }

        public async Task<bool> CheckExisted(string batchName)
        {
            var batch = await _batchRepo.FirstOrDefaultAsync(e => e.BatchName.Trim().ToUpper() == batchName.Trim().ToUpper());

            if (batch == null)
            {
                return false;
            }

            return true;
        }
    }
}
