using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Shared.DTOs.Profile
{
    public class ProfileImageDto
    {
        public Guid UserId { get; set; }

       
        public string ProfileImagePath { get; set; } = string.Empty;

       
        public string ProfileImageUrl { get; set; } = string.Empty;
    }
}
