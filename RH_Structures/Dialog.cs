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
using Structures;


namespace RH_Structures
{
    public partial class Dialog : Form
    {
        private RHInterface _rhInterface;
        private Main _main;
        private Parameters _p;
        protected System.Windows.Forms.Timer _UI_Timer;
        private double _scrollValue;

        public Dialog()
        {
            InitializeComponent();
            _rhInterface = new RHInterface();
            _p = new Parameters();
            _main = new Main(_p);
            _UI_Timer = new Timer();
            
            InitialiseTimer();
            PopulateCmb();
            _scrollValue = 1;
            tbxLoadSF.Text = _scrollValue.ToString();
            tbxLoad.Text = "0";

            ActivateBeamSettings(false);

            tbxTransStiff.Text = _p.TranslationalStiffnessScaleFactor.ToString();
            tbxRotStiff.Text = _p.RotationalStiffnessScaleFactor.ToString();

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
        }

        private void UpdateLoad()
        {
            _main.UpdateLoad();
        }


        //---------------------------------- Properties ------------------------------------//
        #region

        public RHInterface RhinoInterface 
        {
            get { return _rhInterface; }
            set { _rhInterface = value; }
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
            _rhInterface.SelectObjects();
            _rhInterface.SortGeometry();
            _main.CreateObjects(_rhInterface.GetLines(), _rhInterface.GetPoints());
        }

        private void btnCreateObjects_Click(object sender, EventArgs e)
        {
            //_main.CreateObjects(_rhInterface.GetLines(), _rhInterface.GetPoints());
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
            _rhInterface.BakeLines(_main.Structure);
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
            _p.DrawUtilisation = cbxDrawUtilisation.Checked;
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
            double value = (double)sldbrLoadSF.Value;
            _p.LoadScaleFactor = value;
            tbxLoadSF.Text = value.ToString();
        }


        private void sldbrSrfLoad_Scroll(object sender, EventArgs e)
        {
            double value = (double)sldbrSrfLoad.Value;
            _p.SurfaceLoadMagnitude = value;
            tbxLoad.Text = value.ToString();
        }

        private void Dialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _main.Structure.IsDrawable = false;
            _main.DisableDrawing();
        }

        private void btnRewind_Click(object sender, EventArgs e)
        {
            _main.Rewind();
        }
        
        private void rbtnArea_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnArea.Checked)
            {
                _p.LoadType = eLoadType.AreaLoad;
                rbtnLine.Checked = false;
                rbtnPoint.Checked = false;
                UpdateLoad();
            }
        }

        private void rbtnLine_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnLine.Checked)
            {
                _p.LoadType = eLoadType.LineLoad;
                rbtnArea.Checked = false;
                rbtnPoint.Checked = false;
                UpdateLoad();
            }
        }

        private void rbtnPoint_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnPoint.Checked)
            {
                _p.LoadType = eLoadType.PointLoad;
                rbtnArea.Checked = false;
                rbtnLine.Checked = false;
                UpdateLoad();
            }
        }

        private void cbxSelfWeight_CheckedChanged(object sender, EventArgs e)
        {
            _p.IsSelfWeight = cbxSelfWeight.Checked;
        }

        private void tbxLoad_TextChanged(object sender, EventArgs e)
        {
            string textValue = tbxLoad.Text;
            double prevValue = sldbrSrfLoad.Value;

            if (Util.IsValidNumericalInput(textValue, (double)sldbrSrfLoad.Minimum, (double)sldbrSrfLoad.Maximum))
            {
                double dValue = double.Parse(textValue);
                sldbrSrfLoad.Value = (int)(dValue);
                _p.SurfaceLoadMagnitude = dValue;
            }
            else
                tbxLoad.Text = prevValue.ToString();
        }

        private void cbxNodeMass_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawNodeMass = cbxNodeMass.Checked;
        }

        private void cbxNodeLoad_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawNodeLoad = cbxNodeLoad.Checked;
        }

        private void rbtnPC_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnPC.Checked)
            {
                _p.BeamMode = eBeamMode.PropertyControl;
                rbtnFMC.Checked = false;
                rbtnLC.Checked = false;
            }
        }

        private void rbtnFMC_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnFMC.Checked)
            {
                _p.BeamMode = eBeamMode.ForceMomentControl;
                rbtnPC.Checked = false;
                rbtnLC.Checked = false;
            }
        }

        private void rbtnLC_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnLC.Checked)
            {
                _p.BeamMode = eBeamMode.LimitControl;
                rbtnPC.Checked = false;
                rbtnFMC.Checked = false;
            }
        }

        private void sldbrAxial_Scroll(object sender, EventArgs e)
        {
            double value = (double)sldbrAxial.Value;
            _p.AxialSF = value /100.0;
            tbxAxial.Text = value.ToString();
        }

        private void sldbrBending_Scroll(object sender, EventArgs e)
        {
            double value = (double)sldbrBending.Value;
            _p.BendingSF = value /100.0;
            tbxBending.Text = value.ToString();
        }


        public void ActivateBeamSettings(bool b)
        {
            sldbrAxial.Enabled = b;
            sldbrBending.Enabled = b;
            tbxBending.Enabled = b;
            tbxAxial.Enabled = b;
            lblAxial.Enabled = b;
            lblBending.Enabled = b;
            rbtnPC.Enabled = b;
            rbtnLC.Enabled = b;
            rbtnFMC.Enabled = b;

            if (b == true)
            {
                tbxAxial.Text = sldbrAxial.Value.ToString();
                tbxBending.Text = sldbrBending.Value.ToString();
            }
            else
            {
                tbxAxial.Text = "";
                tbxBending.Text = "";
            }
        }

        private void cbxActiveBeam_CheckedChanged(object sender, EventArgs e)
        {
            _p.ModeControl = cbxActiveBeam.Checked;
            ActivateBeamSettings(_p.ModeControl);
        }

        private void btnFlipLoad_Click(object sender, EventArgs e)
        {
            _p.GravityVector = -1 * _p.GravityVector;
            _p.SurfaceLoadVector = -1 * _p.SurfaceLoadVector;
            UpdateLoad();
        }

        private void cbxLoadArrow_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawLoadArrows = cbxLoadArrow.Checked;
        }

        private void sldbrTransStiff_Scroll(object sender, EventArgs e)
        {
            double value = (double)sldbrTransStiff.Value;
            _p.TranslationalStiffnessScaleFactor = value;
            tbxTransStiff.Text = value.ToString();
        }

        private void sldbrRotStiff_Scroll(object sender, EventArgs e)
        {
            double value = (double)sldbrRotStiff.Value;
            _p.RotationalStiffnessScaleFactor = value;
            tbxRotStiff.Text = value.ToString();
        }

        private void cbxNodeTS_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawNodeTS = cbxNodeTS.Checked;
        }

        private void cbxNodeRS_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawNodeRS = cbxNodeRS.Checked;
        }

        private void cbxElementTS_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawElementTS = cbxElementTS.Checked;
        }

        private void cbxElementRS_CheckedChanged(object sender, EventArgs e)
        {
            _p.DrawElementRS = cbxElementRS.Checked;
        }





        #endregion
        //----------------------------------------------------------------------------------//

    }
}
