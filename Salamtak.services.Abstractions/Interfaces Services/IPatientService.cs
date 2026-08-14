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
        Task<ApiResponse<PatientProfileDto>>
            GetProfileAsync(
                Guid patientUserId);

        Task<ApiResponse<PatientProfileDto>>
            UpdateProfileAsync(
                Guid patientUserId,
                UpdatePatientProfileDto dto);

        Task<ApiResponse<IReadOnlyList<PatientAppointmentDto>>>
            GetAppointmentsAsync(
                Guid patientUserId);

        Task<ApiResponse<MedicalReportDto>>
            GetMedicalHistoryAsync(
                Guid patientUserId);
    }
}
