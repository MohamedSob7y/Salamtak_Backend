using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Abstractions.Interfaces_Services
{
    public interface IAppointmentService
    {
        Task<ApiResponse> ConfirmAppointmentAsync(Guid doctorUserId,Guid appointmentId);

        Task<ApiResponse> MarkAsNoShowAsync(Guid doctorUserId,Guid appointmentId);
        Task<ApiResponse<AppointmentDto>>BookAppointmentAsync(Guid patientUserId,BookAppointmentDto dto);

        Task<ApiResponse>CancelAppointmentAsync(Guid currentUserId,CancelAppointmentDto dto);

        Task<ApiResponse>CompleteAppointmentAsync(Guid doctorUserId,CompleteAppointmentDto dto);

        Task<ApiResponse<AppointmentDetailsDto>>GetByIdAsync(Guid currentUserId,Guid appointmentId);

        Task<ApiResponse<IReadOnlyList<PatientAppointmentDto>>>GetPatientAppointmentsAsync(Guid patientUserId);

        Task<ApiResponse<IReadOnlyList<DoctorAppointmentDto>>>GetDoctorAppointmentsAsync(Guid doctorUserId);
        Task<int> CancelExpiredPendingAppointmentsAsync();
    }
}