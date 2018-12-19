using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveService.Models
{
    public class VeryfiModel
    {
        public List<string> imgList;

        public double Confidence { get; internal set; }
        public bool IsIdentical { get; internal set; }
    }
}
