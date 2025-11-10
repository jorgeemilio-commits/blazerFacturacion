using Microsoft.EntityFrameworkCore;

namespace blazerFacturacion.Components.Data
{
    public class FacturaDbContexto : DbContext
    {
        public FacturaDbContexto(DbContextOptions<FacturaDbContexto> options)
            : base(options)
        {
        }
        public DbSet<ArticuloFactura> ArticulosFactura { get; set; }

    }
}