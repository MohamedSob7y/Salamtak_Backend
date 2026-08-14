using Salamtak.Shared.DTOs.MedicalReports;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IMedicalReportService
    {
        Task<ApiResponse<MedicalReportDto>> GetMyReportAsync(
          Guid patientUserId);

        Task<ApiResponse<MedicalReportDto>> GetPatientReportForDoctorAsync(
            Guid doctorUserId,
            Guid appointmentId);

        Task<ApiResponse<MedicalReportEntryDto>> AddEntryAsync(
            Guid doctorUserId,
            CreateMedicalReportEntryDto dto);

        Task<ApiResponse<MedicalReportEntryDto>> UpdateEntryAsync(
            Guid doctorUserId,
            UpdateMedicalReportEntryDto dto);
    }
}
