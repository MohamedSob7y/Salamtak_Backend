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

        Task<ApiResponse<AppointmentDto>> BookAppointmentAsync(
            Guid patientUserId,
            BookAppointmentDto dto);

        Task<ApiResponse> CancelAppointmentAsync(
            Guid currentUserId,
            CancelAppointmentDto dto);

        Task<ApiResponse> CompleteAppointmentAsync(
            Guid doctorUserId,
            CompleteAppointmentDto dto);

        Task<ApiResponse<AppointmentDetailsDto>> GetByIdAsync(
            Guid currentUserId,
            Guid appointmentId);

        Task<ApiResponse<IReadOnlyList<PatientAppointmentDto>>>
            GetPatientAppointmentsAsync(Guid patientUserId);

        Task<ApiResponse<IReadOnlyList<DoctorAppointmentDto>>>
            GetDoctorAppointmentsAsync(Guid doctorUserId);
    }
}
