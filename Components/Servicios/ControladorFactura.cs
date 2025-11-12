using blazerFacturacion.Components.Data;

namespace blazerFacturacion.Components.Servicios
{
    public class ControladorFactura
    {
        // Se guarda el último nombre de cliente buscado
        public string NombreClienteActual { get; set; } = string.Empty;

        // Se guarda el borrador de la factura
        public Factura FacturaEnProgreso { get; set; } = new Factura();
    }
}