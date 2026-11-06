using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    class Concrete : Material
    {
        public Concrete()
        {
            //High strenght concrete
            _yieldStrength = 40000000;  //40 MPa in compression
            _E = 30000000000;           //30 GPa   E for compression
            _G = 2200000000;            //2.2GPa
            _density = 2400;
        }

    }
}
