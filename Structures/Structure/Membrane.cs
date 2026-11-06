using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    public class Membrane : Face
    {

        
        public new void Initialise()
        {
        
        }
        
        public Membrane() : base()
        {
            Initialise();
        }

        public Membrane(int id) : base(id)
        {
            Initialise();
        }

    }
}
