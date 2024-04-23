using ScanApp.Data.Entities;
using ScanApp.Model.Requests.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Service.Constracts
{
    public interface IDocumentService
    {
        Task<int> Create(DocumentCreateRequest request);
        Task<IEnumerable<Document>> GetAll();
    }
}
