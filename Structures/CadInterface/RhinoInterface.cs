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
    public class RhinoInterface
    {
        List<RhinoObject> _selRHObjs;
        List<RhinoObject> _selCrv;
        List<RhinoObject> _selLine;
        List<RhinoObject> _selPLine;
        List<RhinoObject> _selMesh;
        List<RhinoObject> _selPts;

        public void Initialise() 
        {
            _selRHObjs = new List<RhinoObject>();
            _selCrv = new List<RhinoObject>();
            _selLine = new List<RhinoObject>();
            _selPLine = new List<RhinoObject>();
            _selMesh = new List<RhinoObject>();
            _selPts = new List<RhinoObject>();
        }


        //-------------------------- Constructors and get methods --------------------------//
        #region

        public RhinoInterface()
        {
            Initialise();
        }

        public List<RhinoObject> GetLines() 
        {
            return _selLine;
        }
        
        public List<RhinoObject> GetPolyLines()
        {
            return _selPLine;
        }

        public List<RhinoObject> GetPoints()
        {
            return _selPts;
        }

        #endregion
        //----------------------------------------------------------------------------------//
        

        //---------------------------- Select and sort geometry ----------------------------//
        #region

        /** Select objects 
        * 
        */
        public void SelectObjects()
        {
            ObjectType geometryFilter = ObjectType.Curve | ObjectType.Point | ObjectType.Mesh;
            GetObject go = new GetObject();

            go.SetCommandPrompt("SelectGeometry");
            go.GeometryFilter = geometryFilter;
            go.GroupSelect = true;
            go.SubObjectSelect = false;
            go.EnableClearObjectsOnEntry(false);
            go.EnableUnselectObjectsOnExit(true);
            go.DeselectAllBeforePostSelect = false;
            go.EnablePreSelect(true, true);

            GetResult gr = go.GetMultiple(1, 0);

            for (int i = 0; i < go.ObjectCount; i++)
            {
                RhinoObject rhinoObject = go.Object(i).Object();

                _selRHObjs.Add(rhinoObject);

                if (null != rhinoObject)
                    rhinoObject.Select(false);
            }
            RhinoDoc.ActiveDoc.Views.Redraw();
        }


        /** Sort selected geometry based on type
        * 
        */
        public void SortGeometry()
        {
            for (int i = 0; i < _selRHObjs.Count; i++)
            {
                switch (_selRHObjs[i].ObjectType)
                {
                    case ObjectType.Curve:
                        if (IsLine(_selRHObjs[i]) || IsPolyLine(_selRHObjs[i]))
                            _selLine.Add(_selRHObjs[i]);
                        break;

                    case ObjectType.Point:
                        _selPts.Add(_selRHObjs[i]);
                        break;

                    case ObjectType.Mesh:
                        _selMesh.Add(_selRHObjs[i]);
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

        #endregion
        //----------------------------------------------------------------------------------//



        //-------------------------------- Bake geometry -----------------------------------//
        #region

        public void BakeMesh(Structure structure) 
        {
            Rhino.RhinoDoc.ActiveDoc.Objects.Add(structure.GetMesh());
        }

        public void BakeLines(Structure structure)
        {
            List<Line> lines = structure.GetLines();
            foreach (Line line in lines) 
                Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(line);
        }

        #endregion
        //----------------------------------------------------------------------------------//


    }
}
