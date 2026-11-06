using System;
using System.Collections.Generic;
using Rhino;
using Structures;

namespace RH_Structures
{
    /** Holds the analysis state (Main and Parameters) for the lifetime of the plug-in.
    *   Rhino on Mac creates and destroys panels per document, so this state must not live
    *   in the panel itself.
    */
    public class RelaxSession : IDisposable
    {
        private readonly Parameters _p;
        private Main _main;


        //-------------------------- Constructors and get methods --------------------------//
        #region

        public RelaxSession()
        {
            _p = new Parameters();
        }

        public Parameters Parameters
        {
            get { return _p; }
        }

        public Main Main
        {
            get
            {
                if (_main == null)
                    _main = new Main(_p);
                return _main;
            }
        }

        public bool HasStructure
        {
            get { return _main != null && _main.Structure.IsDrawable; }
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------------ Actions -------------------------------------//
        #region

        /** Let the user pick lines and constraint points, and build a new structure from them.
        *   Must run inside a command (see RHSelectGeometryCommand), since it uses GetObject.
        */
        public bool SelectGeometry()
        {
            RHInterface rhInterface = new RHInterface();     //Fresh selection, so earlier picks are not reused.
            rhInterface.SelectObjects();
            rhInterface.SortGeometry();

            if (rhInterface.GetLines().Count == 0)
            {
                RhinoApp.WriteLine("Relax: no lines or polylines selected.");
                return false;
            }

            Main.StopThread();
            Main.Structure = new Structure();               //Replace any previous model.
            Main.CreateObjects(rhInterface.GetLines(), rhInterface.GetPoints());
            Main.EnableDrawing();

            RhinoApp.WriteLine("Relax: structure created from " + rhInterface.GetLines().Count + " curve(s) and " + rhInterface.GetPoints().Count + " constraint point(s).");
            return true;
        }

        public void RunDR()
        {
            if (!CheckStructure())
                return;
            Main.RunDR();
        }

        public void RunSizing()
        {
            if (!CheckStructure())
                return;
            Main.RunSizing();
        }

        public void Stop()
        {
            if (_main != null)
                _main.StopThread();
        }

        public void Rewind()
        {
            if (!CheckStructure())
                return;
            Main.Rewind();
        }

        public void Bake(RhinoDoc doc)
        {
            if (!CheckStructure())
                return;

            Main.mutex.WaitOne();
            List<Rhino.Geometry.Line> lines;
            try
            {
                lines = Main.Structure.GetLines();
            }
            finally
            {
                Main.mutex.ReleaseMutex();
            }

            foreach (Rhino.Geometry.Line line in lines)
                doc.Objects.AddLine(line);
            doc.Views.Redraw();
        }

        /** Load type or direction changed, so node loads must be recalculated.
        *
        */
        public void UpdateLoad()
        {
            if (_main != null)
                _main.UpdateLoad();
        }

        /** A draw setting changed; update the preview even if no analysis is running.
        *
        */
        public void Refresh()
        {
            if (_main != null)
                _main.RequestRefresh();
        }

        public void ShowPreview()
        {
            if (_main != null)
                _main.EnableDrawing();
        }

        /** Stop the analysis and hide the preview, e.g. when the panel is closed.
        *
        */
        public void StopAndHide()
        {
            if (_main == null)
                return;

            _main.StopThread();
            _main.DisableDrawing();
        }

        private bool CheckStructure()
        {
            if (HasStructure)
                return true;

            RhinoApp.WriteLine("Relax: select geometry first.");
            return false;
        }

        public void Dispose()
        {
            if (_main != null)
                _main.Dispose();
            _main = null;
        }

        #endregion
        //----------------------------------------------------------------------------------//
    }
}
