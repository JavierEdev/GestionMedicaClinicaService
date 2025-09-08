using GestionClinica.Common;
using GestionClinica.Domain.DTOs;
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
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CitaVm>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CitaVm>>>> ListarTodas()
    {
        var data = await _svc.ListarTodasAsync();
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

}
