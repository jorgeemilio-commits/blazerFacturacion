using blazerFacturacion.Components.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System; 

namespace blazerFacturacion.Components.Servicios
{
    public class ServicioFactura
    {
        private readonly string _cadenaConexion;

        public ServicioFactura(IConfiguration configuracion)
        {
            _cadenaConexion = configuracion.GetConnectionString("DefaultConnection");
        }

        // Guarda una factura (INSERT)
        public async Task GuardarFacturaAsync(Factura nuevaFactura)
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();

                // Inserta la "Cabecera" de la factura
                var sqlFactura = @"
                    INSERT INTO Facturas (NombreCliente, Fecha, Total) 
                    VALUES (@nombre, @fecha, @total);
                    SELECT last_insert_rowid();";

                int idFacturaGenerado = 0;

                using (var comando = new SqliteCommand(sqlFactura, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", nuevaFactura.NombreCliente);
                    comando.Parameters.AddWithValue("@fecha", nuevaFactura.Fecha.ToString("yyyy-MM-dd"));
                    comando.Parameters.AddWithValue("@total", nuevaFactura.TotalCalculado);

                    // Ejecuta la consulta y devuelve el primer valor (el ID)
                    var resultado = await comando.ExecuteScalarAsync();
                    idFacturaGenerado = Convert.ToInt32(resultado);
                }

                // Inserta los "Artículos" de la factura
                foreach (var articulo in nuevaFactura.Articulos)
                {
                    var sqlArticulo = @"
                        INSERT INTO ArticulosFactura (Nombre, Cantidad, PrecioUnitario, FacturaId) 
                        VALUES (@nombre, @cant, @precio, @idFactura)";

                    using (var comandoArt = new SqliteCommand(sqlArticulo, conexion))
                    {
                        comandoArt.Parameters.AddWithValue("@nombre", articulo.Nombre);
                        comandoArt.Parameters.AddWithValue("@cant", articulo.Cantidad);
                        comandoArt.Parameters.AddWithValue("@precio", articulo.PrecioUnitario);
                        comandoArt.Parameters.AddWithValue("@idFactura", idFacturaGenerado);

                        await comandoArt.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        // Ver facturas por nombre de cliente (SELECT)
        public async Task<List<Factura>> GetFacturasPorClienteAsync(string nombreCliente)
        {
            var listaFacturas = new List<Factura>();

            if (string.IsNullOrWhiteSpace(nombreCliente)) return listaFacturas;

            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();

                // Buscamos las facturas (Cabeceras)
                var sql = @"SELECT Id, NombreCliente, Fecha, Total 
                            FROM Facturas 
                            WHERE UPPER(NombreCliente) = UPPER(@nombre) 
                            ORDER BY Fecha DESC";

                using (var comando = new SqliteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", nombreCliente);

                    using (var lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            var factura = new Factura
                            {
                                Id = lector.GetInt32(0),
                                NombreCliente = lector.GetString(1),
                                Fecha = DateTime.Parse(lector.GetString(2)),
                                Total = lector.GetDecimal(3),
                                Articulos = new List<ArticuloFactura>() // Inicializamos lista vacía
                            };
                            listaFacturas.Add(factura);
                        }
                    }
                }

                // Carga los artículos para cada factura encontrada
                foreach (var factura in listaFacturas)
                {
                    var sqlArticulos = @"SELECT Nombre, Cantidad, PrecioUnitario 
                                         FROM ArticulosFactura 
                                         WHERE FacturaId = @id";

                    using (var cmdArt = new SqliteCommand(sqlArticulos, conexion))
                    {
                        cmdArt.Parameters.AddWithValue("@id", factura.Id);
                        using (var lectorArt = await cmdArt.ExecuteReaderAsync())
                        {
                            while (await lectorArt.ReadAsync())
                            {
                                factura.Articulos.Add(new ArticuloFactura
                                {
                                    Nombre = lectorArt.GetString(0),
                                    Cantidad = lectorArt.GetInt32(1),
                                    PrecioUnitario = lectorArt.GetDecimal(2)
                                });
                            }
                        }
                    }
                }
            }
            return listaFacturas;
        }

        // Actualizar factura (UPDATE)
        public async Task ActualizarFacturaAsync(int idFactura, string nuevoNombre, DateTime nuevaFecha)
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();
                var sql = "UPDATE Facturas SET NombreCliente = @nombre, Fecha = @fecha WHERE Id = @id";

                using (var comando = new SqliteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", nuevoNombre);
                    comando.Parameters.AddWithValue("@fecha", nuevaFecha.ToString("yyyy-MM-dd"));
                    comando.Parameters.AddWithValue("@id", idFactura);

                    await comando.ExecuteNonQueryAsync();
                }
            }
        }

        // Eliminar factura (DELETE)
        public async Task EliminarFacturaAsync(int idFactura)
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();

                var sqlBorrarHijos = "DELETE FROM ArticulosFactura WHERE FacturaId = @id";
                using (var cmdHijos = new SqliteCommand(sqlBorrarHijos, conexion))
                {
                    cmdHijos.Parameters.AddWithValue("@id", idFactura);
                    await cmdHijos.ExecuteNonQueryAsync();
                }

                var sqlBorrarPadre = "DELETE FROM Facturas WHERE Id = @id";
                using (var cmdPadre = new SqliteCommand(sqlBorrarPadre, conexion))
                {
                    cmdPadre.Parameters.AddWithValue("@id", idFactura);
                    await cmdPadre.ExecuteNonQueryAsync();
                }
            }
        }

        // Reporte anual (SELECT con filtros de fecha)
        public async Task<List<Factura>> GetFacturasPorAnioAsync(string nombreCliente, int anio)
        {
            var lista = new List<Factura>();
            if (string.IsNullOrWhiteSpace(nombreCliente)) return lista;

            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();

                var sql = @"SELECT Id, NombreCliente, Fecha, Total 
                            FROM Facturas 
                            WHERE UPPER(NombreCliente) = UPPER(@nombre) 
                            AND strftime('%Y', Fecha) = @anio 
                            ORDER BY Fecha ASC";

                using (var comando = new SqliteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", nombreCliente);
                    comando.Parameters.AddWithValue("@anio", anio.ToString());

                    using (var lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            lista.Add(new Factura
                            {
                                Id = lector.GetInt32(0),
                                NombreCliente = lector.GetString(1),
                                Fecha = DateTime.Parse(lector.GetString(2)),
                                Total = lector.GetDecimal(3)
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}