using blazerFacturacion.Components.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks; 

namespace blazerFacturacion.Components.Servicios
{
    public class ServicioFactura
    {
        private readonly FacturaDbContexto _contexto;

        public ServicioFactura(FacturaDbContexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<List<Factura>> GetFacturasAsync()
        {
            return await _contexto.Facturas
                                 .Include(f => f.Articulos)
                                 .ToListAsync();
        }

        public async Task AgregarFacturaAsync(Factura nuevaFactura)
        {
            if (nuevaFactura != null)
            {
                _contexto.Facturas.Add(nuevaFactura);
                await _contexto.SaveChangesAsync();
            }
        }
    }
}