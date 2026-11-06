using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;

namespace Structures
{
    public class Node : Vertex
    {

        protected eConstraint _constraint;              //Constraint status



        private new void Initialise()
        {


        }
        

        public Node()
        {
            Initialise();
        }

        public Node(Point3d pt) : base(pt)
        {
            Initialise();
        }


        public eConstraint Constraint
        {
            get { return _constraint; }
            set { _constraint = value; }
        }


        public virtual void CalcFictiousMass(double dt, double transStiffSF, double rotStiffSF){}

        public virtual void TransformDueToStrain(double dt, double vd, double loadSF, bool isSelfWeight, double surfaceLoadMagnitude) { }

        public virtual void Clear() { }

        public virtual void CalcStabilityLimit() { }

        public virtual void InitialiseCoordSystem() { }

        public virtual void ClearLoad() { }

        public virtual void ApplyLoadFromSurface(double loadPerSquareMeter) { }


    }
}
