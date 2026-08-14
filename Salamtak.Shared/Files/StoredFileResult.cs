using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Shared.Files
{
    //This DTo لما بخزن فايل وعايز اوصله دا اللى بيرجع لما اشوفه يعنى الداتا بتاعته 
    public class StoredFileResult
    {
       //name of File With user
        public string OriginalFileName { get; set; } = string.Empty;

       //name of File اللى حفظه الserver
        public string StoredFileName { get; set; } = string.Empty;

        //Path of File اللى حفظه الserver
        public string RelativePath { get; set; } = string.Empty;

        //type of File اللى حفظه الserver
        public string ContentType { get; set; } = string.Empty;

        
        public long FileSize { get; set; }
    }
}
