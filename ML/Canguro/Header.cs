using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.Canguro
{
    public class Header
    {
        ///General
        public string Facility { get; set; }
        public string OrderRelease { get; set; }
        ///Cust SEARS
        public string SCN { get; set; }
        public string Transfer { get; set; }
        public string FechaEntrega { get; set; }
        ///Generales
        public string CodigoCliente { get; set; }
        public string NombreCliente { get; set; }
        public string Telefono0 { get; set; }
        public string Telefono1 { get; set; }
        public string Telefono2 { get; set; }
        ///Direccion
        public string Estado { get; set; }
        public string CodigoEstado { get; set; }
        public string Municipio { get; set; }
        public string Colonia { get; set; }
        public string CodigoPostal { get; set; }
        ///Direccion especifica
        public string Calle { get; set; }
        public string NumExt { get; set; }
        public string NumInt { get; set; }
        ///Extras
        public string Observaciones { get; set; }
        public string Referencias { get; set; }
        public string Panel { get; set; }
        public string MasGente { get; set; }
        public string Volado { get; set; }
        ///Coords
        public string Longitud { get; set; }
        public string Latitud { get; set; }
    }
}
