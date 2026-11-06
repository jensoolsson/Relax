using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    class Rope : Material
    {
        public Rope()
        {
            _E = 7900000000;    // http://www-materials.eng.cam.ac.uk/mpsite/short/OCR/ropes/default.html
            _G = 210000000;     //Made up value
            _density = 1540;

            _yieldStrength = 225000000;          //225 MPa  ?
            _ultimateStrength = 225000000;       //225 MPa  ?
        }

    }
}
