using ScanApp.Data.Entities;
using ScanApp.Model.Models;
using ScanApp.Model.Requests.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Service.Constracts
{
    public interface IDocumentService
    {
        Task<IEnumerable<Document>> Get(Expression<Func<Document, bool>> predicate);

        Task<Document?> FirstOrDefault(Expression<Func<Document, bool>> predicate);

        Task<int> Create(DocumentCreateRequest request);

        Task<IEnumerable<Document>> GetAll();

        void SetDocument(DocumentModel document);
    }
}
