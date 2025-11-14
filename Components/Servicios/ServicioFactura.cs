using blazerFacturacion.Components.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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

        // Guarda una factura
        public async Task GuardarFacturaAsync(Factura nuevaFactura)
        {
            if (nuevaFactura != null)
            {
                nuevaFactura.Total = nuevaFactura.TotalCalculado;

                _contexto.Facturas.Add(nuevaFactura);
                await _contexto.SaveChangesAsync();
            }
        }

        // Busca facturas anteriores por cliente
        public async Task<List<Factura>> GetFacturasPorClienteAsync(string nombreCliente)
        {
            if (string.IsNullOrWhiteSpace(nombreCliente))
            {
                return new List<Factura>();
            }
            var nombreBusquedaUpper = nombreCliente.ToUpper();

            return await _contexto.Facturas
                .Include(f => f.Articulos)
                .Where(f => f.NombreCliente != null && f.NombreCliente.ToUpper() == nombreBusquedaUpper)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync();
        }
    }
}