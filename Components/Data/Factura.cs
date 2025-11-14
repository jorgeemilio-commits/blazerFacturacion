namespace blazerFacturacion.Components.Data
{
    public class Factura
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public List<ArticuloFactura> Articulos { get; set; } = new List<ArticuloFactura>();
        public decimal Total { get; set; }
        public decimal TotalCalculado => Articulos.Sum(a => a.Total);
    }
}