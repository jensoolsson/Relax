using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    public class Steel : Material
    {
        //TODO: Confim values with reliable source
        public Steel() : base()
        {
            _E = 210000000000;
            _G = 78000000000;
            _density = 7800;

            _yieldStrength = 250000000;          //250 MPa
            _ultimateStrength = 400000000;       //400 MPa
        }
    }
}
