using GestionClinica.Common;
using GestionClinica.Domain.DTOs;
using GestionClinica.Domain.Entities;
using GestionClinica.Domain.Factories;
using GestionClinica.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionClinica.Controllers;

[ApiController]
[Route("api/citas")]
public class CitasController : ControllerBase
{
    private readonly ICitaService _svc;
    public CitasController(IClinicaModuleFactory f) => _svc = f.CreateCitaService();

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CitaCreatedVm>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<CitaCreatedVm>>> Agendar([FromBody] CitaCreateDto dto)
    {
        try
        {
            var data = await _svc.AgendarAsync(dto);
            return Ok(ApiResponses.Ok(data, "Cita creada; se envió correo de confirmación."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponses.Fail<CitaCreatedVm>(ex.Message));
        }
    }

    [HttpPost("{id}/cancelar")]
    [ProducesResponseType(typeof(ApiResponse<CitaCancelledVm>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<CitaCancelledVm>>> Cancelar(int id, [FromBody] CancelarDto dto)
    {
        try
        {
            var data = await _svc.CancelarAsync(id, dto.Razon);
            return Ok(ApiResponses.Ok(data, "Cita cancelada."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponses.Fail<CitaCancelledVm>(ex.Message));
        }
    }

    [HttpPost("{id}/reprogramar")]
    [ProducesResponseType(typeof(ApiResponse<CitaRescheduledVm>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<CitaRescheduledVm>>> Reprogramar(int id, [FromBody] ReprogramarDto dto)
    {
        try
        {
            var data = await _svc.ReprogramarAsync(id, dto.NuevaFecha, dto.Motivo);
            return Ok(ApiResponses.Ok(data, "Cita reprogramada exitosamente."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponses.Fail<CitaRescheduledVm>(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CitaListadoVm>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CitaListadoVm>>>> ListarTodas()
    {
        var data = await _svc.ListarTodasDetalladoAsync();
        return Ok(ApiResponses.Ok(data, "Listado general de citas."));
    }

    [HttpGet("medico/{idMedico}/calendario-medico")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CitaVm>>), 200)]
    public  async Task<ActionResult<ApiResponse<IEnumerable<CitaVm>>>> CalendarioMedicoProximos(int idMedico)
    {
        var data = await _svc.CitasMedico(idMedico);
        return Ok(ApiResponses.Ok(data, "Citas del medico"));
    }

    [HttpGet("medico/{idMedico}/dia")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CitaVm>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CitaVm>>>> CitasPorMedicoEnDia(
    int idMedico, [FromQuery] DateTime fecha)
    {
        var data = await _svc.CitasPorMedicoEnDiaAsync(idMedico, fecha);
        return Ok(ApiResponses.Ok(data, $"Citas del médico en {fecha:yyyy-MM-dd}"));
    }

    [HttpGet("paciente/{idPaciente}")]
    [ProducesResponseType(typeof(IEnumerable<CitaVm>), 200)]
    public Task<IEnumerable<CitaVm>> CitasPorPaciente(int idPaciente)
    => _svc.CitasPorPacienteAsync(idPaciente);

    [HttpGet("paciente/{idPaciente}/{idCita}")]
    [ProducesResponseType(typeof(CitaVm), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CitaDePaciente(int idPaciente, int idCita)
    {
        var vm = await _svc.ObtenerCitaDePacienteAsync(idPaciente, idCita);
        return vm is null ? NotFound() : Ok(vm);
    }

    [HttpPost("{id}/reasignar-medico")]
    public async Task<IActionResult> ReasignarMedico(int id, [FromBody] ReasignarMedicoDto dto)
    {
        try
        {
            await _svc.ReasignarMedicoAsync(id, dto.NuevoMedicoId);
            return Ok(new { success = true, message = "Médico reasignado" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno: " + ex.Message });
        }
    }


    [HttpGet("{id}/medicos-disponibles")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MedicoEspecialidadVm>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MedicoEspecialidadVm>>>> MedicosDisponibles(int id)
    {
        var data = await _svc.MedicosDisponiblesParaCitaAsync(id);
        return Ok(ApiResponses.Ok(data, "Médicos disponibles para la cita."));
    }

}
