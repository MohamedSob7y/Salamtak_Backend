using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Implementation_Of_Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public LocalFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<StoredFileResult> UploadAsync(IFormFile file,string folderName,CancellationToken cancellationToken = default)
        {
            if (file is null || file.Length == 0)
            {
                throw new ArgumentException(
                    "File is required.");
            }

            if (string.IsNullOrWhiteSpace(folderName))
            {
                throw new ArgumentException(
                    "Folder name is required.");
            }

            var extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentException(
                    "File extension is required.");
            }

            var storedFileName =
                $"{Guid.NewGuid()}{extension}";

            var webRootPath =
                _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot");
            }

            var normalizedFolderName =
                folderName
                    .Replace("/", Path.DirectorySeparatorChar.ToString())
                    .Replace("\\", Path.DirectorySeparatorChar.ToString())
                    .Trim(
                        Path.DirectorySeparatorChar);

            var physicalFolderPath =
                Path.Combine(
                    webRootPath,
                    normalizedFolderName);

            if (!Directory.Exists(physicalFolderPath))
            {
                Directory.CreateDirectory(
                    physicalFolderPath);
            }

            var physicalFilePath =
                Path.Combine(
                    physicalFolderPath,
                    storedFileName);

            await using var stream =
                new FileStream(
                    physicalFilePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);

            await file.CopyToAsync(
                stream,
                cancellationToken);

            var relativePath =
                Path.Combine(
                    normalizedFolderName,
                    storedFileName)
                .Replace(
                    Path.DirectorySeparatorChar,
                    '/');

            return new StoredFileResult
            {
                OriginalFileName = file.FileName,
                StoredFileName = storedFileName,
                RelativePath = relativePath,
                ContentType = file.ContentType,
                FileSize = file.Length
            };
        }

        public Task DeleteAsync(string relativePath,CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return Task.CompletedTask;
            }

            var physicalPath =
                GetPhysicalPath(relativePath);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            return Task.CompletedTask;
        }

        public Task<Stream> OpenReadAsync(string relativePath,CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException(
                    "File path is required.");
            }

            var physicalPath =
                GetPhysicalPath(relativePath);

            if (!File.Exists(physicalPath))
            {
                throw new FileNotFoundException(
                    "File was not found.",
                    relativePath);
            }

            Stream stream =
                new FileStream(
                    physicalPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 81920,
                    useAsync: true);

            return Task.FromResult(stream);
        }

        public bool Exists(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return false;
            }

            var physicalPath =
                GetPhysicalPath(relativePath);

            return File.Exists(physicalPath);
        }

        private string GetPhysicalPath(string relativePath)
        {
            var webRootPath =
                _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot");
            }

            var normalizedRelativePath =
                relativePath
                    .Replace("/", Path.DirectorySeparatorChar.ToString())
                    .Replace("\\", Path.DirectorySeparatorChar.ToString())
                    .TrimStart(
                        Path.DirectorySeparatorChar);

            var fullPath =
                Path.GetFullPath(
                    Path.Combine(
                        webRootPath,
                        normalizedRelativePath));

            var fullWebRootPath =
                Path.GetFullPath(webRootPath);

            if (!fullPath.StartsWith(
                fullWebRootPath,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException(
                    "Invalid file path.");
            }

            return fullPath;
        }
    }
}
