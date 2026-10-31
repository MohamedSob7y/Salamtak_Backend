using Salamtak.Domain.Models;

namespace Salamtak.Domain.Specifications.Appointments
{
    public class DoctorAppointmentsCountSpecification
        : BaseSpecification<Appointment>
    {
        public DoctorAppointmentsCountSpecification(Guid doctorId)
            : base(a => a.DoctorId == doctorId)
        {
        }
    }
}