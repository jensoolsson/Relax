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
using Structures;

namespace RH_Structures
{
    public class RHInterface
    {


        private RHGeometry _rhGeometry;


        //-------------------------- Constructors and get methods --------------------------//
        #region

        public RHInterface()
        {
            Initialise();
        }

        public void Initialise()
        {
            _rhGeometry = new RHGeometry();
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

                _rhGeometry.AddRhObj(rhinoObject);

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
            _rhGeometry.SortGeometry();
        }


        #endregion
        //----------------------------------------------------------------------------------//

        public List<RhinoObject> GetLines()
        {
            return _rhGeometry.GetLines();
        }

        public List<RhinoObject> GetPoints()
        {
            return _rhGeometry.GetPoints();
        }


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
