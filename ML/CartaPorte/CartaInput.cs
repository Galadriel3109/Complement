using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.CartaPorte
{
    public class CartaInput
    {
        public string PtoAlm { get; set; }
        public string CarSal { get; set; }
        public string IdUni { get; set; }
        public string FecSal { get; set; }
        public string Tipo { get; set; }
        //public string rfc_ope { get; set; }
        public ML.CartaPorte.OperTda OperTda { get; set; }
        //public string num_eco { get; set; }
        public ML.CartaPorte.TraPro TraPro { get; set; }
    }
}
