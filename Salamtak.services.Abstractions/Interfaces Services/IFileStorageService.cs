using Microsoft.AspNetCore.Http;
using Salamtak.Shared.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IFileStorageService
    {
      
        Task<StoredFileResult> UploadAsync(IFormFile file,string folderName,CancellationToken cancellationToken = default);

       
        Task DeleteAsync(string relativePath,CancellationToken cancellationToken = default);

        
        Task<Stream> OpenReadAsync(string relativePath,CancellationToken cancellationToken = default);

       
        bool Exists(string relativePath);
    }
}
