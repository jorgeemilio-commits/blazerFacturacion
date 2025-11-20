namespace blazerFacturacion.Components.Data
{
    public class DashboardInfo
    {
        public string ArticuloMasVendido { get; set; } = "Sin datos";
        public string MesMasVentas { get; set; } = "Sin datos";
        public string ClienteEstrella { get; set; } = "Sin datos";
        public decimal TotalIngresos { get; set; } = 0;
        public int TotalFacturas { get; set; } = 0;
    }
}