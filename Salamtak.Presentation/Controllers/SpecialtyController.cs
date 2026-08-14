using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.Constants;
using Salamtak.Shared.DTOs.Specialties;
using Salamtak.Web.Api.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialtyController : BaseApiController
    {
        private readonly ISpecialtyService _specialtyService;

        public SpecialtyController(
            ISpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var response =
                await _specialtyService.GetAllAsync();

            return Ok(response);
        }

        [HttpGet("{specialtyId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(
            Guid specialtyId)
        {
            var response =
                await _specialtyService.GetByIdAsync(
                    specialtyId);

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Create(
            [FromBody] CreateSpecialtyDto dto)
        {
            var response =
                await _specialtyService.CreateAsync(dto);

            return Ok(response);
        }

        [HttpPut("{specialtyId:guid}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Update(
            Guid specialtyId,
            [FromBody] UpdateSpecialtyDto dto)
        {
            dto.SpecialtyId = specialtyId;

            var response =
                await _specialtyService.UpdateAsync(dto);

            return Ok(response);
        }

        [HttpDelete("{specialtyId:guid}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(
            Guid specialtyId)
        {
            var response =
                await _specialtyService.DeleteAsync(
                    specialtyId);

            return Ok(response);
        }
    }
}
