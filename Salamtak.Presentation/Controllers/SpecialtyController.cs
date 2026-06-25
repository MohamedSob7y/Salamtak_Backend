using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Specialties;

namespace Salamtak.Web.Api.Controllers
{
    [Route("api/[controller]")]
    public class SpecialtyController : BaseApiController
    {
        private readonly ISpecialtyService _specialtyService;

        public SpecialtyController(ISpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var response = await _specialtyService.GetAllAsync();
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _specialtyService.GetByIdAsync(id);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSpecialtyDto dto)
        {
            var response = await _specialtyService.CreateAsync(dto);
            return Ok(response);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] UpdateSpecialtyDto dto)
        {
            var response = await _specialtyService.UpdateAsync(dto);
            return Ok(response);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _specialtyService.DeleteAsync(id);
            return Ok(response);
        }
    }
}