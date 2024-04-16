using ScanApp.Data.Entities;
using ScanApp.Data.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Data.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ScanContext _context;

        public UnitOfWork ()
        {
            _context = new ScanContext ();
        }

        public virtual async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
