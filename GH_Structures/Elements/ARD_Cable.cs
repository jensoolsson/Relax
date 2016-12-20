using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Structures;

namespace GH_Structures
{
    public class ARD_Cable : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ARD_Cable()
            : base("ARD_Cable", "Cable", "6 DOF Cable element (can only take tension)", "F+P", "Melax")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //00
            pManager.AddLineParameter("Line", "L", "Center lines for the elements", GH_ParamAccess.list);

            //01
            pManager.AddNumberParameter("Stiffness", "S", "Scale factor for the stiffness of the element", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //00
            pManager.AddGenericParameter("Cable", "E", "Cable element", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Line> lines = new List<Line>();
            List<Cable> cables = new List<Cable>();
            Cable tempC = new Cable();
            Node3Dof nS, nE;

            if (!DA.GetDataList(0, lines)) { return; }

            foreach (Line l in lines)
            {
                if (l.Length > 0.0001)
                {
                    nS = new Node3Dof(l.From);
                    nE = new Node3Dof(l.To);
                    tempC = new Cable(nS, nE);
                    cables.Add(tempC);
                }
            }

            DA.SetDataList(0, cables);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return GH_Structures.Properties.Resources.Cable_24x24;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{4D7AD522-0CA2-46C9-97F8-EFEA372DE2D1}"); }
        }
    }
}
