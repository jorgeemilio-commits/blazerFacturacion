using System.ComponentModel.DataAnnotations;

namespace blazerFacturacion.Components.Data
{
    public class ArticuloFactura
    {
        [Key]
        public Guid ArticuloId { get; set; } = Guid.NewGuid();
        public string NombreCliente { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; } = 1;
        public decimal PrecioUnitario { get; set; } = 0;

        public DateTime Fecha { get; set; } = DateTime.Now;

        public decimal Total { get { return Cantidad * PrecioUnitario; } }
    }
}