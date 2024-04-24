using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Common.Common
{
    public class FileConverter
    {
        public static IFormFile ConvertToIFormFile(string filePath)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                string fileName = Path.GetFileName(filePath);
                string contentType = GetContentType(fileName);

                return new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, null, fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
                };
            }
            catch (Exception)
            {
                throw;
            }

        }

        private static string GetContentType(string fileName)
        {
            try
            {
                if (Path.GetExtension(fileName).ToLowerInvariant() == ".pdf")
                    return "application/pdf";
                else
                    return "application/octet-stream";
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
