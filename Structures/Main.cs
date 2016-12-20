using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;

namespace Structures
{
    public class Main
    {
        public static Mutex mutex;

        protected bool _sizing;
        protected bool _dr;
        protected bool _matrix;
        protected int _sizingCounter;
        protected int _sizingLimit;
        protected int _drCounter;
        protected int _drLimit;
        protected eState _state;
        protected bool _isDarwable;

        protected Structure _structure;
        protected AnalysisEngine _analysisEngine;
        protected Conduit _conduit;
        protected Parameters _p;

        protected Thread _analysisThread;
        protected Thread _drawingThread;

        protected System.Windows.Forms.Timer _previewTimer;

        public void Initialise() 
        {
            _sizing = false;
            _dr = false;
            _matrix = false;
            _sizingCounter = 0;
            _sizingLimit = 10;
            _drCounter = 0;
            _drLimit = 5000;
            _state = eState.NotReady;
            _isDarwable = false;

            mutex = new Mutex();
            
        }

        public void InitialiseTimer() 
        {
            _previewTimer = new System.Windows.Forms.Timer();
            _previewTimer.Interval = 40;
            _previewTimer.Tick += new System.EventHandler(this.UpdatePreviewTimer_Tick);
            _previewTimer.Start();
        }


        public Main() 
        {
            Initialise();
            InitialiseTimer();
        }

        public Main(Parameters p)
        {
            Initialise();
            InitialiseTimer();
            
            _p = p;
            _structure = new Structure();
            _analysisEngine = new AnalysisEngine(_structure);
            _conduit = new Conduit(_structure, _p);
            _conduit.Enabled = true;
        }

        //Constructor when used as library, or as GH component.
        public Main(Structure structure, Parameters p)
        {
            Initialise();
            //InitialiseTimer();

            _p = p;
            _structure = structure;
            _analysisEngine = new AnalysisEngine(structure);
            _conduit = new Conduit(structure, p);
            //_conduit.Enabled = true;
        }

        public void Update(Structure structure, Parameters parameters) 
        {
            _structure = structure;
            _p = parameters;

            _analysisEngine.Structure = structure;
        }


        //---------------------------------- Properties ------------------------------------//
        #region

        public Structure Structure 
        {
            get { return _structure; }
            set 
            { 
                _structure = value;
                _analysisEngine.Structure = value;
                _conduit.Structure = value;
            }
        }

        public Parameters Parameters 
        {
            get { return _p; }
            set 
            { 
                _p = value;
                _conduit.Parameters = value;
            }
        }

        public void EnableDrawing() 
        {
            _conduit.Enabled = true;
        }

