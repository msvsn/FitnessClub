using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessClub.Web.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClubsApiController : ControllerBase
    {
        private readonly IClubService _clubService;

        public ClubsApiController(IClubService clubService)
        {
            _clubService = clubService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClubDto>>> GetClubs()
        {
            var clubs = await _clubService.GetAllClubsAsync();
            return Ok(clubs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClubDto>> GetClub(int id)
        {
            var club = await _clubService.GetClubByIdAsync(id);
            if (club == null)
            {
                return NotFound($"Клуб з ID {id} не знайдено.");
            }
            return Ok(club);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutClub(int id, [FromBody] ClubDto clubDto)
        {
            if (id != clubDto.ClubId)
            {
                return BadRequest("ID в URL має співпадати з ID в тілі запиту.");
            }
            var success = await _clubService.UpdateClubAsync(id, clubDto);
            if (!success)
            {
                return NotFound($"Клуб з ID {id} не знайдено або оновлення не вдалося.");
            }
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<ClubDto>> PostClub([FromBody] ClubDto clubDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdClub = await _clubService.CreateClubAsync(clubDto);
            return CreatedAtAction(nameof(GetClub), new { id = createdClub.ClubId }, createdClub);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClub(int id)
        {
            var clubToDelete = await _clubService.GetClubByIdAsync(id);
            if (clubToDelete == null)
            {
                return NotFound($"Club with ID {id} not found.");
            }
            var success = await _clubService.DeleteClubAsync(id);
            if (!success)
            {
                return StatusCode(500, "Сталася помилка під час видалення клубу.");
            }
            return NoContent();
        }
    }
} 