using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.CartaPorte
{
    public class Parada
    {
        public string CodDir { get; set; }
        public int Stop { get; set; }
        public string Ori { get; set; }
        public string Des { get; set; }
        public string Rfc { get; set; }
        public string Calle { get; set; }
        public string NumExt { get; set; }
        public string NumInt { get; set; }
        public string CveCol { get; set; }
        public string CveMun { get; set; }
        public string CveEst { get; set; }
        public string CveCp { get; set; }
        public int SCNCount { get; set; }
    }
}
