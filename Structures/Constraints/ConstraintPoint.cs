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
    public class ConstraintPoint
    {
        protected Point3d _pt;
        protected eConstraint _type;
        protected int _id;

        public void Initialse() 
        {
            _pt = new Point3d(0,0,0);
            _id = -1;
        }

        public ConstraintPoint(Point3d pt, int id) 
        {
            Initialse();
            _type = eConstraint.Pined;
            _pt = pt;
            _id = id;
        }

        public ConstraintPoint(Point3d pt, eConstraint cType)
        {
            Initialse();
            _type = cType;
            _pt = pt;
        }

        public Point3d Point 
        {
            get { return _pt; }
            set { _pt = value; }
        }

        public eConstraint Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
    }
}
