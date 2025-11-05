namespace blazerFacturacion.Components.Data
{
    public class Factura
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public List<ArticuloFactura> Articulos { get; set; } = new List<ArticuloFactura>();
        public decimal Total => Articulos.Sum(a => a.Total);
    }
}
