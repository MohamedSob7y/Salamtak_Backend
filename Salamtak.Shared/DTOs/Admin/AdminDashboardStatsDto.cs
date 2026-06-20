namespace Salamtak.Shared.DTOs.Admin;

public class AdminDashboardStatsDto
{
    //public int TotalPatients { get; set; }

    //public int TotalDoctors { get; set; }

    //public int VerifiedDoctors { get; set; }

    //public int PendingDoctors { get; set; }

    //public int TotalAppointments { get; set; }

    //public int CompletedAppointments { get; set; }

    //public int CancelledAppointments { get; set; }
    public int TotalUsers { get; set; }

    public int TotalPatients { get; set; }

    public int TotalDoctors { get; set; }

    public int TotalAdmins { get; set; }

    public int VerifiedDoctors { get; set; }

    public int PendingDoctors { get; set; }

    public int RejectedDoctors { get; set; }

    public int TotalAppointments { get; set; }

    public int CompletedAppointments { get; set; }

    public int CancelledAppointments { get; set; }
}
