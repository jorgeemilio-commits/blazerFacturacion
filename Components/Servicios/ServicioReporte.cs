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
                var sqlIngresos = "SELECT SUM(Total) FROM Facturas";
                using (var comando = new SqliteCommand(sqlIngresos, conexion))
                {
                    var resultado = await comando.ExecuteScalarAsync();
                    if (resultado != DBNull.Value && resultado != null)
                    {
                        info.TotalIngresos = Convert.ToDecimal(resultado);
                    }
                }

                // 2. TOTAL FACTURAS EMITIDAS
                var sqlConteo = "SELECT COUNT(*) FROM Facturas";
                using (var comando = new SqliteCommand(sqlConteo, conexion))
                {
                    info.TotalFacturas = Convert.ToInt32(await comando.ExecuteScalarAsync());
                }

                // 3. ARTÍCULO MÁS VENDIDO (Por cantidad)
                var sqlArticulo = @"
                    SELECT Nombre 
                    FROM ArticulosFactura 
                    GROUP BY Nombre 
                    ORDER BY SUM(Cantidad) DESC 
                    LIMIT 1";
                using (var comando = new SqliteCommand(sqlArticulo, conexion))
                {
                    var resultado = await comando.ExecuteScalarAsync();
                    if (resultado != null) info.ArticuloMasVendido = resultado.ToString();
                }

                // 4. CLIENTE BALLENA (El que más ha gastado)
                var sqlCliente = @"
                    SELECT NombreCliente 
                    FROM Facturas 
                    GROUP BY NombreCliente 
                    ORDER BY SUM(Total) DESC 
                    LIMIT 1";
                using (var comando = new SqliteCommand(sqlCliente, conexion))
                {
                    var resultado = await comando.ExecuteScalarAsync();
                    if (resultado != null) info.ClienteEstrella = resultado.ToString();
                }

                // 5. MEJOR MES (Mes con más ingresos)
                // Usamos strftime para agrupar por mes numérico ('01', '02')
                var sqlMes = @"
                    SELECT strftime('%m', Fecha) 
                    FROM Facturas 
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