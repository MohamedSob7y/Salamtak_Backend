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
    public class PrivateFileStorageService
         : IPrivateFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        private readonly string _privateRootPath;

        public PrivateFileStorageService(
            IWebHostEnvironment environment)
        {
            _environment = environment;

            _privateRootPath = Path.Combine(
                _environment.ContentRootPath,
                "PrivateUploads");
        }

        public async Task<StoredFileResult> UploadAsync(
            IFormFile file,
            string folderName,
            CancellationToken cancellationToken = default)
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

            var extension = Path
                .GetExtension(file.FileName)
                .ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentException(
                    "File extension is required.");
            }

            var storedFileName =
                $"{Guid.NewGuid():N}{extension}";

            var normalizedFolderName =
                NormalizeRelativePath(folderName);

            var physicalFolderPath =
                GetSafePhysicalPath(normalizedFolderName);

            Directory.CreateDirectory(
                physicalFolderPath);

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
                OriginalFileName =
                    Path.GetFileName(file.FileName),

                StoredFileName =
                    storedFileName,

                RelativePath =
                    relativePath,

                ContentType =
                    file.ContentType,

                FileSize =
                    file.Length
            };
        }

        public Task<Stream> OpenReadAsync(
            string relativePath,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException(
                    "File path is required.");
            }

            var physicalPath =
                GetSafePhysicalPath(relativePath);

            if (!File.Exists(physicalPath))
            {
                throw new FileNotFoundException(
                    "Private file was not found.",
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

        public Task DeleteAsync(
            string relativePath,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return Task.CompletedTask;
            }

            var physicalPath =
                GetSafePhysicalPath(relativePath);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            return Task.CompletedTask;
        }

        public bool Exists(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return false;
            }

            var physicalPath =
                GetSafePhysicalPath(relativePath);

            return File.Exists(physicalPath);
        }

        private string GetSafePhysicalPath(
            string relativePath)
        {
            var normalizedRelativePath =
                NormalizeRelativePath(relativePath);

            var fullRootPath =
                Path.GetFullPath(
                    _privateRootPath);

            var fullFilePath =
                Path.GetFullPath(
                    Path.Combine(
                        fullRootPath,
                        normalizedRelativePath));

            var rootWithSeparator =
                fullRootPath.EndsWith(
                    Path.DirectorySeparatorChar)
                    ? fullRootPath
                    : fullRootPath +
                      Path.DirectorySeparatorChar;

            if (!fullFilePath.StartsWith(
                    rootWithSeparator,
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    fullFilePath,
                    fullRootPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException(
                    "Invalid private file path.");
            }

            return fullFilePath;
        }

        private static string NormalizeRelativePath(
            string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    "Path is required.");
            }

            return path
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar)
                .Replace(
                    '\\',
                    Path.DirectorySeparatorChar)
                .Trim(
                    Path.DirectorySeparatorChar);
        }
    }
}
