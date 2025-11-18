using blazerFacturacion.Components.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace blazerFacturacion.Components.Servicios
{
    public class ServicioFactura
    {
        private readonly FacturaDbContexto _contexto;
        private readonly string _cadenaConexion;

        public ServicioFactura(FacturaDbContexto contexto, IConfiguration configuracion)
        {
            _contexto = contexto;
            _cadenaConexion = configuracion.GetConnectionString("DefaultConnnection");
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

        // Obtener el catálogo de artículos
        public async Task<List<Articulo>> GetCatalogoArticulosAsync()
        {
            var listaArticulos = new List<Articulo>();

            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();

                var consulta = "SELECT Id, Nombre, Precio FROM Articulos";

                using (var comando = new SqliteCommand(consulta, conexion))
                {
                    using (var lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            listaArticulos.Add(new Articulo
                            {
                                Id = lector.GetInt32(0),      // Columna 0
                                Nombre = lector.GetString(1), // Columna 1
                                Precio = lector.GetDecimal(2) // Columna 2
                            });
                        }
                    }
                }
            }
            return listaArticulos;
        }

        // Guardar o actualizar un artículo en el catálogo
        public async Task GuardarArticuloCatalogoAsync(Articulo articulo)
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();
                string sentencia;

                if (articulo.Id == 0)
                {
                    sentencia = "INSERT INTO Articulos (Nombre, Precio) VALUES (@nombre, @precio)";
                }
                else
                {
                    sentencia = "UPDATE Articulos SET Nombre = @nombre, Precio = @precio WHERE Id = @id";
                }

                using (var comando = new SqliteCommand(sentencia, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", articulo.Nombre);
                    comando.Parameters.AddWithValue("@precio", articulo.Precio);

                    if (articulo.Id != 0)
                    {
                        comando.Parameters.AddWithValue("@id", articulo.Id);
                    }

                    await comando.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
