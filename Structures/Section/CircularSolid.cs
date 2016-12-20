using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{
    public class CircularSolid : CrossSection
    {
        //Initial sizes not changed when the section is scaled.
        private double _iR1;
        private double _prevR1;

        public void Initialise()
        {
            _crossSectionType = eCrossSection.Circular_Solid;
            _elemMesh = new Cylindrical();

            //Hard coded values for circular sections:
            _r1 = 0.1;
            _iR1 = _r1;
        }

        public CircularSolid() : base()
        {
            Initialise();
        }

        //---------------------------------- Methods ---------------------------------------//
        #region


        /** Calculates mechanical section properties
        *   For formulas see: https://engineering.purdue.edu/~ahvarma/CE%20579/References/AISC_Torsion_Guide.pdf
        */
        public override void CalcSectionProperties()
        {
            _A = Math.Pow(_r1, 2) * Math.PI;
            _Iy = Math.PI * (Math.Pow(_r1, 4)) / 4.0;
            _Ix = Math.PI * (Math.Pow(_r1, 4)) / 4.0;

            _Ixy = 0; //Seems to be zeros for symetric cross sections. Confirm this!!

            //J = pi*(Ro^4 - Ri^4)/2
            _J = Math.PI * (Math.Pow(_r1, 4)) / 2.0;
        }

        /** Scale section with scale factor
        *  
        */
        public override void Scale(double sf)
        {
            _prevR1 = _r1;
            _r1 = sf * _iR1;
        }

        /** Scale section incrementaly, saving previous size if new section does'nt meet requrements
        *   
        */
        public override void IncrementSize(double sf)
        {
            _prevR1 = _r1;

            _r1 = sf * _r1;
        }

        /** Go to previous section size
        *   
        */
        public override void ApplyPreviousSection()
        {
            _r1 = _prevR1;
        }

        /** ..
        *   
        */
        public override void GetXY(ref double x, ref double y)
        {
            x = _r1;
            y = _r2;
        }


        #endregion
        //----------------------------------------------------------------------------------//

    }
}
