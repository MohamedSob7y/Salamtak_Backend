using AutoMapper;
using Salamtak.Domain.Models;
using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Shared.DTOs.AvailabilitySlots;
using Salamtak.Shared.DTOs.Clinics;
using Salamtak.Shared.DTOs.Specialties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            SpecialtyMapping();
            ClicicMapping();
            AvailabilitySlotMapping();
            AppionmentMapping();
        }
        private void SpecialtyMapping()
        {
            CreateMap<Specialty, SpecialtyDto>()
                 .ForMember(dest => dest.SpecialtyId, opt => opt.MapFrom(src => src.Id));

        }
        private void AvailabilitySlotMapping()
        {
            CreateMap<AvailabilitySlot, AvailabilitySlotDto>()
                .ForMember(dest => dest.SlotId, opt => opt.MapFrom(src => src.Id));
        }
        private void ClicicMapping()
        {

            CreateMap<Clinic, ClinicDto>()
                .ForMember(dest => dest.ClinicId, opt => opt.MapFrom(src => src.Id));
        }
        private void AppionmentMapping()
        {
            CreateMap<Appointment, AppointmentDto>()
                  .ForMember(dest => dest.Id,
                      opt => opt.MapFrom(src => src.Id))
                  .ForMember(dest => dest.AppointmentDate,
                      opt => opt.MapFrom(src =>
                          src.AvailabilitySlot != null
                              ? src.AvailabilitySlot.StartTime
                              : src.CreatedAt))
                  .ForMember(dest => dest.Status,
                      opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Appointment, AppointmentDetailsDto>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PatientName,
                    opt => opt.MapFrom(src =>
                        src.Patient != null && src.Patient.User != null
                            ? src.Patient.User.FullName
                            : string.Empty))
                .ForMember(dest => dest.DoctorName,
                    opt => opt.MapFrom(src =>
                        src.Doctor != null && src.Doctor.User != null
                            ? src.Doctor.User.FullName
                            : string.Empty))
                .ForMember(dest => dest.SpecialtyName,
                    opt => opt.MapFrom(src =>
                        src.Doctor != null && src.Doctor.Specialty != null
                            ? src.Doctor.Specialty.Name
                            : string.Empty))
                .ForMember(dest => dest.ClinicName,
                    opt => opt.MapFrom(src =>
                        src.Clinic != null ? src.Clinic.Name : string.Empty))
                .ForMember(dest => dest.ClinicAddress,
                    opt => opt.MapFrom(src =>
                        src.Clinic != null ? src.Clinic.Address : string.Empty))
                .ForMember(dest => dest.AppointmentDate,
                    opt => opt.MapFrom(src =>
                        src.AvailabilitySlot != null
                            ? src.AvailabilitySlot.StartTime
                            : src.CreatedAt))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Reason,
                    opt => opt.MapFrom(src => src.Reason ?? string.Empty));

            CreateMap<Appointment, PatientAppointmentDto>()
                .ForMember(dest => dest.AppointmentId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DoctorName,
                    opt => opt.MapFrom(src =>
                        src.Doctor != null && src.Doctor.User != null
                            ? src.Doctor.User.FullName
                            : string.Empty))
                .ForMember(dest => dest.SpecialtyName,
                    opt => opt.MapFrom(src =>
                        src.Doctor != null && src.Doctor.Specialty != null
                            ? src.Doctor.Specialty.Name
                            : string.Empty))
                .ForMember(dest => dest.AppointmentDate,
                    opt => opt.MapFrom(src =>
                        src.AvailabilitySlot != null
                            ? src.AvailabilitySlot.StartTime
                            : src.CreatedAt))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Appointment, DoctorAppointmentDto>()
                .ForMember(dest => dest.AppointmentId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PatientName,
                    opt => opt.MapFrom(src =>
                        src.Patient != null && src.Patient.User != null
                            ? src.Patient.User.FullName
                            : string.Empty))
                .ForMember(dest => dest.AppointmentDate,
                    opt => opt.MapFrom(src =>
                        src.AvailabilitySlot != null
                            ? src.AvailabilitySlot.StartTime
                            : src.CreatedAt))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Reason,
                    opt => opt.MapFrom(src => src.Reason ?? string.Empty));
        }
    }
}
