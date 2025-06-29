using Microsoft.EntityFrameworkCore;
using SistemaControlAC.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<EquipoAireAcondicionado> Equipos { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<NotaReparacion> NotasReparacion { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("UsuarioID");
                entity.Property(e => e.NombreUsuario).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Rol).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.NombreUsuario).IsUnique();
            });

            // Configuración de Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clientes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ClienteID");
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Telefono).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Direccion).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Ciudad).HasMaxLength(50);
                entity.Property(e => e.CodigoPostal).HasMaxLength(10);
            });

            // Configuración de Equipo
            modelBuilder.Entity<EquipoAireAcondicionado>(entity =>
            {
                entity.ToTable("Equipos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("EquipoID");
                entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
                entity.Property(e => e.Marca).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Modelo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.NumeroSerie).HasMaxLength(100);
                entity.Property(e => e.Tipo).HasColumnName("TipoEquipo").IsRequired().HasMaxLength(30);
                entity.Property(e => e.Capacidad).HasMaxLength(20);
                entity.Property(e => e.Ubicacion).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FechaInstalacion).HasColumnName("FechaInstalacion");

                // CORREGIDO: Mapear Activo (bool) a EstadoEquipo (BIT) correctamente
                entity.Property(e => e.Activo)
                    .HasColumnName("EstadoEquipo")
                    .HasDefaultValue(true);

                // Relación con Cliente
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Equipos)
                    .HasForeignKey(e => e.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict); // Evitar eliminación en cascada

                // Índices para optimización
                entity.HasIndex(e => e.ClienteId)
                    .HasDatabaseName("IX_Equipos_Cliente");
                entity.HasIndex(e => e.NumeroSerie)
                    .HasDatabaseName("IX_Equipos_NumeroSerie");
                entity.HasIndex(e => e.Activo)
                    .HasDatabaseName("IX_Equipos_Estado");
            });

            // Configuración de Cita
            modelBuilder.Entity<Cita>(entity =>
            {
                entity.ToTable("Citas");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("CitaID");
                entity.Property(e => e.ClienteId).HasColumnName("ClienteID");
                entity.Property(e => e.EquipoId).HasColumnName("EquipoID");
                entity.Property(e => e.TecnicoAsignadoId).HasColumnName("TecnicoID");
                entity.Property(e => e.FechaProgramada).HasColumnName("FechaCita");
                entity.Property(e => e.TipoServicio).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.CreadoPorUsuarioId).HasColumnName("CreatedBy");

                // Relaciones
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Citas)
                    .HasForeignKey(e => e.ClienteId);

                //entity.HasOne(e => e.Equipo)
                //    //.WithMany(eq => eq.Citas)
                //    .HasForeignKey(e => e.EquipoId);

                entity.HasOne(e => e.TecnicoAsignado)
                    .WithMany(u => u.CitasAsignadas)
                    .HasForeignKey(e => e.TecnicoAsignadoId);

                entity.HasOne(e => e.CreadoPor)
                    .WithMany()
                    .HasForeignKey(e => e.CreadoPorUsuarioId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configuración de NotaReparacion
            modelBuilder.Entity<NotaReparacion>(entity =>
            {
                entity.ToTable("OrdenesTrabajos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("OrdenID");
                entity.Property(e => e.CitaId).HasColumnName("CitaID");
                entity.Property(e => e.DiagnosticoInicial).HasColumnName("Diagnostico");
                entity.Property(e => e.TrabajoRealizado).HasColumnName("TrabajoRealizado");
                entity.Property(e => e.Fecha).HasColumnName("FechaInicio");

                // Relación con Cita
                entity.HasOne(e => e.Cita)
                    .WithMany(c => c.NotasReparacion)
                    .HasForeignKey(e => e.CitaId);
            });

            // Configuración de Notificacion
            modelBuilder.Entity<Notificacion>(entity =>
            {
                entity.ToTable("Notificaciones");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("NotificacionID");
                entity.Property(e => e.TipoNotificacion).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Destinatario).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Asunto).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Mensaje).IsRequired();
                entity.Property(e => e.Estado).IsRequired().HasMaxLength(20);
                entity.Property(e => e.MensajeError).HasColumnName("ErrorMessage").HasMaxLength(500);
            });
        }
    }
}
