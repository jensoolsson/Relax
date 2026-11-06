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
    public class Main : IDisposable
    {
        public static Mutex mutex = new Mutex();

        private const int PreviewIntervalMs = 40;
        private const int StopTimeoutMs = 5000;

        protected bool _sizing;
        protected int _sizingCounter;
        protected int _sizingLimit;
        protected int _drCounter;
        protected int _drLimit;

        protected Structure _structure;
        protected AnalysisEngine _analysisEngine;
        protected Conduit _conduit;
        protected Parameters _p;

        protected Thread _analysisThread;
        protected volatile bool _stopRequested;     //Set from the UI thread, polled by the analysis thread.

        protected System.Threading.Timer _previewTimer;
        private int _previewBusy;                   //1 while a preview tick is running, so ticks never overlap.
        private volatile bool _refreshRequested;    //One-off preview update requested by the UI.
        private bool _disposed;

        //-------------------- Constructor and Initialisation functions --------------------//
        #region

        public void Initialise()
        {
            _sizing = false;
            _sizingCounter = 0;
            _sizingLimit = 10;
            _drCounter = 0;
            _drLimit = 5000;
            _stopRequested = false;
            _refreshRequested = false;
        }

        /** The preview timer runs for the lifetime of Main, but only rebuilds geometry while an
        *   analysis is running or when a refresh has been requested (see UpdatePreviewTimer_Tick).
        */
        public void InitialiseTimer()
        {
            _previewTimer = new System.Threading.Timer(UpdatePreviewTimer_Tick, null, PreviewIntervalMs, PreviewIntervalMs);
        }

        public Main()
        {
            Initialise();
            InitialiseTimer();
        }

        public Main(Parameters p)
        {
            Initialise();

            _p = p;
            _structure = new Structure();
            _analysisEngine = new AnalysisEngine(_structure);
            _conduit = new Conduit(_structure, _p);
            _conduit.Enabled = true;

            InitialiseTimer();
        }

        //Constructor when used as library.
        public Main(Structure structure, Parameters p)
        {
            Initialise();

            _p = p;
            _structure = structure;
            _analysisEngine = new AnalysisEngine(structure);
            _conduit = new Conduit(structure, p);
            _conduit.Enabled = true;

            InitialiseTimer();
        }

        #endregion
        //----------------------------------------------------------------------------------//


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

        public bool IsRunning
        {
            get { return _analysisThread != null && _analysisThread.IsAlive; }
        }

        public void EnableDrawing()
        {
            _conduit.Enabled = true;
            RequestRefresh();
        }

        public void DisableDrawing()
        {
            _conduit.Enabled = false;
            _conduit.Update();
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //-------------------------------- StartUp Process ---------------------------------//
        #region

        public void CreateObjects(List<RhinoObject> lines, List<RhinoObject> pts)
        {
            StopThread();

            //The structure becomes drawable part way through, so keep the preview out until it is complete.
            mutex.WaitOne();
            try
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
            finally
            {
                mutex.ReleaseMutex();
            }

            RequestRefresh();
        }

        public void Rewind()
        {
            StopThread();

            mutex.WaitOne();
            try
            {
                _structure.Rewind();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            _drCounter = 0;
            _sizingCounter = 0;
            RequestRefresh();
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------ Structural Analysis -------------------------------//
        #region

        public void RunDR()
        {
            StartThread(DR_Thread);
        }

        /** Runs DR until stopped, so that loads can be changed from the UI while it runs.
        *
        */
        private void DR_Thread()
        {
            _drCounter = 0;
            bool reportedConvergence = false;
            _analysisEngine.CalcConvergenceForce(_p);

            while (!_stopRequested)
            {
                bool converged;

                mutex.WaitOne();
                try
                {
                    _analysisEngine.RunDR(_p);
                    converged = _analysisEngine.IsConverged_DR;
                }
                catch (Exception e)
                {
                    WriteLine("DR stopped after an error: " + e.Message);
                    return;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
                _drCounter++;

                if (converged && !reportedConvergence)
                {
                    WriteLine("DR Converged at iteration: " + _drCounter.ToString());
                    reportedConvergence = true;
                }
            }
        }

        public void RunSizing()
        {
            StartThread(Sizing_Thread);
        }

        private void Sizing_Thread()
        {
            _sizing = true;
            _sizingCounter = 0;
            _analysisEngine.CalcConvergenceForce(_p);

            try
            {
                while (!_stopRequested && _sizingCounter < _sizingLimit)
                {
                    _drCounter = 0;
                    bool converged = false;

                    while (!_stopRequested && !converged && _drCounter < _drLimit)
                    {
                        mutex.WaitOne();
                        try
                        {
                            _analysisEngine.RunDR(_p);
                            converged = _analysisEngine.IsConverged_Sizing;
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                        }
                        _drCounter++;
                    }

                    if (_stopRequested)
                        break;

                    WriteLine("DR in sizing loop converged at iteration: " + _drCounter.ToString());

                    mutex.WaitOne();
                    try
                    {
                        _analysisEngine.RunSizing(_p);
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                    _sizingCounter++;
                }
                WriteLine(_stopRequested ? "Sizing stopped" : "Sizing done");
            }
            catch (Exception e)
            {
                WriteLine("Sizing stopped after an error: " + e.Message);
            }
            finally
            {
                _sizing = false;
            }
        }

        public void StopDR()
        {
            StopThread();
        }

        public void StopSizing()
        {
            StopThread();
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------------- Drawing --------------------------------------//
        #region

        /** Ask for one preview update, e.g. after the geometry or a draw setting changed while
        *   no analysis is running.
        */
        public void RequestRefresh()
        {
            _refreshRequested = true;
        }

        private void UpdatePreviewTimer_Tick(object state)
        {
            if (_structure == null) { return; }

            if (!IsRunning && !_refreshRequested) { return; }

            //Skip this tick if the previous one is still updating the geometry.
            if (Interlocked.Exchange(ref _previewBusy, 1) == 1) { return; }

            try
            {
                _refreshRequested = false;
                UpdateGeometry();
            }
            finally
            {
                Interlocked.Exchange(ref _previewBusy, 0);
            }
        }

        private void UpdateGeometry()
        {
            if (!_structure.IsDrawable)
                return;

            try
            {
                mutex.WaitOne();
                try
                {
                    _structure.UpdateGeometry(_p);

                    //Feed values back from calculations to the UI.
                    _p.OutOfBalanceForce = _analysisEngine.OutOfBalanceForce;
                    _p.OutOfBalanceMoment = _analysisEngine.OutOfBalanceMoment;
                    _p.ConvergenceForce = _analysisEngine.ConvergenceForce;
                    _p.StructuralWeight = _structure.StructuralWeight;
                    _p.DRCounter = _drCounter;
                    _p.SizingCounter = _sizingCounter;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }

                _conduit.Update();
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
            }
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //---------------------------------- Threading --------------------------------------//
        #region

        private void StartThread(ThreadStart work)
        {
            StopThread();

            _stopRequested = false;
            _analysisThread = new Thread(work);
            _analysisThread.IsBackground = true;    //Never keep Rhino alive on exit.
            _analysisThread.Start();
        }

        /** Ask the analysis thread to finish its current iteration and wait for it.
        *
        */
        public void StopThread()
        {
            if (_analysisThread == null)
                return;

            _stopRequested = true;

            if (!_analysisThread.Join(StopTimeoutMs))
                WriteLine("Analysis thread did not stop within " + (StopTimeoutMs / 1000).ToString() + " s");
            else
                _analysisThread = null;

            //Show the final state of the stopped analysis.
            RequestRefresh();
        }

        /** Write to the Rhino command line from any thread.
        *
        */
        private static void WriteLine(string message)
        {
            RhinoApp.InvokeOnUiThread(new Action(() => RhinoApp.WriteLine(message)));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            StopThread();

            if (_previewTimer != null)
                _previewTimer.Dispose();

            if (_conduit != null)
                DisableDrawing();
        }

        #endregion
        //----------------------------------------------------------------------------------//

        //------------------------------------ Other ---------------------------------------//
        #region

        public void Update(Structure structure, Parameters parameters)
        {
            _structure = structure;
            _p = parameters;

            _analysisEngine.Structure = structure;
        }

        public void UpdateLoad()
        {
            _analysisEngine.UpdateLoad(_p);
            RequestRefresh();
        }


        #endregion
        //----------------------------------------------------------------------------------//




    }
}
