
using Salamtak.Domain.Models;

namespace Salamtak.Domain.Specifications.Appointments
{
    public class PatientAppointmentsCountSpecification
        : BaseSpecification<Appointment>
    {
        public PatientAppointmentsCountSpecification(Guid patientId)
            : base(a => a.PatientId == patientId)
        {
        }
    }
}
