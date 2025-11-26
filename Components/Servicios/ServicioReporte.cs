using blazerFacturacion.Components.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace blazerFacturacion.Components.Servicios
{
    public class ServicioReporte
    {
        private readonly string _cadenaConexion;

        public ServicioReporte(IConfiguration configuracion)
        {
            _cadenaConexion = configuracion.GetConnectionString("DefaultConnection");
        }

        public async Task<DashboardInfo> ObtenerDashboardAsync()
        {
            var info = new DashboardInfo();

            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                await conexion.OpenAsync();

                // 1. TOTAL INGRESOS 
                var sqlIngresos = "SELECT SUM(Total) FROM Facturas WHERE Archivada = 0"; // WHERE Archivada es para checar si esta archivada o no
                using (var comando = new SqliteCommand(sqlIngresos, conexion))
                {
                    var resultado = await comando.ExecuteScalarAsync();
                    if (resultado != DBNull.Value && resultado != null)
                    {
                        info.TotalIngresos = Convert.ToDecimal(resultado);
                    }
                }

                // 2. TOTAL FACTURAS EMITIDAS 
                var sqlConteo = "SELECT COUNT(*) FROM Facturas WHERE Archivada = 0";
                using (var comando = new SqliteCommand(sqlConteo, conexion))
                {
                    info.TotalFacturas = Convert.ToInt32(await comando.ExecuteScalarAsync());
                }

                // 3. ARTÍCULO MÁS VENDIDO 
                var sqlArticulo = @"
                    SELECT af.Nombre 
                    FROM ArticulosFactura af
                    JOIN Facturas f ON af.FacturaId = f.Id
                    WHERE f.Archivada = 0
                    GROUP BY af.Nombre 
                    ORDER BY SUM(af.Cantidad) DESC 
                    LIMIT 1";
                using (var comando = new SqliteCommand(sqlArticulo, conexion))
                {
                    var resultado = await comando.ExecuteScalarAsync();
                    if (resultado != null) info.ArticuloMasVendido = resultado.ToString();
                }

                // 4. CLIENTE BALLENA 
                var sqlCliente = @"
                    SELECT NombreCliente 
                    FROM Facturas 
                    WHERE Archivada = 0
                    GROUP BY NombreCliente 
                    ORDER BY SUM(Total) DESC 
                    LIMIT 1";
                using (var comando = new SqliteCommand(sqlCliente, conexion))
                {
                    var resultado = await comando.ExecuteScalarAsync();
                    if (resultado != null) info.ClienteEstrella = resultado.ToString();
                }

                // 5. MEJOR MES 
                var sqlMes = @"
                    SELECT strftime('%m', Fecha) 
                    FROM Facturas 
                    WHERE Archivada = 0
                    GROUP BY strftime('%m', Fecha) 
                    ORDER BY SUM(Total) DESC 
                    LIMIT 1";
                using (var comando = new SqliteCommand(sqlMes, conexion))
                {
                    var resultado = await comando.ExecuteScalarAsync();
                    if (resultado != null)
                    {
                        int numeroMes = Convert.ToInt32(resultado);
                        string nombreMes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(numeroMes);
                        info.MesMasVentas = char.ToUpper(nombreMes[0]) + nombreMes.Substring(1);
                    }
                }
            }

            return info;
        }
    }
}