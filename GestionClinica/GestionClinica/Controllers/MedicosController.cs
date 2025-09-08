using GestionClinica.Common;
using GestionClinica.Domain.DTOs;
using GestionClinica.Domain.Factories;
using GestionClinica.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionClinica.Controllers;

[ApiController]
[Route("api/medicos")]
public class MedicosController : ControllerBase
{
    private readonly IMedicoService _svc;
    public MedicosController(IClinicaModuleFactory f) => _svc = f.CreateMedicoService();

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<object>>> Crear([FromBody] MedicoCreateDto dto)
    {
        try
        {
            var id = await _svc.RegistrarAsync(dto);
            return Ok(ApiResponses.Ok<object>(new { idMedico = id }, "Médico registrado exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponses.Fail<object>(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MedicoListVm>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MedicoListVm>>>> Listar()
    {
        var data = await _svc.ListarAsync();
        return Ok(ApiResponses.Ok(data, "Listado de médicos"));
    }

    [HttpGet("{id}/disponibilidad")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DiaDisponibilidadVm>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<IEnumerable<DiaDisponibilidadVm>>>> Disponibilidad(
        int id, [FromQuery] DateTime fecha)
    {
        try
        {
            var data = await _svc.DisponibilidadPorRangoAsync(id, fecha);
            return Ok(ApiResponses.Ok(data, "Disponibilidad del día"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponses.Fail<IEnumerable<DiaDisponibilidadVm>>(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MedicoDetailVm>>> Obtener(int id)
    {
        var vm = await _svc.ObtenerMedicoAsync(id);
        return vm is null
            ? NotFound(ApiResponses.Fail<MedicoDetailVm>("Médico no encontrado"))
            : Ok(ApiResponses.Ok(vm, "Detalle del médico"));
    }

    [HttpGet("por-especialidad")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MedicoListVm>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<MedicoListVm>>>> PorEspecialidad([FromQuery] string especialidad)
    {
        var data = (await _svc.MedicosPorEspecialidadAsync(especialidad))
                   .Select(m => new MedicoListVm(m.Id, m.NombreCompleto, m.Especialidad));
        return Ok(ApiResponses.Ok(data, "Médicos por especialidad"));
    }

    [HttpGet("productividad")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductividadMedicaDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductividadMedicaDto>>>> Productividad(
        [FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        var data = await _svc.ReporteProductividadAsync(desde, hasta);
        var totalConsultas = data.Sum(x => x.Consultas);
        var totalProcedimientos = data.Sum(x => x.Procedimientos);
        var msg = $"Productividad del {desde:yyyy-MM-dd} al {hasta:yyyy-MM-dd}. Total consultas: {totalConsultas}, procedimientos: {totalProcedimientos}.";
        return Ok(ApiResponses.Ok(data, msg));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<MedicoDetailVm>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<MedicoDetailVm>>> Actualizar(int id, [FromBody] MedicoUpdateDto dto)
    {
        try
        {
            var vm = await _svc.ActualizarAsync(id, dto);
            return Ok(ApiResponses.Ok(vm, "Médico actualizado correctamente."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponses.Fail<MedicoDetailVm>(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<object>>> Eliminar(int id)
    {
        try
        {
            await _svc.EliminarAsync(id);
            return Ok(ApiResponses.Ok<object>(new { id }, "Médico eliminado."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponses.Fail<object>(ex.Message));
        }
    }
}
