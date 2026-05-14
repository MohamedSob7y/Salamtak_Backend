namespace Salamtak.Shared.DTOs.Admin;

public class AdminDashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int PendingDoctorVerifications { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
}
