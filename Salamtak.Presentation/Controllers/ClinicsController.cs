using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.Constants;
using Salamtak.Shared.DTOs.Clinics;
using Salamtak.Web.Api.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Presentation.Controllers
{
    [Route("api/clinics")]
    [Authorize]
    public class ClinicsController : BaseApiController
    {
        private readonly IClinicService _clinicService;

        public ClinicsController(
            IClinicService clinicService)
        {
            _clinicService = clinicService;
        }

        [HttpGet("doctor/{doctorId:guid}")]
        [Authorize(Roles = Roles.Patient + "," + Roles.Doctor)]
        public async Task<IActionResult> GetDoctorClinics(
            Guid doctorId)
        {
            var response = await _clinicService
                .GetDoctorClinicsAsync(doctorId);

            return Ok(response);
        }


        [HttpGet("me")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> GetMyClinics()
        {
            var doctorUserId = GetCurrentUserId();

            var response = await _clinicService
                .GetMyClinicsAsync(doctorUserId);

            return Ok(response);
        }


        [HttpGet("{clinicId:guid}")]
        [Authorize(Roles = Roles.Patient + "," + Roles.Doctor)]
        public async Task<IActionResult> GetById(
            Guid clinicId)
        {
            var response = await _clinicService
                .GetByIdAsync(clinicId);

            return Ok(response);
        }


        [HttpPost]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> Create(
            [FromBody] CreateClinicDto dto)
        {
            var doctorUserId = GetCurrentUserId();

            var response = await _clinicService
                .CreateAsync(
                    doctorUserId,
                    dto);

            return Ok(response);
        }

        [HttpPost("{clinicId:guid}/join")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> JoinClinic(
            Guid clinicId)
        {
            var doctorUserId = GetCurrentUserId();

            var response = await _clinicService
                .JoinClinicAsync(
                    doctorUserId,
                    clinicId);

            return Ok(response);
        }


        [HttpDelete("{clinicId:guid}/leave")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> LeaveClinic(
            Guid clinicId)
        {
            var doctorUserId = GetCurrentUserId();

            var response = await _clinicService
                .LeaveClinicAsync(
                    doctorUserId,
                    clinicId);

            return Ok(response);
        }
    }
}
