using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Data.Infrastructure.Interface
{
    public interface IUnitOfWork
    {
        void CreateTransaction();

        void Commit();

        void Rollback();

        Task Save();
    }
}
