using Microsoft.EntityFrameworkCore;
using Salamtak.Domain.Contracts;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Common_Entity;
using Salamtak.Persistance.Context;
using System.Text.Json;

namespace Salamtak.Persistance.DataSeeding
{
    public class DataSeeding : IDataSeeding
    {
        private readonly SalamtakDBContext _salamtakDBContext;

        public DataSeeding(SalamtakDBContext salamtakDBContext)
        {
            _salamtakDBContext = salamtakDBContext;
        }

        public async Task IntializeAsync()
        {
            try
            {
               

                await SeedDataFromJson<Specialty>(
                    "specialties.json",
                    _salamtakDBContext.Specialties);

                await _salamtakDBContext.SaveChangesAsync();

              
                await SeedDataFromJson<User>(
                    "users.json",
                    _salamtakDBContext.Users);

                await _salamtakDBContext.SaveChangesAsync();


                await SeedDataFromJson<Admin>(
                    "admins.json",
                    _salamtakDBContext.Admins);

                await SeedDataFromJson<Doctor>(
                    "doctors.json",
                    _salamtakDBContext.Doctors);

                await SeedDataFromJson<Patient>(
                    "patients.json",
                    _salamtakDBContext.Patients);

                await _salamtakDBContext.SaveChangesAsync();

               
                await SeedDataFromJson<DoctorDocument>(
                    "doctorDocuments.json",
                    _salamtakDBContext.DoctorDocuments);

                await _salamtakDBContext.SaveChangesAsync();

                
                await SeedDataFromJson<Clinic>(
                    "clinics.json",
                    _salamtakDBContext.Clinics);

                await _salamtakDBContext.SaveChangesAsync();


                await SeedDataFromJson<DoctorClinic>(
                    "doctorClinics.json",
                    _salamtakDBContext.DoctorClinics);

                await _salamtakDBContext.SaveChangesAsync();

               
                await SeedDataFromJson<AvailabilitySlot>(
                    "availabilitySlots.json",
                    _salamtakDBContext.AvailabilitySlots);

                await _salamtakDBContext.SaveChangesAsync();

               
                await SeedDataFromJson<Appointment>(
                    "appointments.json",
                    _salamtakDBContext.Appointments);

                await _salamtakDBContext.SaveChangesAsync();

               
                await SeedDataFromJson<MedicalReport>(
                    "medicalReports.json",
                    _salamtakDBContext.MedicalReports);

                await _salamtakDBContext.SaveChangesAsync();

                await SeedDataFromJson<MedicalReportEntry>(
                    "medicalReportEntries.json",
                    _salamtakDBContext.MedicalReportEntries);

                await _salamtakDBContext.SaveChangesAsync();

                await SeedDataFromJson<MedicalReportAttachment>(
                    "medicalReportAttachments.json",
                    _salamtakDBContext.MedicalReportAttachments);

                await _salamtakDBContext.SaveChangesAsync();

                await SeedDataFromJson<Prescription>(
                    "prescriptions.json",
                    _salamtakDBContext.Prescriptions);

                await _salamtakDBContext.SaveChangesAsync();

               
                await SeedDataFromJson<Feedback>(
                    "feedbacks.json",
                    _salamtakDBContext.Feedbacks);

                await _salamtakDBContext.SaveChangesAsync();

                
                await SeedDataFromJson<Notification>(
                    "notifications.json",
                    _salamtakDBContext.Notifications);

                await _salamtakDBContext.SaveChangesAsync();

                Console.WriteLine(
                    "Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "An error occurred during database seeding.");

                Console.WriteLine(
                    ex.ToString());

                throw;
            }
        }

        private async Task SeedDataFromJson<T>(string filename,DbSet<T> dbSet)where T : BaseEntity
        {
            var filePath =
                GetJsonFilePath(filename);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"JSON file '{filename}' was not found.",
                    filePath);
            }

            try
            {
                await using var dataStream =
                    File.OpenRead(filePath);

                var jsonOptions =
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                var data =
                    await JsonSerializer
                        .DeserializeAsync<List<T>>(
                            dataStream,
                            jsonOptions);

                if (data is null ||
                    data.Count == 0)
                {
                    Console.WriteLine(
                        $"No records found in '{filename}'.");

                    return;
                }

               
                var existingIds =
                    await dbSet
                        .IgnoreQueryFilters()
                        .Select(entity =>
                            entity.Id)
                        .ToListAsync();

                var existingIdsSet =
                    existingIds.ToHashSet();

                var newData =
                    data
                        .Where(entity =>
                            !existingIdsSet.Contains(
                                entity.Id))
                        .ToList();

                if (newData.Count == 0)
                {
                    Console.WriteLine(
                        $"No new records to seed from '{filename}'.");

                    return;
                }

                
                await dbSet.AddRangeAsync(
                    newData);

                Console.WriteLine(
                    $"Loaded {newData.Count} new records from '{filename}'.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Invalid JSON format inside '{filename}'.",
                    ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error while reading seed file '{filename}'.",
                    ex);
            }
        }

        private static string GetJsonFilePath(string filename)
        {
            var possiblePaths = new[]
            {
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "..",
                    "Salamtak.Persistance",
                    "Json Files",
                    filename),

                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Salamtak.Persistance",
                    "Json Files",
                    filename),

                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Json Files",
                    filename),

                Path.Combine(
                    AppContext.BaseDirectory,
                    "Json Files",
                    filename),

                Path.Combine(
                    AppContext.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    "..",
                    "Salamtak.Persistance",
                    "Json Files",
                    filename)
            };

            foreach (var path in possiblePaths)
            {
                var fullPath =
                    Path.GetFullPath(path);

                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return Path.GetFullPath(
                possiblePaths[0]);
        }
    }
}