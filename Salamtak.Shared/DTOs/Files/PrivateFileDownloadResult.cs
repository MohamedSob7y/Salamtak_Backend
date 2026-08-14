using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Shared.DTOs.Files
{
    public class PrivateFileDownloadResult
    {
        public Stream Stream { get; set; } = null!;

        public string ContentType { get; set; } ="application/octet-stream";

        public string FileName { get; set; } = null!;
    }
}
