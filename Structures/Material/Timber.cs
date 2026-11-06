using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    public class Timber : Material
    {   
        //Glulam properties 
        //Source: http://research.ttlchiltern.co.uk/pif294/tdk/background_resources/timber%20engineering%20technology/mechanical%20properties/mechanical%20properties%20of%20glulam.htm

        public Timber() : base()
        {   
            //GL 36H
            _yieldStrength = 31000000;      //31 MPa    Compressive strength parallel to the grain, for tension = 26 Mpa
            _E = 14700000000;               //14.7 GPa  E for compression paralle to grain.
            _G = 910000000;                 //0.91 GPa
            _density = 450;                 
        }
    }
}
