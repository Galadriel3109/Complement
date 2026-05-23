using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.CartaPorte
{
    public class TransTim24
    {
        public string? per_sct { get; set; }
        public string? num_per { get; set; }
        public string? nom_ase { get; set; } = "SEGUROS INBURSA S.A. GRUPO FIN";
        public string? num_seg { get; set; } = "2610020000000";
        public string? con_veh { get; set; }
        public string? pla_veh { get; set; }
        public string? mod_veh { get; set; }
        public string? tip_rem1 { get; set; }
        public string? pla_rem1 { get; set; }
        public string? tip_rem2 { get; set; }
        public string? pla_rem2 { get; set; }
        public string? aseg_carga { get; set; } = "SEGUROS INBURSA S.A. GRUPO FINANCIERO INBURSA";
        public string? num_carga { get; set; } = "2610020000000";
        public string? aseg_med { get; set; }
        public string? num_med { get; set; }
        public decimal? peso_bruto { get; set; }
        public string? prima_seg { get; set; }
        public string? id_uni { get; set; }
        public decimal? cod_emp { get; set; } = 1m;
    }
}
