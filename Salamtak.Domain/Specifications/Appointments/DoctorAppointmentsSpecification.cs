using Salamtak.Domain.Models;
using Salamtak.Domain.Specifications;

namespace Salamtak.Domain.Specifications.Appointments
{
    public class DoctorAppointmentsSpecification
        : BaseSpecification<Appointment>
    {
        public DoctorAppointmentsSpecification(
            Guid doctorId,
            int pageNumber,
            int pageSize)
            : base(a =>
                a.DoctorId == doctorId &&
                !a.IsDeleted)
        {
            AddInclude(a => a.Patient);
            AddInclude(a => a.Doctor);
            AddInclude(a => a.Clinic);
            AddInclude(a => a.AvailabilitySlot);

            AddOrderByDescending(a => a.CreatedAt);

            ApplyPaging(
                (pageNumber - 1) * pageSize,
                pageSize);
        }
    }
}