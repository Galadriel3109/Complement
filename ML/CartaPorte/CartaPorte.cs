using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.CartaPorte
{
    public class CartaPorte
    {
        public ML.CartaPorte.TransTim24? transTim { get; set; }
        public ML.CartaPorte.OperTim2? operTim { get; set; }
        public List<ML.CartaPorte.UbiTim2>? ubiTim { get; set; }
        public List<ML.CartaPorte.DomTim2>? domTim { get; set; }
        public List<ML.CartaPorte.IntTim24>? intTim { get; set; }
    }
}
