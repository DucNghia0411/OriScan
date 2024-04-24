using ScanApp.Data.Entities;
using ScanApp.Data.Infrastructure;
using ScanApp.Data.Infrastructure.Interface;
using ScanApp.Data.Repositories;
using ScanApp.Model.Requests.Document;
using ScanApp.Service.Constracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Service.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepo _documentRepo;
        private readonly IUnitOfWork _unitOfWork;

        public DocumentService(ScanContext context) 
        {
            _documentRepo = new DocumentRepo(context);
            _unitOfWork = new UnitOfWork(context);
        }

        public async Task<IEnumerable<Document>> Get(Expression<Func<Document, bool>> predicate)
        {
            return await _documentRepo.GetAsync(predicate);
        }

        public async Task<Document?> FirstOrDefault(Expression<Func<Document, bool>> predicate)
        {
            return await _documentRepo.FirstOrDefaultAsync(predicate);
        }

        public async Task<int> Create(DocumentCreateRequest request)
        {
            try
            {
                Document document = new Document()
                {
                    BatchId = request.BatchId,
                    DocumentName = request.DocumentName,
                    DocumentPath = request.DocumentPath,
                    Note = request.Note,
                    CreatedDate = request.CreatedDate
                };

                await _documentRepo.AddAsync(document);
                await _unitOfWork.Save();

                return document.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Document>> GetAll()
        {
            return await _documentRepo.GetAllAsync();
        }
    }
}
