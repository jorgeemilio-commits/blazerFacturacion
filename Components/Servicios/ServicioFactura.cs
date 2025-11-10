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

        public async Task<List<ArticuloFactura>> GetArticulosPorClienteAsync(string nombreCliente)
        {
            // checa si no esta vacio
            if (string.IsNullOrWhiteSpace(nombreCliente))
            {
                return new List<ArticuloFactura>();
            }

            return await _contexto.ArticulosFactura
                                 .Where(a => a.NombreCliente.ToLower() == nombreCliente.ToLower())
                                 .ToListAsync();
        }

        // agrega un solo artículo a la BD
        public async Task AgregarArticuloAsync(ArticuloFactura nuevoArticulo)
        {
            if (nuevoArticulo != null && !string.IsNullOrWhiteSpace(nuevoArticulo.NombreCliente))
            {
                _contexto.ArticulosFactura.Add(nuevoArticulo);
                await _contexto.SaveChangesAsync();
            }
        }
    }
}