using Microsoft.AspNetCore.Http;
using ScanApp.Intergration.Constracts;
using ScanApp.Model.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Intergration.ApiClients
{
    public class TransferApiClient : BaseApiClient, ITransferApiClient
    {
        private readonly string _apiAddress = "http://192.168.1.15:4003/";
        //private readonly string _apiAddress = "http://idcag.librasoft.vn/";

        public TransferApiClient() : base() { }

        public async Task<bool> TransferToPortal(string filePath)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiAddress);

                    var fileContent = new StreamContent(File.OpenRead(filePath));
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    var formData = new MultipartFormDataContent();
                    formData.Add(fileContent, "pdfs", Path.GetFileName(filePath));
                    var response = await httpClient.PostAsync("api/upload-documents", formData);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"HTTP request failed: {ex.Message}");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
