using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.CartaPorte
{
    public class DireccionInfo
    {
        public string CveEstado { get; set; }
        public string Estado { get; set; }
        public string CveMunicipio { get; set; }
        public string Municipio { get; set; }
        public List<string> Colonias { get; set; }
    }
}
