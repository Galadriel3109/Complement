using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.CartaPorte
{
    public class DomTim2
    {
        public string? des_ori { get; set; }
        public string? calle { get; set; }
        public string? num_ext { get; set; }
        public string? num_int { get; set; }
        public string? col { get; set; }
        public string? loca { get; set; } = "01";
        public string? refe { get; set; } = "X";
        public string? muni { get; set; }
        public string? est { get; set; }
        public string? pais { get; set; } = "MEX";
        public string? cod_pos { get; set; }
        public string? id_uni { get; set; }
        public decimal? cod_emp { get; set; } = 1m;
    }
}
