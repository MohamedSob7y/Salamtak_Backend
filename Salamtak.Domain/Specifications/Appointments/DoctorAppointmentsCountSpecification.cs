using Salamtak.Domain.Models;
using Salamtak.Domain.Specifications;

namespace Salamtak.Domain.Specifications.Appointments
{
    public class DoctorAppointmentsCountSpecification
        : BaseSpecification<Appointment>
    {
        public DoctorAppointmentsCountSpecification(
            Guid doctorId)
            : base(a =>
                a.DoctorId == doctorId &&
                !a.IsDeleted)
        {
        }
    }
}