using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;


namespace Structures
{
    public partial class Dialog : Form
    {
        private RhinoInterface _rhinoInterface;
        private Main _main;
        private Parameters _p;
        protected System.Windows.Forms.Timer _UI_Timer;
        private double _scrollValue;

        public Dialog()
        {
            InitializeComponent();
            _rhinoInterface = new RhinoInterface();
            _p = new Parameters();
            _main = new Main(_p);
            _UI_Timer = new Timer();
            InitialiseTimer();
            PopulateCmb();
            _scrollValue = 1;
            tbxLoadSF.Text = _scrollValue.ToString();
        }

        public void InitialiseTimer()
        {
            _UI_Timer = new System.Windows.Forms.Timer();
            _UI_Timer.Interval = 40;
            _UI_Timer.Tick += new System.EventHandler(this.UpdateUITimer_Tick);
            _UI_Timer.Start();
        }

        private void UpdateUITimer_Tick(object sender, EventArgs e)
        {
            tbxConvergForce.Text = Math.Round(_p.ConvergenceForce, 3).ToString();
            tbxOBF.Text = Math.Round(_p.OutOfBalanceForce, 3).ToString();
            tbxOBM.Text = Math.Round(_p.OutOfBalanceMoment, 3).ToString();
            tbxStructuralWeight.Text = Math.Round(_p.StructuralWeight, 2).ToString();
            tbxDRCount.Text = _p.DRCounter.ToString();
            tbxSizingCount.Text = _p.SizingCounter.ToString();
            _p.LoadScaleFactor = _scrollValue;
        }


        //---------------------------------- Properties ------------------------------------//
        #region

        public RhinoInterface RhinoInterface 
        {
            get { return _rhinoInterface; }
            set { _rhinoInterface = value; }
        }

        public Main Main 
        {
            get { return _main; }
            set { _main = value; }
        }

        public Parameters Parameters 
        {
            get { return _p; }
            set { _p = value; }
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------------- Buttons --------------------------------------//
        #region

        private void PopulateCmb() 
        {
            foreach (eColorBy c in Enum.GetValues(typeof(eColorBy)))
                cmbColorBy.Items.Add(c.ToString());

            cmbColorBy.SelectedIndex = 0;
        }
        
        private void cmbColorBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            _p.ColorBy = (eColorBy)cmbColorBy.SelectedIndex;
        }

        private void btnSelectGeometry_Click(object sender, EventArgs e)
        {
            _rhinoInterface.SelectObjects();
            _rhinoInterface.SortGeometry();
        }

        private void btnCreateObjects_Click(object sender, EventArgs e)
        {
            _main.CreateObjects(_rhinoInterface.GetLines(), _rhinoInterface.GetPoints());
        }

        private void btnRunDR_Click(object sender, EventArgs e)
        {
            _main.RunDR();
        }

        private void btnSizing_Click(object sender, EventArgs e)
        {
            //_main.StopDR();
            _main.RunSizing();
        }

        private void btnBake_Click(object sender, EventArgs e)
        {
            //_rhinoInterface.BakeMesh(_main.Structure);
            _rhinoInterface.BakeLines(_main.Structure);
        }

        private void cbxShowVertexNormals_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawVertexNormals = cbxShowVertexNormals.Checked;
        }

        private void cbxShowFaceNormals_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawFaceNormals = cbxShowFaceNormals.Checked;
        }

        private void cbxDrawVertices_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawNodes = cbxDrawVertices.Checked;
        }

        private void cbxDrawLines_CheckStateChanged(object sender, EventArgs e)
        {
            _p.DrawLines = cbxDrawLines.Checked;
        }

        private void cbxDrawMeshWire_CheckedChanged(object sender, EventArgs e)
        {
            //_p.DrawMeshWires = cbxDrawMeshWire.Checked;
        }

        private void cbxDrawMeshShaded_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawMeshShaded = cbxDrawMeshShaded.Checked;
        }

        private void cbxDrawNodeRing_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawNodeRing = cbxDrawNodeRing.Checked;
        }

        private void cbxDraw3dBar_CheckedChanged(object sender, EventArgs e)
        {
            _p.Draw3dBars = cbxDraw3dBar.Checked;        
        }

        private void cbxDrawNodeCS_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawNodeCS = cbxDrawNodeCS.Checked;
        }

        private void cbxDrawBarCS_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawBarCS = cbxDrawBarCS.Checked;
        }

        private void cbxDrawVertexID_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawVertexID = cbxDrawVertexID.Checked;
        }

        private void cbxDrawEdgeID_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawEdgeID = cbxDrawEdgeID.Checked;
        }

        private void cbxDrawFaceID_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawFaceID = cbxDrawFaceID.Checked;
        }

        private void cbxDrawDebug_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawDebug = cbxDrawDebug.Checked;
        }

        private void rbtn0100_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtn0100.Checked)
            { 
                _p.ShowMin = 0;
                _p.ShowMax = 100;
                rbtnMinMax.Checked = false;
            }
        }

        private void rbtnMinMax_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnMinMax.Checked)
            {
                Main.mutex.WaitOne();
                _p.ShowMin = _p.RealMin;
                _p.ShowMax = _p.RealMax;
                Main.mutex.ReleaseMutex();

                rbtn0100.Checked = false;
            }
        }

        private void rbtnBeams_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnBeams.Checked) 
            {
                _p.ElementType = eElementType.Beam;
                rbtnBars.Checked = false;
                rbtnCables.Checked = false;
                rbtnBSplines.Checked = false;
                rbtnBTSplines.Checked = false;
            }
        }

        private void rbtnBars_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnBars.Checked)
            {
                _p.ElementType = eElementType.Bar;
                rbtnBeams.Checked = false;
                rbtnCables.Checked = false;
                rbtnBSplines.Checked = false;
                rbtnBTSplines.Checked = false;
            }
        }

        private void rbtnCables_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnCables.Checked)
            {
                _p.ElementType = eElementType.Cable;
                rbtnBeams.Checked = false;
                rbtnBars.Checked = false;
                rbtnBSplines.Checked = false;
                rbtnBTSplines.Checked = false;
            }
        }

        private void rbtnSplines_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnBSplines.Checked)
            {
                _p.ElementType = eElementType.SplineB;
                rbtnBars.Checked = false;
                rbtnCables.Checked = false;
                rbtnBeams.Checked = false;
                rbtnBTSplines.Checked = false;
            }
        }

        private void rbtnBTSplines_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnBTSplines.Checked)
            {
                _p.ElementType = eElementType.SplineBT;
                rbtnBars.Checked = false;
                rbtnCables.Checked = false;
                rbtnBeams.Checked = false;
                rbtnBSplines.Checked = false;
            }
        }

        private void sldbrLoadSF_Scroll(object sender, EventArgs e)
        {
            _scrollValue = (double)sldbrLoadSF.Value;
            tbxLoadSF.Text = _scrollValue.ToString();
        }

        private void Dialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _main.Structure.IsDrawable = false;
            _main.DisableDrawing();
        }

        #endregion

        private void btnRewind_Click(object sender, EventArgs e)
        {
            _main.Rewind();
        }
        //----------------------------------------------------------------------------------//

    }
}
