namespace Salamtak.Domain.Models.Enums
{
    public enum NotificationType
    {
        General = 0,

        AppointmentBooked = 1,
        AppointmentCancelled = 2,
        AppointmentCompleted = 3,
        AppointmentRescheduled = 4,

        DoctorProfileApproved = 5,
        DoctorProfileRejected = 6,

        PatientProfileApproved = 7,
        PatientProfileRejected = 8,

        MedicalReportAdded = 9,
        PrescriptionAdded = 10,

        AppointmentConfirmed = 11,
        AppointmentNoShow = 12
    }
}