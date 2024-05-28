using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Intergration.Constracts
{
    public interface ITransferApiClient
    {
        string Api { get; set; }
        void UpdateApiAddress(string newApiAddress);
        Task<bool> TransferToPortal(string filePath);
    }
}
