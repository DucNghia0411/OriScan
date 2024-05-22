using Microsoft.EntityFrameworkCore;
using ScanApp.Data.Entities;
using ScanApp.Data.Infrastructure;
using ScanApp.Data.Infrastructure.Interface;
using ScanApp.Data.Repositories;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Batch;
using ScanApp.Model.Requests.Document;
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
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepo _documentRepo;
        private readonly IUnitOfWork _unitOfWork;

        public DocumentModel? SelectedDocument { get; set; }

        public DocumentService(ScanContext context) 
        {
            _documentRepo = new DocumentRepo(context);
            _unitOfWork = new UnitOfWork(context);
        }

        public void SetDocument(DocumentModel document)
        {
            SelectedDocument = document;
        }

        public void ClearSelectedDocument()
        {
            SelectedDocument = null;
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
                    CreatedDate = FormatDateTime(request.CreatedDate),
                    AgencyIdentifier = request.AgencyIdentifier,
                    DocumentIdentifier = request.DocumentIdentifier,
                    NumberOfSheets = request.NumberOfSheets,
                    StartDate = FormatDateTime(request.StartDate),
                    EndDate = FormatDateTime(request.EndDate),
                    StoragePeriod = request.StoragePeriod
                };

                await _documentRepo.AddAsync(document);
                await _unitOfWork.Save();

                int result = document.Id;

                _unitOfWork.ClearChangeTracker();

                return result;
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

        public async Task<bool> DeleteByBatch(int batchId)
        {
            try
            {
                var documents = await _documentRepo.GetAsync(x => x.BatchId == batchId);

                if (documents == null || !documents.Any())
                {
                    return false;
                }

                _documentRepo.DeleteRange(documents);
                await _unitOfWork.Save();

                return true;
            }
            catch(Exception)
            {
                throw;
            }
        }

        public async Task<int> Update(DocumentUpdateRequest request)
        {
            try
            {
                var editDocument = await _documentRepo.GetByIdAsync(request.Id);

                if (editDocument == null)
                {
                    return 0;
                }

                editDocument.DocumentName = request.DocumentName;
                editDocument.Note = request.Note;
                editDocument.AgencyIdentifier = request.AgencyIdentifier;
                editDocument.DocumentIdentifier = request.DocumentIdentifier;
                editDocument.NumberOfSheets = request.NumberOfSheets;
                editDocument.StartDate = FormatDateTime(request.StartDate);
                editDocument.EndDate = FormatDateTime(request.EndDate);
                editDocument.StoragePeriod = request.StoragePeriod;

                _documentRepo.Update(editDocument);
                await _unitOfWork.Save();
                _unitOfWork.ClearChangeTracker();

                return editDocument.Id;
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
                await _documentRepo.DeleteAsync(id);
                await _unitOfWork.Save();

                return true;
            }
            catch (Exception)
            {
                throw;

            }
        }

        public async Task<bool> CheckExisted(int batchId, string documentName)
        {
            var batch = await _documentRepo.FirstOrDefaultAsync(e => (e.BatchId == batchId && e.DocumentName == documentName));

            if (batch == null)
            {
                return false;
            }

            return true;
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
