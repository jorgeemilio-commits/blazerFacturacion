using Microsoft.EntityFrameworkCore;

namespace blazerFacturacion.Components.Data
{
    public class FacturaDbContexto : DbContext
    {
        public FacturaDbContexto(DbContextOptions<FacturaDbContexto> options)
            : base(options)
        {
        }

        // tablas en la base de datos
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<ArticuloFactura> ArticulosFactura { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
