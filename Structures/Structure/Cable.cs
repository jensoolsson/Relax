using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    public class Cable : Bar
    {

        public new void Initialise() 
        {
            //Presettings for cables.
            _crossSection = new CircularSolid();
            _crossSection.RadiusOuter = 0.05;
            _material = new Steel();
        }

        public Cable() : base() 
        {
        
        }

        public Cable(Node3Dof nodeStart, Node3Dof nodeEnd) : base(nodeStart, nodeEnd)
        {
            Initialise();

            _nts_Sf = 1;
        }

        /** Calculate axial force 
        * 
        */
        public override void CalcAxialForce()
        {
            _N = (_sfEA * _material.YoungsModulus * _crossSection.Area / _initLength) * (_ea);

            AddPreStress();

            if (_N < 0)
                _N = 0;
        }

        public override void DirectSizing()
        {
            base.DirectSizing();
        }








    }
}
