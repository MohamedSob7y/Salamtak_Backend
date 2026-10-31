
using Salamtak.Domain.Models;

namespace Salamtak.Domain.Specifications.Appointments
{
    public class PatientAppointmentsSpecification
        : BaseSpecification<Appointment>
    {
        public PatientAppointmentsSpecification(
            Guid patientId,
            int pageNumber,
            int pageSize)
            : base(a => a.PatientId == patientId)
        {
            AddInclude(a => a.Doctor);
            AddInclude(a => a.AvailabilitySlot);

            AddOrderByDescending(a => a.CreatedAt);

            ApplyPaging(
                (pageNumber - 1) * pageSize,
                pageSize);
        }
    }
}
