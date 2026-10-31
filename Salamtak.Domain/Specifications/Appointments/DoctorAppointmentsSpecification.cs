using Salamtak.Domain.Models;

namespace Salamtak.Domain.Specifications.Appointments
{
    public class DoctorAppointmentsSpecification
        : BaseSpecification<Appointment>
    {
        public DoctorAppointmentsSpecification(
            Guid doctorId,
            int pageNumber,
            int pageSize)
            : base(a => a.DoctorId == doctorId)
        {
            AddInclude(a => a.Patient);
            AddInclude(a => a.AvailabilitySlot);

            AddOrderByDescending(a => a.CreatedAt);

            ApplyPaging(
                (pageNumber - 1) * pageSize,
                pageSize);
        }
    }
}