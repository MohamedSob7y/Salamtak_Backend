using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IAppointmentService
    {
        Task<ApiResponse<AppointmentDto>> BookAppointmentAsync(Guid patientId, BookAppointmentDto dto);

        Task<ApiResponse> CancelAppointmentAsync(Guid userId, CancelAppointmentDto dto);

        Task<ApiResponse> CompleteAppointmentAsync(Guid doctorId, CompleteAppointmentDto dto);

        Task<ApiResponse<AppointmentDetailsDto>> GetByIdAsync(Guid appointmentId);

        Task<ApiResponse<IReadOnlyList<PatientAppointmentDto>>> GetPatientAppointmentsAsync(Guid patientId);

        Task<ApiResponse<IReadOnlyList<DoctorAppointmentDto>>> GetDoctorAppointmentsAsync(Guid doctorId);
    }
}
