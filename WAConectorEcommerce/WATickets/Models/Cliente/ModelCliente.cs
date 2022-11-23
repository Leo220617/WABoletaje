namespace WATickets.Models.Cliente
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ModelCliente : DbContext
    {
        public ModelCliente()
            : base("name=ModelCliente")
        {
        }

        public virtual DbSet<BitacoraErrores> BitacoraErrores { get; set; }
        public virtual DbSet<Garantias> Garantias { get; set; }
        public virtual DbSet<LlamadasServicios> LlamadasServicios { get; set; }
        public virtual DbSet<Login> Login { get; set; }
        public virtual DbSet<Parametros> Parametros { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<SeguridadModulos> SeguridadModulos { get; set; }
        public virtual DbSet<SeguridadRolesModulos> SeguridadRolesModulos { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<Sucursales> Sucursales { get; set; }
        public virtual DbSet<TiposCasos> TiposCasos { get; set; }
        public virtual DbSet<Tecnicos> Tecnicos { get; set; }
        public virtual DbSet<ConexionSAP> ConexionSAP { get; set; }
        public virtual DbSet<ProductosPadres> ProductosPadres { get; set; }
        public virtual DbSet<ProductosHijos> ProductosHijos { get; set; }
        public virtual DbSet<EncReparacion> EncReparacion { get; set; }
        public virtual DbSet<Bodegas> Bodegas { get; set; }

        public virtual DbSet<DetReparacion> DetReparacion { get; set; }
        public virtual DbSet<BitacoraMovimientos> BitacoraMovimientos { get; set; }
        public virtual DbSet<DetBitacoraMovimientos> DetBitacoraMovimientos { get; set; }
        public virtual DbSet<Diagnosticos> Diagnosticos { get; set; }
        public virtual DbSet<Errores> Errores { get; set; }
        public virtual DbSet<Adjuntos> Adjuntos { get; set; }
        public virtual DbSet<CorreoEnvio> CorreoEnvio { get; set; }
        public virtual DbSet<BackOffice> BackOffice { get; set; }
        public virtual DbSet<EncMovimiento> EncMovimiento { get; set; }
        public virtual DbSet<DetMovimiento> DetMovimiento { get; set; }
        public virtual DbSet<PadresHijosProductos> PadresHijosProductos { get; set; }
        public virtual DbSet<CotizacionesAprobadas> CotizacionesAprobadas { get; set; }
        public virtual DbSet<Impuestos> Impuestos { get; set; }
        public virtual DbSet<Asuntos> Asuntos { get; set; }











        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BitacoraErrores>()
                .Property(e => e.DocNum)
                .IsUnicode(false);

            modelBuilder.Entity<BitacoraErrores>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<BitacoraErrores>()
                .Property(e => e.Descripcion)
                .IsUnicode(false);

            modelBuilder.Entity<BitacoraErrores>()
                .Property(e => e.StackTrace)
                .IsUnicode(false);

            modelBuilder.Entity<Garantias>()
                .Property(e => e.idSAP)
                .IsUnicode(false);

            modelBuilder.Entity<Garantias>()
                .Property(e => e.Nombre)
                .IsUnicode(false);

            modelBuilder.Entity<LlamadasServicios>()
                .Property(e => e.TipoLlamada)
                .IsUnicode(false);

            modelBuilder.Entity<LlamadasServicios>()
                .Property(e => e.CardCode)
                .IsUnicode(false);

            modelBuilder.Entity<LlamadasServicios>()
                .Property(e => e.SerieFabricante)
                .IsUnicode(false);

            modelBuilder.Entity<LlamadasServicios>()
                .Property(e => e.ItemCode)
                .IsUnicode(false);

            modelBuilder.Entity<LlamadasServicios>()
                .Property(e => e.Asunto)
                .IsUnicode(false);

            modelBuilder.Entity<LlamadasServicios>()
                .Property(e => e.Comentarios)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.Nombre)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.Clave)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.CardCode)
                .IsUnicode(false);

            modelBuilder.Entity<Roles>()
                .Property(e => e.NombreRol)
                .IsUnicode(false);

            modelBuilder.Entity<SeguridadModulos>()
                .Property(e => e.Descripcion)
                .IsUnicode(false);

            modelBuilder.Entity<Status>()
                .Property(e => e.idSAP)
                .IsUnicode(false);

            modelBuilder.Entity<Status>()
                .Property(e => e.Nombre)
                .IsUnicode(false);

            modelBuilder.Entity<Sucursales>()
                .Property(e => e.idSAP)
                .IsUnicode(false);

            modelBuilder.Entity<Sucursales>()
                .Property(e => e.Nombre)
                .IsUnicode(false);

            modelBuilder.Entity<TiposCasos>()
                .Property(e => e.idSAP)
                .IsUnicode(false);

            modelBuilder.Entity<TiposCasos>()
                .Property(e => e.Nombre)
                .IsUnicode(false);

            modelBuilder.Entity<Tecnicos>()
                .Property(e => e.idSAP)
                .IsUnicode(false);

            modelBuilder.Entity<Tecnicos>()
                .Property(e => e.Nombre)
                .IsUnicode(false);
        }
    }
}
