using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.CartaPorte
{
    public class ScnDir
    {
        public string NumScn { get; set; }
        public string CodDir { get; set; }
        public string Calle { get; set; }
        public string NumExt { get; set; }
        public string NumInt { get; set; }
        public bool OkCol { get; set; }
        public string CveCol { get; set; }
        public string ColCli { get; set; }
        public bool OkMun { get; set; }
        public string CveMun { get; set; }
        public string MunCli { get; set; }
        public bool OkEst { get; set; }
        public string CveEst { get; set; }
        public string EstCli { get; set; }
        public bool OkCp { get; set; }
        public string CveCp { get; set; }
        public string CpCli { get; set; }
        public bool OkDireccion { get; set; }
    }
}
