using blazerFacturacion.Data;
using System.Collections.Generic;

namespace blazerFacturacion.Components.Servicios
{
    public class ServicioFactura
    {
        // Setter
        private readonly List<Factura> _facturas = new List<Factura>();

        // Getter
        public List<Factura> GetFacturas()
        {
            return _facturas;
        }

        // Agregar factura
        public void AgregarFactura(Factura nuevaFactura)
        {
            if (nuevaFactura != null)
            {
                _facturas.Add(nuevaFactura);
            }
        }
    }
}
