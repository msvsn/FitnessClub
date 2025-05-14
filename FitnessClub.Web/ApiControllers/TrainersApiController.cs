using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessClub.Web.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainersApiController : ControllerBase
    {
        private readonly ITrainerService _trainerService;

        public TrainersApiController(ITrainerService trainerService)
        {
            _trainerService = trainerService;
        }

        // GET: api/TrainersApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainerDto>>> GetTrainers([FromQuery] int? clubId)
        {
            if (clubId.HasValue)
            {
                var trainersByClub = await _trainerService.GetTrainersByClubAsync(clubId.Value);
                return Ok(trainersByClub);
            }
            var trainers = await _trainerService.GetAllTrainersAsync();
            return Ok(trainers);
        }

        // GET: api/TrainersApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TrainerDto>> GetTrainer(int id)
        {
            var trainer = await _trainerService.GetTrainerByIdAsync(id);

            if (trainer == null)
            {
                return NotFound($"Тренера з ID {id} не знайдено.");
            }

            return Ok(trainer);
        }

        // POST: api/TrainersApi
        [HttpPost]
        public async Task<ActionResult<TrainerDto>> PostTrainer([FromBody] TrainerDto trainerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdTrainer = await _trainerService.CreateTrainerAsync(trainerDto);
            if (createdTrainer == null || createdTrainer.TrainerId <= 0) 
            {
                 return StatusCode(500, "Помилка при створенні тренера в сервісі.");
            }
            return CreatedAtAction(nameof(GetTrainer), new { id = createdTrainer.TrainerId }, createdTrainer);
        }

        // PUT: api/TrainersApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTrainer(int id, [FromBody] TrainerDto trainerDto)
        {
            if (id != trainerDto.TrainerId)
            {
                return BadRequest("ID в URL має співпадати з ID в тілі запиту.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var success = await _trainerService.UpdateTrainerAsync(id, trainerDto);
            if (!success)
            {
                return NotFound($"Тренера з ID {id} не знайдено або оновлення не вдалося.");
            }
            return NoContent();
        }

        // DELETE: api/TrainersApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrainer(int id)
        {
            var success = await _trainerService.DeleteTrainerAsync(id);
            if (!success)
            {
                return NotFound($"Тренера з ID {id} не знайдено або видалення не вдалося.");
            }
            return NoContent();
        }
    }
} 