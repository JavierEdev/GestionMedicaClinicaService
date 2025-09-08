namespace GestionClinica.Domain.DTOs;

// Citas
public record CitaCreateDto(int IdPaciente, int IdMedico, DateTime Fecha);
public record CancelarDto(string Razon);
public record ReprogramarDto(DateTime NuevaFecha, string? Motivo);
public record CitaVm(int IdCita, int IdPaciente, int IdMedico, DateTime Fecha, string Estado);
public record BuscarMedicosDto(string Especialidad, DateTime Fecha);
public record CitaCreatedVm(int IdCita, int IdPaciente, int IdMedico, DateTime Fecha);
public record CitaCancelledVm(int IdCita, string Razon);
public record CitaRescheduledVm(int IdCita, DateTime NuevaFecha, string? Motivo);


// Médicos
public record MedicoCreateDto(string Nombres, string Apellidos, string? NumeroColegiado, string Especialidad, string? Telefono, string? Correo);
public record MedicoListVm(int Id, string NombreCompleto, string Especialidad);
public record MedicoEspecialidadVm(int Id, string NombreCompleto, string Especialidad, string Horario);
public record ProductividadMedicaDto(int IdMedico, string Medico, int Consultas, int Procedimientos);
public record MedicoDetailVm(int Id, string Nombres, string Apellidos, string NumeroColegiado,string Especialidad, string? Telefono, string? Correo, string Horario);
public record DiaDisponibilidadVm(DateOnly Fecha, List<string> HorasDisponibles);
public record MedicoUpdateDto(string Nombres, string Apellidos, string NumeroColegiado, string Especialidad, string? Telefono, string? Correo);

// Recetas
public record RecetaItemDto(string Medicamento, string Dosis, string Frecuencia, string Duracion);
public record RecetaCreateDto(int IdConsulta, List<RecetaItemDto> Items);
public record RecetaVm(int IdReceta, int IdConsulta, string Medicamento, string Dosis, string Frecuencia, string Duracion);
public record MedicamentoHistoricoVm(DateTime Fecha, int IdReceta, string Medicamento, string Dosis, string Frecuencia, string Duracion, int IdConsulta, int IdMedico, string MedicoNombre);
public record RecetaBatchCreatedVm(int IdConsulta, int Total, IEnumerable<int> Ids);
public record RecetaDetalleVm(int IdReceta, int IdConsulta, int IdPaciente, int IdMedico, DateTime FechaConsulta, string Medicamento, string Dosis, string Frecuencia, string Duracion, string MedicoNombre, string PacienteNombre, string Especialidad, string? Diagnostico, string? Observaciones, string ClinicaNombre);
public record RecetaUpdateDto(string Medicamento, string Dosis, string Frecuencia, string Duracion);

