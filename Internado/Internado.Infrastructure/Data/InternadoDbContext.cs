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
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Usuario> Usuarios { get; set; }
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

        // === Configuración específica de RESIDENTE ===
        modelBuilder.Entity<Residente>(entity =>
        {
            // Si tu tabla está en otro esquema, descomenta y ajusta:
            // entity.ToTable("Residentes", "dbo");

            entity.Property(e => e.NombreCompleto)
                  .HasMaxLength(120)
                  .IsRequired();

            entity.Property(e => e.DPI)
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.Tutor)
                  .HasMaxLength(120)
                  .IsRequired();

            entity.Property(e => e.Estado)
                  .HasMaxLength(20)
                  .IsRequired();

            // Solo fecha (sin hora) en SQL Server
            entity.Property(e => e.FechaNacimiento).HasColumnType("date");
            entity.Property(e => e.FechaIngreso).HasColumnType("date");
            entity.Property(e => e.FechaEgreso).HasColumnType("date");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
