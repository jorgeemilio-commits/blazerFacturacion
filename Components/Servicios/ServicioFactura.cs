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
        // acualizar una factura existente
        public async Task ActualizarFacturaAsync(int facturaId, string nuevoNombre, DateTime nuevaFecha)
        {
            var factura = await _contexto.Facturas.FindAsync(facturaId);
            if (factura != null)
            {
                factura.NombreCliente = nuevoNombre;
                factura.Fecha = nuevaFecha;
                await _contexto.SaveChangesAsync();
            }
        }

        // elimina una factura existente
        public async Task EliminarFacturaAsync(int facturaId)
        {
            var factura = await _contexto.Facturas.FindAsync(facturaId);
            if (factura != null)
            {
                var articulos = await _contexto.ArticulosFactura
                                    .Where(a => a.FacturaId == facturaId)
                                    .ToListAsync();

                if (articulos.Any())
                {
                    _contexto.ArticulosFactura.RemoveRange(articulos);
                }

                _contexto.Facturas.Remove(factura);
                await _contexto.SaveChangesAsync();
            }
        }

        // Busca facturas por cliente y año
        public async Task<List<Factura>> GetFacturasPorAnioAsync(string nombreCliente, int anio)
        {
            if (string.IsNullOrWhiteSpace(nombreCliente) || anio <= 0)
            {
                return new List<Factura>();
            }

            var nombreBusquedaUpper = nombreCliente.ToUpper();

            return await _contexto.Facturas
                // Solo se ocupan los totales
                .Where(f => f.NombreCliente != null &&
                            f.NombreCliente.ToUpper() == nombreBusquedaUpper &&
                            f.Fecha.Year == anio)
                .OrderBy(f => f.Fecha) // Ordenar por fecha
                .ToListAsync();
        }
    }
}
