namespace blazerFacturacion.Components.Data
{
    public class ArticuloFactura
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public decimal Total => Cantidad * PrecioUnitario;

    }
}
