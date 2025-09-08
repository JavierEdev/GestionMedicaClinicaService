using GestionClinica.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionClinica.Infrastructure.Persistence;

public class ClinicaDbContext : DbContext
{
    public ClinicaDbContext(DbContextOptions<ClinicaDbContext> opt) : base(opt) { }
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<ConsultaMedica> Consultas => Set<ConsultaMedica>();
    public DbSet<ProcedimientoMedico> Procedimientos => Set<ProcedimientoMedico>();
    public DbSet<RecetaMedica> Recetas => Set<RecetaMedica>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Medico>().ToTable("medicos").HasKey(x => x.Id);
        b.Entity<Medico>().Property(x => x.Id).HasColumnName("id_medico");
        b.Entity<Medico>().Property(x => x.Nombres).HasColumnName("nombres");
        b.Entity<Medico>().Property(x => x.Apellidos).HasColumnName("apellidos");
        b.Entity<Medico>().Property(x => x.NumeroColegiado).HasColumnName("numero_colegiado");
        b.Entity<Medico>().Property(x => x.Especialidad).HasColumnName("especialidad");
        b.Entity<Medico>().Property(x => x.Telefono).HasColumnName("telefono");
        b.Entity<Medico>().Property(x => x.Correo).HasColumnName("correo");
        b.Entity<Medico>().Property(x => x.HorarioLaboral).HasColumnName("horario_laboral");

        b.Entity<Cita>().ToTable("citasmedicas").HasKey(x => x.Id);
        b.Entity<Cita>().Property(x => x.Id).HasColumnName("id_cita");
        b.Entity<Cita>().Property(x => x.IdPaciente).HasColumnName("id_paciente");
        b.Entity<Cita>().Property(x => x.IdMedico).HasColumnName("id_medico");
        b.Entity<Cita>().Property(x => x.Fecha).HasColumnName("fecha");
        b.Entity<Cita>().Property(x => x.Estado).HasColumnName("estado");
        b.Entity<Cita>().Property(x => x.RazonCancelacion).HasColumnName("razon_cancelacion");

        b.Entity<Paciente>().ToTable("pacientes").HasKey(x => x.Id);
        b.Entity<Paciente>().Property(x => x.Id).HasColumnName("id_paciente");
        b.Entity<Paciente>().Property(x => x.Nombres).HasColumnName("nombres");
        b.Entity<Paciente>().Property(x => x.Apellidos).HasColumnName("apellidos");
        b.Entity<Paciente>().Property(x => x.Correo).HasColumnName("correo");

        b.Entity<ConsultaMedica>().ToTable("consultasmedicas").HasKey(x => x.Id);
        b.Entity<ConsultaMedica>().Property(x => x.Id).HasColumnName("id_consulta");
        b.Entity<ConsultaMedica>().Property(x => x.IdMedico).HasColumnName("id_medico");
        b.Entity<ConsultaMedica>().Property(x => x.IdPaciente).HasColumnName("id_paciente");
        b.Entity<ConsultaMedica>().Property(x => x.Fecha).HasColumnName("fecha");
        b.Entity<ConsultaMedica>().Property(x => x.MotivoConsulta).HasColumnName("motivo_consulta");
        b.Entity<ConsultaMedica>().Property(x => x.Observaciones).HasColumnName("observaciones");
        b.Entity<ConsultaMedica>().Property(x => x.Diagnostico).HasColumnName("diagnostico");

        b.Entity<ProcedimientoMedico>().ToTable("procedimientosmedicos").HasKey(x => x.IdProcedimiento);
        b.Entity<ProcedimientoMedico>().Property(x => x.IdProcedimiento).HasColumnName("id_procedimiento");
        b.Entity<ProcedimientoMedico>().Property(x => x.IdConsulta).HasColumnName("id_consulta");
        b.Entity<ProcedimientoMedico>().Property(x => x.Procedimiento).HasColumnName("procedimiento");
        b.Entity<ProcedimientoMedico>().Property(x => x.Descripcion).HasColumnName("descripcion");

        b.Entity<RecetaMedica>().ToTable("recetasmedicas").HasKey(x => x.Id);
        b.Entity<RecetaMedica>().Property(x => x.Id).HasColumnName("id_receta");
        b.Entity<RecetaMedica>().Property(x => x.IdConsulta).HasColumnName("id_consulta");
        b.Entity<RecetaMedica>().Property(x => x.Medicamento).HasColumnName("medicamento").HasMaxLength(255);
        b.Entity<RecetaMedica>().Property(x => x.Dosis).HasColumnName("dosis").HasMaxLength(100);
        b.Entity<RecetaMedica>().Property(x => x.Frecuencia).HasColumnName("frecuencia").HasMaxLength(100);
        b.Entity<RecetaMedica>().Property(x => x.Duracion).HasColumnName("duracion").HasMaxLength(50);

    }
}
