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
    public interface IDocumentRepo : IGenericRepository<Document> { }

    public class DocumentRepo : GenericRepository<Document>, IDocumentRepo
    {
        public DocumentRepo(ScanContext context) : base(context) { }
    }
}
