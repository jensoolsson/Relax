using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Structures;

namespace GH_Structures
{
    static class ARD_Util
    {


        static public bool IsSame(Parameters prevP, Parameters newP)
        {
            if (newP.TimeStep == prevP.TimeStep &&
                newP.ViscousDamping == prevP.ViscousDamping &&
                newP.GravityMagnitude == prevP.GravityMagnitude &&
                VectorCompare(newP.GravityVector, prevP.GravityVector) &&
                newP.SurfaceLoadMagnitude == prevP.SurfaceLoadMagnitude &&
                VectorCompare(newP.SurfaceLoadVector, prevP.SurfaceLoadVector) &&
                newP.NrIterations == prevP.NrIterations)
                return true;

            return false;
        }


        static public bool IsSame(Structure prevS, Structure newS) 
        {
            if (prevS.GetVertices().Count == newS.GetVertices().Count && 
                prevS.GetEdges().Count == newS.GetEdges().Count) 
            {
                //Compare vertex positions, if not the same, lenghts will have changed and structure has to be recomputed..
                //if(IsVertexPositionSame(prevS.GetVertices(), newS.GetVertices()))
                //{
                    return true;             
                //}
            }
            return false;
        }

        static private bool IsVertexPositionSame(List<Vertex> vs1, List<Vertex> vs2)
        {
            double tol = 0.001;
            for (int i = 0; i < vs1.Count; i++) 
            {
                if (vs1[i].Point.DistanceTo(vs2[i].Point) > tol) 
                {
                    return false;                 
                }
            }
            return true;
        }


        static public bool VectorCompare(Vector3d v1, Vector3d v2)
        {
            if (v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z)
                return true;

            return false;
        }


        static public List<Grasshopper.Kernel.Types.GH_ObjectWrapper> WrapObjects<T>(List<T> objs)
        {
            List<Grasshopper.Kernel.Types.GH_ObjectWrapper> wrapList = new List<Grasshopper.Kernel.Types.GH_ObjectWrapper>();
            for (int i = 0; i < objs.Count; i++)
            {
                Grasshopper.Kernel.Types.GH_ObjectWrapper ghWrap = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
                ghWrap.Value = objs[i];
                wrapList.Add(ghWrap);
            }
            return wrapList;
        }

    }
}
