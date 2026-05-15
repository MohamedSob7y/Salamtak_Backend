using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Shared.DTOs.MedicalReports;
using Salamtak.Shared.DTOs.Patients;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IPatientService
    {
        Task<ApiResponse<PatientProfileDto>> GetProfileAsync(Guid patientId);

        Task<ApiResponse<PatientProfileDto>> UpdateProfileAsync(Guid patientId, UpdatePatientProfileDto dto);

        Task<ApiResponse<IReadOnlyList<PatientAppointmentDto>>> GetAppointmentsAsync(Guid patientId);

        Task<ApiResponse<MedicalReportDto>> GetMedicalHistoryAsync(Guid patientId);
    }
}
