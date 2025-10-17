using System;
using System.Collections.Generic;
using Internado.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Data;

public partial class InternadoDbContext : DbContext
{
    public InternadoDbContext(DbContextOptions<InternadoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asistencium> Asistencia { get; set; }
    public virtual DbSet<AuditoriaAcceso> AuditoriaAccesos { get; set; }
    public virtual DbSet<Calificacione> Calificaciones { get; set; }
    public virtual DbSet<Consulta> Consultas { get; set; }
    public virtual DbSet<Curso> Cursos { get; set; }
    public virtual DbSet<HistorialAcademico> HistorialAcademicos { get; set; }
    public virtual DbSet<HistorialMedico> HistorialMedicos { get; set; }
    public virtual DbSet<Medicamento> Medicamentos { get; set; }
    public virtual DbSet<MovimientosMedicamento> MovimientosMedicamentos { get; set; }
    public virtual DbSet<Periodo> Periodos { get; set; }
    public virtual DbSet<Residente> Residentes { get; set; }
    public virtual DbSet<Habitacion> Habitaciones { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<DocenteCurso> DocenteCursos { get; set; }
    public virtual DbSet<LoginAttempt> LoginAttempts { get; set; }
    public virtual DbSet<vw_Indicadore> vw_Indicadores { get; set; }
    public virtual DbSet<vw_ReportesGenerale> vw_ReportesGenerales { get; set; }
     

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Si tienes IEntityTypeConfiguration<> en el proyecto, se aplicarán aquí:
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InternadoDbContext).Assembly);
       

        modelBuilder.Entity<Asistencium>(entity =>
        {
            entity.HasOne(d => d.Curso).WithMany(p => p.Asistencia).HasConstraintName("FK_Asistencia_Curso");
            entity.HasOne(d => d.Residente).WithMany(p => p.Asistencia).HasConstraintName("FK_Asistencia_Res");
        });

        modelBuilder.Entity<AuditoriaAcceso>(entity =>
        {
            entity.Property(e => e.FechaUtc).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<Calificacione>(entity =>
        {
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysutcdatetime())");
            entity.HasOne(d => d.Curso).WithMany(p => p.Calificaciones).HasConstraintName("FK_Calif_Curso");
            entity.HasOne(d => d.Residente).WithMany(p => p.Calificaciones).HasConstraintName("FK_Calif_Res");
        });

        modelBuilder.Entity<Consulta>(entity =>
        {
            entity.HasOne(d => d.Medico).WithMany(p => p.Consulta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cons_Med");

            entity.HasOne(d => d.Residente).WithMany(p => p.Consulta).HasConstraintName("FK_Cons_Res");
        });

        modelBuilder.Entity<Curso>(entity =>
        {
            entity.HasOne(d => d.Docente).WithMany(p => p.Cursos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cursos_Docente");
        });

        modelBuilder.Entity<HistorialAcademico>(entity =>
        {
            entity.HasOne(d => d.Residente).WithMany(p => p.HistorialAcademicos).HasConstraintName("FK_HA_Res");
        });

        modelBuilder.Entity<HistorialMedico>(entity =>
        {
            entity.HasOne(d => d.Residente).WithMany(p => p.HistorialMedicos).HasConstraintName("FK_HM_Res");
        });

        modelBuilder.Entity<Medicamento>(entity =>
        {
            entity.Property(e => e.StockMinimo).HasDefaultValue(10);
        });

        modelBuilder.Entity<MovimientosMedicamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_MovimientosMed");

            entity.Property(e => e.Fecha).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Medicamento).WithMany(p => p.MovimientosMedicamentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MM_Med");

            entity.HasOne(d => d.Usuario).WithMany(p => p.MovimientosMedicamentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MM_Usr");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        modelBuilder.Entity<vw_Indicadore>(entity =>
        {
            entity.ToView("vw_Indicadores", "rep");
        });

        modelBuilder.Entity<vw_ReportesGenerale>(entity =>
        {
            entity.ToView("vw_ReportesGenerales", "rep");
            entity.Property(e => e.ResidenteId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FechaIntento).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.TipoError).HasMaxLength(100).HasDefaultValue("CredencialesInvalidas");
            entity.Property(e => e.Usuario).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DireccionIp).HasMaxLength(45);

            entity.HasOne(d => d.UsuarioNavigation)
                .WithMany()
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_LoginAttempts_Usuarios");
        });

        modelBuilder.Entity<DocenteCurso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FechaAsignacion).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Activa).HasDefaultValue(true);

            entity.HasOne(d => d.Docente)
                .WithMany(p => p.DocenteCursos)
                .HasForeignKey(d => d.DocenteId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DocenteCurso_Usuario");

            entity.HasOne(d => d.Curso)
                .WithMany(p => p.AsignacionesDocentes)
                .HasForeignKey(d => d.CursoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DocenteCurso_Curso");

            // Índice único: un docente no puede estar asignado dos veces al mismo curso
            entity.HasIndex(e => new { e.DocenteId, e.CursoId }).IsUnique();
        });

        // === Configuración de RESIDENTE usando ResidenteConfiguration ===

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}