using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace blazerFacturacion.Components.Data
{
    public class ArticuloFactura
    {
        [Key]
        public Guid ArticuloId { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; } = 1;
        public decimal PrecioUnitario { get; set; } = 0;
        public decimal Total { get { return Cantidad * PrecioUnitario; } }

        public Guid FacturaId { get; set; }

        [JsonIgnore] 
        public Factura? Factura { get; set; }
    }
}