using Salamtak.Domain.Models.Common_Entity;
using Salamtak.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Domain.Models
{
    public class User : BaseEntity
    {
        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public UserRole Role { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Active;

        // Navigation Properties
        public Patient? Patient { get; set; }

        public Doctor? Doctor { get; set; }

        public Admin? Admin { get; set; }

        public ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
    }
}
