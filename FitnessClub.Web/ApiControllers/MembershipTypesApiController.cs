using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessClub.Web.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipTypesApiController : ControllerBase
    {
        private readonly IMembershipService _membershipService;

        public MembershipTypesApiController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        // GET: api/MembershipTypesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MembershipTypeDto>>> GetMembershipTypes()
        {
            var membershipTypes = await _membershipService.GetAllMembershipTypesAsync();
            return Ok(membershipTypes);
        }

        // GET: api/MembershipTypesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MembershipTypeDto>> GetMembershipType(int id)
        {
            var membershipType = await _membershipService.GetMembershipTypeByIdAsync(id);
            if (membershipType == null)
            {
                return NotFound($"Тип абонементу з ID {id} не знайдено.");
            }
            return Ok(membershipType);
        }

        // POST: api/MembershipTypesApi
        [HttpPost]
        public async Task<ActionResult<MembershipTypeDto>> PostMembershipType([FromBody] MembershipTypeDto membershipTypeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdType = await _membershipService.CreateMembershipTypeAsync(membershipTypeDto);
            if (createdType == null || createdType.MembershipTypeId <= 0)
            {
                return StatusCode(500, "Помилка при створенні типу абонементу в сервісі.");
            }
            return CreatedAtAction(nameof(GetMembershipType), new { id = createdType.MembershipTypeId }, createdType);
        }

        // PUT: api/MembershipTypesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMembershipType(int id, [FromBody] MembershipTypeDto membershipTypeDto)
        {
            if (id != membershipTypeDto.MembershipTypeId)
            {
                return BadRequest("ID в URL має співпадати з ID в тілі запиту.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var success = await _membershipService.UpdateMembershipTypeAsync(id, membershipTypeDto);
            if (!success)
            {
                return NotFound($"Тип абонементу з ID {id} не знайдено або оновлення не вдалося.");
            }
            return NoContent();
        }

        // DELETE: api/MembershipTypesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMembershipType(int id)
        {
            var success = await _membershipService.DeleteMembershipTypeAsync(id);
            if (!success)
            {
                return NotFound($"Тип абонементу з ID {id} не знайдено або видалення не вдалося.");
            }
            return NoContent();
        }
    }
} 