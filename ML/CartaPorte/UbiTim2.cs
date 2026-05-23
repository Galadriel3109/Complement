using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.CartaPorte
{
    public class UbiTim2
    {
        public string? rfc_rem { get; set; }
        public string? rfc_des { get; set; }
        public string? id_ori { get; set; }
        public string? id_des { get; set; }
        public string? fec_sal { get; set; }
        public string? fec_lle { get; set; }
        public decimal? dis_rec { get; set; } = 3.2m;
        public string? id_uni { get; set; }
        public decimal? cod_emp { get; set; } = 1m;
    }
}
