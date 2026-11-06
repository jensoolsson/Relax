using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    class GroupN : Group
    {
        List<Vertex> _vertices;


        public new void Initialise() 
        {
            _vertices = new List<Vertex>();
        
        }


        public GroupN() : base()
        {


        }

    }
}
