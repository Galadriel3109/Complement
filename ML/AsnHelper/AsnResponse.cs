using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.AsnHelper
{
    public class AsnResponse
    {
        public int result_count { get; set; }
        public int page_count { get; set; }
        public int page_nbr { get; set; }
        public string next_page { get; set; }
        public string previous_page { get; set; }
        public List<AsnInfo> results { get; set; }
    }
}
