using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ScanApp.Data.Entities;
using ScanApp.Data.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Data.Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ScanContext _context;
        private IDbContextTransaction? _objTran = null;

        public UnitOfWork (ScanContext context)
        {
            _context = context;
        }

        public void CreateTransaction()
        {
            _objTran = _context.Database.BeginTransaction();
        }

        public virtual async Task Save()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public void Commit()
        {
            _objTran?.Commit();
        }

        public void Rollback()
        {
            _objTran?.Rollback();
            _objTran?.Dispose();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public void ClearChangeTracker()
        {
            _context.ChangeTracker.Clear();
        }
    }
}
