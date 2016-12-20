using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    public class Plate : Membrane
    {


        public new void Initialise()
        {
        
        }

        public Plate() : base() 
        {
            Initialise();
        }

        public Plate(int id) : base(id)
        {
            Initialise();    
        }


    }
}
