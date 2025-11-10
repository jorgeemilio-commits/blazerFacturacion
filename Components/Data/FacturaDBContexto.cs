using Microsoft.EntityFrameworkCore;

namespace blazerFacturacion.Components.Data
{
    public class FacturaDbContexto : DbContext
    {
        public FacturaDbContexto(DbContextOptions<FacturaDbContexto> options)
            : base(options)
        {
        }

        // La tabla 'Facturas' usa la clase 'Factura'
        public DbSet<Factura> Facturas { get; set; }

        // La tabla 'ArticulosFactura' usa la clase 'ArticuloFactura'
        public DbSet<ArticuloFactura> ArticulosFactura { get; set; }

    }
}