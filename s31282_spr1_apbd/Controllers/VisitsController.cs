using Microsoft.AspNetCore.Mvc;
using s31282_spr1_apbd.Exceptions;
using s31282_spr1_apbd.Models;
using s31282_spr1_apbd.Services;

namespace s31282_spr1_apbd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitsController : ControllerBase
    {
        private readonly IVisitService _visitService;

        public VisitsController(IVisitService visitService)
        {
            _visitService = visitService;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetVisitById(int id)
        {
            try
            {
                var result = await _visitService.GetVisitByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                if(ex is EntityNotFoundException)
                {
                    return NotFound(ex.Message);
                }
                else
                {
                    return BadRequest(ex.Message);
                }
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> AddVisit([FromBody] AddVisitDTO dto)
        {
            try
            {
                await _visitService.AddVisitAsync(dto);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (EntityConflictException ex)
            {
                return Conflict(ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
