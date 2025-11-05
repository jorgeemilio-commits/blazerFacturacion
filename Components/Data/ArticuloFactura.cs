namespace blazerFacturacion.Components.Data
{
    public class ArticuloFactura
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; } = 1;
        public decimal PrecioUnitario { get; set; } = 0;

        public decimal Total { get { return Cantidad * PrecioUnitario; } }

    }
}
