namespace blazerFacturacion.Components.Data
{
    public class ArticuloFactura
    {
        public Guid ArticuloId { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; } = string.Empty;

        public int Cantidad { get; set; } = 1;
        public decimal PrecioUnitario { get; set; } = 0;

        public decimal Total { get { return Cantidad * PrecioUnitario; } }

    }
}
