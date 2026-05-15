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
        Task<ApiResponse<MedicalReportDto>> GetPatientReportAsync(Guid patientId);

        Task<ApiResponse<MedicalReportDto>> GetPatientReportForDoctorAsync(Guid doctorId, Guid patientId, Guid appointmentId);

        Task<ApiResponse<MedicalReportEntryDto>> AddEntryAsync(Guid doctorId, CreateMedicalReportEntryDto dto);

        Task<ApiResponse<MedicalReportEntryDto>> UpdateEntryAsync(Guid doctorId, UpdateMedicalReportEntryDto dto);
    }
}
