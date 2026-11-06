using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structures
{   
    //ENUMS

    public enum eAnalysis
    {
        DR,
        Matrix,
        Sizing
    };

    public enum eDRMode
    { 
        Elastic,
        Force
    }

    public enum eLoadType
    {
        PointLoad,
        LineLoad,
        AreaLoad
    }

    public enum eCrossSection 
    { 
        Circular_Solid,
        Circualr_Hollow,
        Rectangular_Solid,
        Rectangular_Hollow,
        I
    };

    public enum eConstraint
    {
        Free,
        Pined,
        Fixed,
        Curve,
    };


    public enum eRelease
    {
        none,
        torsion,
        rot_X,
        rot_Y,
        rot_X_and_Torsion,
        rot_Y_and_Torsion
    };

    public enum eElementType
    { 
        Bar,
        Cable,
        Beam,
        SplineB,
        SplineBT
    };

    public enum eOrientation
    { 
        vertical,
        normal
    };

    public enum eState
    {  
        NotReady,
        Ready,
        Converged,
    };


    public enum eSizingMode 
    {
        Direct,
        Incremental,
    }

    public enum eColorBy
    {
        Utilisation,
        UtilisationBending,
        UtilisationAxial,
        Moment_X,
        Moment_Y,
        Twist,
        Force,
        None,
    }

    public enum eSplineType
    {
        BendingSpline,
        BendingTorsionSpline
    }

    public enum eBeamMode
    {
        PropertyControl,
        ForceMomentControl,
        LimitControl
    }

}
