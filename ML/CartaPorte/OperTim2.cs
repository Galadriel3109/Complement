using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.CartaPorte
{
    public class OperTim2
    {
        public string? rfc_ope { get; set; }
        public string? num_lic { get; set; }
        public string? nom_ope { get; set; }
        public string? tip_fig { get; set; } = "01";
        public string? part_trans { get; set; } = "PT02";
        public string? id_uni { get; set; }
        public decimal? cod_emp { get; set; } = 1m;
    }
}
