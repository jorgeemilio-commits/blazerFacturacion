using blazerFacturacion.Components.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace blazerFacturacion.Components.Servicios
{
    public class ServicioFactura
    {
        private readonly string _cadenaConexion;

        public ServicioFactura(IConfiguration configuracion)
        {
            _cadenaConexion = configuracion.GetConnectionString("DefaultConnection");
        }

        // --- MÉTODOS DE ESCRITURA ---

        public async Task GuardarFacturaAsync(Factura nuevaFactura)
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();
                // Por defecto Archivada es 0 (false)
                var sqlFactura = @"
                    INSERT INTO Facturas (NombreCliente, Fecha, Total, Archivada) 
                    VALUES (@nombre, @fecha, @total, 0);
                    SELECT last_insert_rowid();";

                int idFacturaGenerado = 0;

                using (var comando = new SqliteCommand(sqlFactura, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", nuevaFactura.NombreCliente);
                    comando.Parameters.AddWithValue("@fecha", nuevaFactura.Fecha.ToString("yyyy-MM-dd"));
                    comando.Parameters.AddWithValue("@total", nuevaFactura.TotalCalculado);

                    var resultado = await comando.ExecuteScalarAsync();
                    idFacturaGenerado = Convert.ToInt32(resultado);
                }

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

        // --- MÉTODOS DE LECTURA ---

        public async Task<List<Factura>> GetFacturasPorClienteAsync(string nombreCliente)
        {
            var listaFacturas = new List<Factura>();
            if (string.IsNullOrWhiteSpace(nombreCliente)) return listaFacturas;

            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();

                var sql = @"SELECT Id, NombreCliente, Fecha, Total, Archivada 
                            FROM Facturas 
                            WHERE UPPER(NombreCliente) = UPPER(@nombre) 
                            AND Archivada = 0
                            ORDER BY Fecha DESC";

                using (var comando = new SqliteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", nombreCliente);
                    using (var lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            listaFacturas.Add(new Factura
                            {
                                Id = lector.GetInt32(0),
                                NombreCliente = lector.GetString(1),
                                Fecha = DateTime.Parse(lector.GetString(2)),
                                Total = lector.GetDecimal(3),
                                Archivada = lector.GetBoolean(4),
                                Articulos = new List<ArticuloFactura>()
                            });
                        }
                    }
                }

                // Cargar artículos 
                foreach (var factura in listaFacturas)
                {
                    var sqlArticulos = @"SELECT ArticuloId, Nombre, Cantidad, PrecioUnitario, FacturaId 
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
                                    ArticuloId = lectorArt.GetInt32(0),
                                    Nombre = lectorArt.GetString(1),
                                    Cantidad = lectorArt.GetInt32(2),
                                    PrecioUnitario = lectorArt.GetDecimal(3),
                                    FacturaId = lectorArt.GetInt32(4)
                                });
                            }
                        }
                    }
                }
            }
            return listaFacturas;
        }

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
                            AND Archivada = 0
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

        // --- OTROS MÉTODOS (Actualizar, Eliminar, etc.) ---

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

        // Métodos de Artículos (Agregar, Actualizar, Eliminar, Recalcular)
        public async Task AgregarArticuloAsync(ArticuloFactura articulo)
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();
                var sql = @"INSERT INTO ArticulosFactura (Nombre, Cantidad, PrecioUnitario, FacturaId) 
                            VALUES (@nombre, @cant, @precio, @idFactura)";
                using (var comando = new SqliteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", articulo.Nombre);
                    comando.Parameters.AddWithValue("@cant", articulo.Cantidad);
                    comando.Parameters.AddWithValue("@precio", articulo.PrecioUnitario);
                    comando.Parameters.AddWithValue("@idFactura", articulo.FacturaId);
                    await comando.ExecuteNonQueryAsync();
                }
            }
            await RecalcularTotalFacturaAsync(articulo.FacturaId);
        }

        public async Task ActualizarArticuloAsync(ArticuloFactura articulo)
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();
                var sql = @"UPDATE ArticulosFactura 
                            SET Nombre = @nombre, Cantidad = @cant, PrecioUnitario = @precio 
                            WHERE ArticuloId = @id";
                using (var comando = new SqliteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", articulo.Nombre);
                    comando.Parameters.AddWithValue("@cant", articulo.Cantidad);
                    comando.Parameters.AddWithValue("@precio", articulo.PrecioUnitario);
                    comando.Parameters.AddWithValue("@id", articulo.ArticuloId);
                    await comando.ExecuteNonQueryAsync();
                }
            }
            await RecalcularTotalFacturaAsync(articulo.FacturaId);
        }

        public async Task EliminarArticuloAsync(int idArticulo, int idFactura)
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();
                var sql = "DELETE FROM ArticulosFactura WHERE ArticuloId = @id";
                using (var comando = new SqliteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@id", idArticulo);
                    await comando.ExecuteNonQueryAsync();
                }
            }
            await RecalcularTotalFacturaAsync(idFactura);
        }

        private async Task RecalcularTotalFacturaAsync(int idFactura)
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();
                var sql = @"UPDATE Facturas 
                            SET Total = (
                                SELECT COALESCE(SUM(Cantidad * PrecioUnitario), 0) 
                                FROM ArticulosFactura 
                                WHERE FacturaId = @id
                            )
                            WHERE Id = @id";
                using (var comando = new SqliteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@id", idFactura);
                    await comando.ExecuteNonQueryAsync();
                }
            }
        }

        // --- MÉTODO PARA ARCHIVAR/DESARCHIVAR ---
        public async Task AlternarArchivoFacturaAsync(int idFactura, bool archivar)
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();
                var sql = "UPDATE Facturas SET Archivada = @estado WHERE Id = @id";
                using (var comando = new SqliteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@estado", archivar ? 1 : 0);
                    comando.Parameters.AddWithValue("@id", idFactura);
                    await comando.ExecuteNonQueryAsync();
                }
            }
        }

        // --- OBTENER SOLO ARCHIVADAS ---
        public async Task<List<Factura>> GetFacturasArchivadasAsync()
        {
            var listaFacturas = new List<Factura>();
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();

                // SOLO LAS QUE TIENEN Archivada = 1
                var sql = @"SELECT Id, NombreCliente, Fecha, Total, Archivada 
                            FROM Facturas 
                            WHERE Archivada = 1
                            ORDER BY Fecha DESC";

                using (var comando = new SqliteCommand(sql, conexion))
                {
                    using (var lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            listaFacturas.Add(new Factura
                            {
                                Id = lector.GetInt32(0),
                                NombreCliente = lector.GetString(1),
                                Fecha = DateTime.Parse(lector.GetString(2)),
                                Total = lector.GetDecimal(3),
                                Archivada = lector.GetBoolean(4)
                            });
                        }
                    }
                }
            }
            return listaFacturas;
        }
    }
}