        public void DisableDrawing()
        {
            _conduit.Enabled = false;
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //-------------------------------- StartUp Process ---------------------------------//
        #region

        public void CreateObjects(List<RhinoObject> lines, List<RhinoObject> pts)
        {
            switch (_p.ElementType)
            {
                case eElementType.Beam:
                    _structure.CreateBeamsFromLines(lines);
                    break;
                case eElementType.Bar:
                    _structure.CreateBarsFromLines(lines);
                    break;
                case eElementType.Cable:
                    _structure.CreateCablesFromLines(lines);
                    break;
                case eElementType.SplineB:
                    _structure.CreateBSplinesFromLines(lines);
                    break;
                case eElementType.SplineBT:
                    _structure.CreateBTSplinesFromLines(lines);
                    break;
            }

            _structure.CreateConstraintObjects(pts);

            _structure.AssignConstPtsToVertices();
            
            _structure.PreprocessGeometry1();

            _structure.CreateFacesFromEdges();

            _structure.PreprocessGeometry2();

            _structure.PreprocessStructure(_p);

            _analysisEngine.Preprocess(_p);
        }

        public void Rewind() 
        {
            mutex.WaitOne();
            _structure.Rewind();
            mutex.ReleaseMutex();
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------ Structural Analysis -------------------------------//
        #region

        public void RunDR()
        {
            _state = eState.Ready;

            switch (_state)
            {
                case eState.Ready:
                    _analysisThread = new Thread(DR_Thread);
                    _analysisThread.Start();
                    break;

                default:
                    break;
            }
        }

        public void RunDR_GH()
        {
            bool converged = false;
            int apa = 0;
            
            for (int i = 0; i < _p.NrIterations; i++) 
            {
                _analysisEngine.RunDR(_p);
                converged = _analysisEngine.IsConverged_DR;
            }
        }


        private void DR_Thread()
        {
            _drCounter = 0;
            bool converged = false;
            _analysisEngine.CalcConvergenceForce(_p);

            while (!converged)
            {
                try
                {
                    mutex.WaitOne();
                    _analysisEngine.RunDR(_p);
                    converged = _analysisEngine.IsConverged_DR;
                    mutex.ReleaseMutex();
                    _drCounter++;
                }
                catch (Exception e)
                {
                    Rhino.RhinoApp.WriteLine(e.Message);
                }
            }
            RhinoApp.WriteLine("DR Converged at iteration: " + _drCounter.ToString());
        }

        public void RunSizing()
        {
            _state = eState.Ready;

            switch (_state)
            {
                case eState.Ready:
                    _analysisThread = new Thread(Sizing_Thread);
                    _analysisThread.Start();
                    break;

                default:
                    break;
            }
        }

        private void Sizing_Thread()
        {
            bool converged = false;
            _sizing = true;
            _sizingCounter = 0;
            _analysisEngine.CalcConvergenceForce(_p);

            while (_sizing && _sizingCounter < _sizingLimit)
            {
                _drCounter = 0;
                converged = false;

                while (!converged && _drCounter < _drLimit)
                {
                    mutex.WaitOne();
                    _analysisEngine.RunDR(_p);
                    converged = _analysisEngine.IsConverged_Sizing;
                    mutex.ReleaseMutex();
                    _drCounter++;
                }

                RhinoApp.WriteLine("DR in sizing loop converged at iteration: " + _drCounter.ToString());
                mutex.WaitOne();
                _analysisEngine.RunSizing(_p);
                mutex.ReleaseMutex();
                _sizingCounter++;
            }
            RhinoApp.WriteLine("Sizing done");
            _sizing = false;
        }

        public void StopDR() 
        {
            _dr = false;
            StopThread();
            _previewTimer.Stop();
        }

        public void StopSizing() 
        {
            _sizing = false;
            StopThread();
            _previewTimer.Stop();
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------------- Drawing --------------------------------------//
        #region

        private void UpdatePreviewTimer_Tick(object sender, EventArgs e)
        {
            if (_structure == null) { return; }

            if (_drawingThread == null || !_drawingThread.IsAlive)
            {
                _drawingThread = new Thread(UpdateGeometry);
                _drawingThread.Start();
            }
        }

        private void UpdateGeometry()
        {
            if (_structure.IsDrawable)
            {
                try
                {
                    mutex.WaitOne();
                    _structure.UpdateGeometry(_p);
                    mutex.ReleaseMutex();

                    //Feed values back from calculations to the UI.
                    mutex.WaitOne();
                    _p.OutOfBalanceForce = _analysisEngine.OutOfBalanceForce;
                    _p.OutOfBalanceMoment = _analysisEngine.OutOfBalanceMoment;
                    _p.ConvergenceForce = _analysisEngine.ConvergenceForce;
                    _p.StructuralWeight = _structure.StructuralWeight;
                    _p.DRCounter = _drCounter;
                    _p.SizingCounter = _sizingCounter;
                    mutex.ReleaseMutex();

                    _conduit.Update();
                }
                catch (Exception e)
                {
                    Rhino.RhinoApp.WriteLine(e.Message);
                }
            }
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //---------------------------------- Threading --------------------------------------//
        #region

        public void StopThread()
        {
            if (_analysisThread != null)
                if (_analysisThread.Join(300))
                    _analysisThread.Abort();
        }

        #endregion
        //----------------------------------------------------------------------------------//





    }
}
