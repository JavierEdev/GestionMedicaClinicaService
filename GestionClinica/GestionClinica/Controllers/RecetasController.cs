using GestionClinica.Common;
using GestionClinica.Domain.DTOs;
using GestionClinica.Domain.Factories;
using GestionClinica.Domain.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/recetas")]
public class RecetasController : ControllerBase
{
    private readonly IRecetaService _svc;
    public RecetasController(IClinicaModuleFactory f) => _svc = f.CreateRecetaService();

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RecetaBatchCreatedVm>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<RecetaBatchCreatedVm>>> Crear([FromBody] RecetaCreateDto dto)
    {
        try
        {
            var ids = await _svc.GenerarAsync(dto);
            var payload = new RecetaBatchCreatedVm(dto.IdConsulta, ids.Count(), ids);
            return Ok(ApiResponses.Ok(payload, "Receta creada."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponses.Fail<RecetaBatchCreatedVm>(ex.Message));
        }
    }

    [HttpGet("{idReceta}")]
    [ProducesResponseType(typeof(ApiResponse<RecetaVm>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<RecetaVm>>> Obtener(int idReceta)
    {
        var vm = await _svc.ObtenerAsync(idReceta);
        return vm is null
            ? NotFound(ApiResponses.Fail<RecetaVm>("Receta no encontrada."))
            : Ok(ApiResponses.Ok(vm, "Receta"));
    }

    [HttpGet("consulta/{idConsulta}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RecetaVm>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RecetaVm>>>> PorConsulta(int idConsulta)
    {
        var data = await _svc.ListarPorConsultaAsync(idConsulta);
        return Ok(ApiResponses.Ok(data, "Recetas de la consulta"));
    }

    [HttpGet("{idReceta}/pdf")]
    public async Task<IActionResult> Pdf(int idReceta)
    {
        try
        {
            var bytes = await _svc.ImprimirPdfAsync(idReceta);
            return File(bytes, "application/pdf", $"receta-{idReceta}.pdf");
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(ApiResponses.Fail<object>(knf.Message));
        }
    }

    [HttpPut("{idReceta}")]
    [ProducesResponseType(typeof(ApiResponse<RecetaVm>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<RecetaVm>>> Actualizar(int idReceta, [FromBody] RecetaUpdateDto dto)
    {
        try
        {
            var vm = await _svc.ActualizarAsync(idReceta, dto);
            return Ok(ApiResponses.Ok(vm, "Receta actualizada."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponses.Fail<RecetaVm>(ex.Message));
        }
    }

    [HttpDelete("{idReceta}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<object>>> Eliminar(int idReceta)
    {
        try
        {
            await _svc.EliminarAsync(idReceta);
            return Ok(ApiResponses.Ok<object>(new { idReceta }, "Receta eliminada."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponses.Fail<object>(ex.Message));
        }
    }
}

