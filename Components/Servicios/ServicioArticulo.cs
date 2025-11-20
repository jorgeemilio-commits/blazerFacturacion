using blazerFacturacion.Components.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace blazerFacturacion.Components.Servicios
{
    public class ServicioArticulo
    {
        private readonly string _cadenaConexion;

        public ServicioArticulo(IConfiguration configuracion)
        {
            _cadenaConexion = configuracion.GetConnectionString("DefaultConnection");
        }

        // Leer Catálogo de Artículos
        public async Task<List<Articulo>> GetCatalogoArticulosAsync()
        {
            var lista = new List<Articulo>();

            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();
                var sql = "SELECT Id, Nombre, Precio FROM Articulos";

                using (var comando = new SqliteCommand(sql, conexion))
                {
                    using (var lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            lista.Add(new Articulo
                            {
                                Id = lector.GetInt32(0),
                                Nombre = lector.GetString(1),
                                Precio = lector.GetDecimal(2)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // Guardar Articulo (Nuevo o Editar)
        public async Task GuardarArticuloCatalogoAsync(Articulo articulo)
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();
                string sql;

                if (articulo.Id == 0) // Es nuevo
                {
                    sql = "INSERT INTO Articulos (Nombre, Precio) VALUES (@nombre, @precio)";
                }
                else // Es edición
                {
                    sql = "UPDATE Articulos SET Nombre = @nombre, Precio = @precio WHERE Id = @id";
                }

                using (var comando = new SqliteCommand(sql, conexion))
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

        // Eliminar Articulo del Catálogo
        public async Task EliminarArticuloCatalogoAsync(int id)
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();
                var sql = "DELETE FROM Articulos WHERE Id = @id";

                using (var comando = new SqliteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@id", id);
                    await comando.ExecuteNonQueryAsync();
                }
            }
        }
    }
}