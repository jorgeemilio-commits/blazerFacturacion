using Microsoft.EntityFrameworkCore;

namespace blazerFacturacion.Components.Data
{
    public class FacturaDbContexto : DbContext
    {
        public FacturaDbContexto(DbContextOptions<FacturaDbContexto> options)
            : base(options)
        {
        }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<ArticuloFactura> ArticulosFactura { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Factura>()
                .HasMany(f => f.Articulos) // Una Factura tiene muchos Artículos
                .WithOne(a => a.Factura) // Un Artículo tiene una Factura
                .HasForeignKey(a => a.FacturaId); // La llave foránea es FacturaId
        }
    }
}