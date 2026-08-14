using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models.Enums
{
    public enum DoctorDocumentType
    {
        License = 1,//ترخيص المهنة زى كارنيه النقابة كدة 
        Certificate = 2,
        CV = 3,
        SyndicateCard = 4,//كارنيه نقابة الاطباء
        NationalId = 5,//بطاقة الرقم القومة للدكتور
        Other = 6
    }
}
