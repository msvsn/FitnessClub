using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessClub.Web.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassSchedulesApiController : ControllerBase
    {
        private readonly IClassScheduleService _classScheduleService;

        public ClassSchedulesApiController(IClassScheduleService classScheduleService)
        {
            _classScheduleService = classScheduleService;
        }

        // GET: api/ClassSchedulesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClassScheduleDto>>> GetClassSchedules([FromQuery] int? clubId, [FromQuery] DateTime? date)
        {
            if (clubId.HasValue && date.HasValue)
            {
                var schedules = await _classScheduleService.GetSchedulesByClubAndDateAsync(clubId.Value, date.Value);
                return Ok(schedules);
            }
            var allSchedules = await _classScheduleService.GetAllClassSchedulesAsync();
            return Ok(allSchedules);
        }

        // GET: api/ClassSchedulesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClassScheduleDto>> GetClassSchedule(int id)
        {
            var classSchedule = await _classScheduleService.GetClassScheduleByIdAsync(id);

            if (classSchedule == null)
            {
                return NotFound($"Розклад з ID {id} не знайдено.");
            }

            return Ok(classSchedule);
        }

        // POST: api/ClassSchedulesApi
        [HttpPost]
        public async Task<ActionResult<ClassScheduleDto>> PostClassSchedule([FromBody] ClassScheduleDto classScheduleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdSchedule = await _classScheduleService.CreateClassScheduleAsync(classScheduleDto);
            if (createdSchedule == null || createdSchedule.ClassScheduleId <= 0) 
            {
                return StatusCode(500, "Помилка при створенні розкладу в сервісі.");
            }
            return CreatedAtAction(nameof(GetClassSchedule), new { id = createdSchedule.ClassScheduleId }, createdSchedule);
        }

        // PUT: api/ClassSchedulesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClassSchedule(int id, [FromBody] ClassScheduleDto classScheduleDto)
        {
            if (id != classScheduleDto.ClassScheduleId)
            {
                return BadRequest("ID в URL має співпадати з ID в тілі запиту.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var success = await _classScheduleService.UpdateClassScheduleAsync(id, classScheduleDto);
            if (!success)
            {
                return NotFound($"Розклад з ID {id} не знайдено або оновлення не вдалося.");
            }
            return NoContent();
        }

        // DELETE: api/ClassSchedulesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClassSchedule(int id)
        {
            var success = await _classScheduleService.DeleteClassScheduleAsync(id);
            if (!success)
            {   
                return NotFound($"Розклад з ID {id} не знайдено або видалення не вдалося.");
            }
            return NoContent();
        }
    }
} 