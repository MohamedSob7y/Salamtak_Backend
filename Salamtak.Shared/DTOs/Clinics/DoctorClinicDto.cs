using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Shared.DTOs.Clinics
{
    public class DoctorClinicDto
    {
        public Guid DoctorClinicId { get; set; }

        public Guid DoctorId { get; set; }

        public Guid ClinicId { get; set; }

        public string ClinicName { get; set; } = string.Empty;

        public string ClinicAddress { get; set; } = string.Empty;

        public string ClinicCity { get; set; } = string.Empty;

        public string? ClinicPhoneNumber { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}
