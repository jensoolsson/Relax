using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;
using Structures;

namespace RH_Structures
{

    class RHGeometry
    {

        //private List<Line> _lines;
        //private List<Curve> _curves;
        //private List<Point3d> _points;
        //private List<Mesh> _mesh;

        private List<RhinoObject> _refRHObjs;
        private List<RhinoObject> _refCrv;
        private List<RhinoObject> _refLine;
        private List<RhinoObject> _refPLine;
        private List<RhinoObject> _refMesh;
        private List<RhinoObject> _refPts;

        public RHGeometry()
        {
            Initialise();
        }

        public void Initialise()
        {
            //_lines = new List<Line>();
            //_curves = new List<Curve>();
            //_points = new List<Point3d>();
            //_mesh = new List<Mesh>();

            _refRHObjs = new List<RhinoObject>();
            _refCrv = new List<RhinoObject>();
            _refLine = new List<RhinoObject>();
            _refPLine = new List<RhinoObject>();
            _refMesh = new List<RhinoObject>();
            _refPts = new List<RhinoObject>();
        }

        //-------------------------------- Get and Set methods -----------------------------//
        #region
        public List<RhinoObject> GetLines()
        {
            return _refLine;
        }

        public List<RhinoObject> GetPolyLines()
        {
            return _refPLine;
        }

        public List<RhinoObject> GetPoints()
        {
            return _refPts;
        }

        public void AddRhObj(RhinoObject rhObj)
        {
            _refRHObjs.Add(rhObj);
        }

        public void AddRhObjs(List<RhinoObject> rhObjs)
        {
            _refRHObjs.AddRange(rhObjs);
        }

        public RhinoObject GetRhObj(int index)
        {
            return _refRHObjs[index];
        }

        public List<RhinoObject> GetRhObjs()
        {
            return _refRHObjs;
        }


        //public List<Line> Lines
        //{
        //    get { return _lines; }
        //    set { _lines = value; }
        //}

        //public List<Curve> Curves
        //{
        //    get { return _curves; }
        //    set { _curves = value; }
        //}

        //public List<Point3d> Points
        //{
        //    get { return _points; }
        //    set { _points = value; }
        //}

        //public List<Mesh> Meshes
        //{
        //    get { return _mesh; }
        //    set { _mesh = value; }
        //}


        //public void AddLines(List<Line> inputLines)
        //{
        //    _lines.AddRange(inputLines);
        //}

        //public void AddLine(Line line)
        //{
        //    _lines.Add(line);
        //}

        //public void AddCurves(List<Curve> inputCurves)
        //{
        //    _curves.AddRange(inputCurves);
        //}

        //public void AddCurve(Curve curve)
        //{
        //    _curves.Add(curve);
        //}

        //public void AddPoints(List<Point3d> inputPoints)
        //{
        //    _points.AddRange(inputPoints);
        //}

        //public void AddPoints(Point3d point)
        //{
        //    _points.Add(point);
        //}

        //public void AddMeshes(List<Mesh> inputMeshes)
        //{
        //    _mesh.AddRange(inputMeshes);
        //}

        //public void AddMesh(Mesh mesh)
        //{
        //    _mesh.Add(mesh);
        //}

        #endregion
        //----------------------------------------------------------------------------------//

        public void SortGeometry()
        {

            for (int i = 0; i < _refRHObjs.Count; i++)
            {
                switch (_refRHObjs[i].ObjectType)
                {
                    case ObjectType.Curve:
                        if (IsLine(_refRHObjs[i]) || IsPolyLine(_refRHObjs[i]))
                            _refLine.Add(_refRHObjs[i]);
                        break;

                    case ObjectType.Point:
                        _refPts.Add(_refRHObjs[i]);
                        break;

                    case ObjectType.Mesh:
                        _refMesh.Add(_refRHObjs[i]);
                        break;
                }
            }
        }


        public bool IsPolyLine(RhinoObject ro)
        {
            Curve c = ro.Geometry as Curve;
            return c.IsPolyline();
        }

        public bool IsLine(RhinoObject ro)
        {
            Curve c = ro.Geometry as Curve;
            return c.IsLinear();
        }



    }
}
