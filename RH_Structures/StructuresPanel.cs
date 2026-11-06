using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.UI;
using Structures;

namespace RH_Structures
{
    /** Dockable Relax panel. Replaces the WinForms dialog from the Rhino 5 version.
    *   All state lives in RelaxSession; the panel only reads and writes Parameters.
    */
    [Guid("D6B52529-52AC-46D4-BE1B-4532FBF21BF1")]
    public class StructuresPanel : Panel, IPanel
    {
        private readonly RelaxSession _session;
        private readonly Parameters _p;
        private readonly UITimer _uiTimer;
        private bool _updating;                     //Guards slider <-> stepper feedback loops.

        //Beam settings, enabled only when "Active" is checked.
        private RadioButtonList _rblBeamMode;
        private Slider _sldAxial;
        private Slider _sldBending;
        private Label _lblAxial;
        private Label _lblBending;

        //Progress read-outs.
        private Label _lblDRCount;
        private Label _lblSizingCount;
        private Label _lblOBF;
        private Label _lblOBM;
        private Label _lblConvergForce;
        private Label _lblWeight;


        //-------------------------- Constructors and get methods --------------------------//
        #region

        public static Guid PanelId
        {
            get { return typeof(StructuresPanel).GUID; }
        }

        public StructuresPanel(uint documentSerialNumber)
        {
            _session = RHStructuresPlugIn.Instance.Session;
            _p = _session.Parameters;

            _uiTimer = new UITimer { Interval = 0.1 };
            _uiTimer.Elapsed += (sender, e) => UpdateProgress();

            Content = new Scrollable
            {
                Border = BorderType.None,
                Content = BuildLayout(),
            };
        }

        //Also tie the progress timer to the control's own lifetime, in case Rhino does not
        //call PanelShown/PanelHidden for a newly created panel.
        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
            _uiTimer.Start();
        }

        protected override void OnUnLoad(EventArgs e)
        {
            _uiTimer.Stop();
            base.OnUnLoad(e);
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------------ Layout --------------------------------------//
        #region

        private Control BuildLayout()
        {
            DynamicLayout layout = new DynamicLayout { Padding = new Padding(8), DefaultSpacing = new Size(4, 6) };

            layout.Add(Section("Geometry", BuildGeometrySection()));
            layout.Add(Section("Analysis", BuildAnalysisSection()));
            layout.Add(Section("Load", BuildLoadSection()));
            layout.Add(Section("Beam", BuildBeamSection()));
            layout.Add(Section("Stability control", BuildStabilitySection()));
            layout.Add(Section("Show", BuildShowSection()));
            layout.Add(Section("Progress", BuildProgressSection()));
            layout.Add(null);

            return layout;
        }

        private static GroupBox Section(string title, Control content)
        {
            return new GroupBox { Text = title, Padding = new Padding(6), Content = content };
        }

        private Control BuildGeometrySection()
        {
            RadioButtonList rblElementType = new RadioButtonList { Orientation = Orientation.Vertical };
            rblElementType.Items.Add("Beams");
            rblElementType.Items.Add("Bars");
            rblElementType.Items.Add("Cables");
            rblElementType.Items.Add("B_Splines");
            rblElementType.Items.Add("BT_Splines");
            rblElementType.SelectedIndex = ElementTypeToIndex(_p.ElementType);
            rblElementType.SelectedIndexChanged += (sender, e) =>
            {
                _p.ElementType = IndexToElementType(rblElementType.SelectedIndex);
            };

            Button btnSelect = new Button { Text = "Select geometry" };
            btnSelect.Click += (sender, e) => RhinoApp.RunScript("_" + RHSelectGeometryCommand.Instance.EnglishName, false);

            Button btnBake = new Button { Text = "Bake" };
            btnBake.Click += (sender, e) => RhinoApp.RunScript("_" + RHBakeCommand.Instance.EnglishName, false);

            DynamicLayout layout = new DynamicLayout { DefaultSpacing = new Size(4, 4) };
            layout.AddRow(new Label { Text = "Element type (set before selecting)" });
            layout.AddRow(rblElementType);
            layout.AddSeparateRow(btnSelect, btnBake, null);
            return layout;
        }

        private Control BuildAnalysisSection()
        {
            Button btnRunDR = new Button { Text = "Run DR" };
            btnRunDR.Click += (sender, e) => _session.RunDR();

            Button btnSizing = new Button { Text = "Sizing" };
            btnSizing.Click += (sender, e) => _session.RunSizing();

            Button btnStop = new Button { Text = "Stop" };
            btnStop.Click += (sender, e) => _session.Stop();

            Button btnRewind = new Button { Text = "Rewind" };
            btnRewind.Click += (sender, e) => _session.Rewind();

            DynamicLayout layout = new DynamicLayout { DefaultSpacing = new Size(4, 4) };
            layout.AddSeparateRow(btnRunDR, btnSizing, null);
            layout.AddSeparateRow(btnStop, btnRewind, null);
            return layout;
        }

        private Control BuildLoadSection()
        {
            CheckBox cbxSelfWeight = Check("Self weight", _p.IsSelfWeight, v => _p.IsSelfWeight = v);

            RadioButtonList rblLoadType = new RadioButtonList { Orientation = Orientation.Horizontal };
            rblLoadType.Items.Add("N/m2");
            rblLoadType.Items.Add("N/m");
            rblLoadType.Items.Add("N");
            rblLoadType.SelectedIndex = LoadTypeToIndex(_p.LoadType);
            rblLoadType.SelectedIndexChanged += (sender, e) =>
            {
                _p.LoadType = IndexToLoadType(rblLoadType.SelectedIndex);
                _session.UpdateLoad();
            };

            //Load magnitude: slider for quick changes, stepper for exact values.
            Slider sldLoad = new Slider { MinValue = 0, MaxValue = 20000, TickFrequency = 1000, Value = (int)_p.SurfaceLoadMagnitude };
            NumericStepper nsLoad = new NumericStepper { MinValue = 0, MaxValue = 20000, DecimalPlaces = 0, Value = _p.SurfaceLoadMagnitude };
            sldLoad.ValueChanged += (sender, e) =>
            {
                if (_updating) return;
                _updating = true;
                _p.SurfaceLoadMagnitude = sldLoad.Value;
                nsLoad.Value = sldLoad.Value;
                _updating = false;
            };
            nsLoad.ValueChanged += (sender, e) =>
            {
                if (_updating) return;
                _updating = true;
                _p.SurfaceLoadMagnitude = nsLoad.Value;
                sldLoad.Value = (int)nsLoad.Value;
                _updating = false;
            };

            Label lblLoadSF = new Label { Text = Format(_p.LoadScaleFactor) };
            Slider sldLoadSF = new Slider { MinValue = 0, MaxValue = 20, Value = (int)_p.LoadScaleFactor };
            sldLoadSF.ValueChanged += (sender, e) =>
            {
                _p.LoadScaleFactor = sldLoadSF.Value;
                lblLoadSF.Text = Format(_p.LoadScaleFactor);
            };

            Button btnFlip = new Button { Text = "Flip load" };
            btnFlip.Click += (sender, e) =>
            {
                _p.GravityVector = -1 * _p.GravityVector;
                _p.SurfaceLoadVector = -1 * _p.SurfaceLoadVector;
                _session.UpdateLoad();
            };

            DynamicLayout layout = new DynamicLayout { DefaultSpacing = new Size(4, 4) };
            layout.AddSeparateRow(cbxSelfWeight, null, btnFlip);
            layout.AddRow(rblLoadType);
            layout.AddSeparateRow(new Label { Text = "Load" }, nsLoad);
            layout.AddRow(sldLoad);
            layout.AddSeparateRow(new Label { Text = "Scale factor" }, lblLoadSF);
            layout.AddRow(sldLoadSF);
            return layout;
        }

        private Control BuildBeamSection()
        {
            CheckBox cbxActive = Check("Active", _p.ModeControl, v =>
            {
                _p.ModeControl = v;
                EnableBeamSettings(v);
            });

            _rblBeamMode = new RadioButtonList { Orientation = Orientation.Horizontal };
            _rblBeamMode.Items.Add("PC");
            _rblBeamMode.Items.Add("FMC");
            _rblBeamMode.Items.Add("LC");
            _rblBeamMode.SelectedIndex = BeamModeToIndex(_p.BeamMode);
            _rblBeamMode.SelectedIndexChanged += (sender, e) =>
            {
                _p.BeamMode = IndexToBeamMode(_rblBeamMode.SelectedIndex);
            };

            _lblAxial = new Label { Text = Percent(_p.AxialSF) };
            _sldAxial = new Slider { MinValue = 0, MaxValue = 100, TickFrequency = 5, Value = (int)Math.Round(_p.AxialSF * 100) };
            _sldAxial.ValueChanged += (sender, e) =>
            {
                _p.AxialSF = _sldAxial.Value / 100.0;
                _lblAxial.Text = Percent(_p.AxialSF);
            };

            _lblBending = new Label { Text = Percent(_p.BendingSF) };
            _sldBending = new Slider { MinValue = 0, MaxValue = 100, TickFrequency = 5, Value = (int)Math.Round(_p.BendingSF * 100) };
            _sldBending.ValueChanged += (sender, e) =>
            {
                _p.BendingSF = _sldBending.Value / 100.0;
                _lblBending.Text = Percent(_p.BendingSF);
            };

            EnableBeamSettings(_p.ModeControl);

            DynamicLayout layout = new DynamicLayout { DefaultSpacing = new Size(4, 4) };
            layout.AddRow(cbxActive);
            layout.AddRow(_rblBeamMode);
            layout.AddSeparateRow(new Label { Text = "Axial" }, _lblAxial);
            layout.AddRow(_sldAxial);
            layout.AddSeparateRow(new Label { Text = "Bending" }, _lblBending);
            layout.AddRow(_sldBending);
            return layout;
        }

        private Control BuildStabilitySection()
        {
            Label lblTrans = new Label { Text = Format(_p.TranslationalStiffnessScaleFactor) };
            Slider sldTrans = new Slider { MinValue = 1, MaxValue = 20, Value = (int)_p.TranslationalStiffnessScaleFactor };
            sldTrans.ValueChanged += (sender, e) =>
            {
                _p.TranslationalStiffnessScaleFactor = sldTrans.Value;
                lblTrans.Text = Format(_p.TranslationalStiffnessScaleFactor);
            };

            Label lblRot = new Label { Text = Format(_p.RotationalStiffnessScaleFactor) };
            Slider sldRot = new Slider { MinValue = 1, MaxValue = 100, TickFrequency = 10, Value = (int)_p.RotationalStiffnessScaleFactor };
            sldRot.ValueChanged += (sender, e) =>
            {
                _p.RotationalStiffnessScaleFactor = sldRot.Value;
                lblRot.Text = Format(_p.RotationalStiffnessScaleFactor);
            };

            DynamicLayout layout = new DynamicLayout { DefaultSpacing = new Size(4, 4) };
            layout.AddSeparateRow(new Label { Text = "Trans Stiff SF" }, lblTrans);
            layout.AddRow(sldTrans);
            layout.AddSeparateRow(new Label { Text = "Rot Stiff SF" }, lblRot);
            layout.AddRow(sldRot);
            return layout;
        }

        private Control BuildShowSection()
        {
            CheckBox[] checks = new CheckBox[]
            {
                Check("Vertices",    _p.DrawNodes,          v => _p.DrawNodes = v),
                Check("Lines",       _p.DrawLines,          v => _p.DrawLines = v),
                Check("Mesh",        _p.DrawMeshShaded,     v => _p.DrawMeshShaded = v),
                Check("V Normals",   _p.DrawVertexNormals,  v => _p.DrawVertexNormals = v),
                Check("F Normals",   _p.DrawFaceNormals,    v => _p.DrawFaceNormals = v),
                Check("Node Ring",   _p.DrawNodeRing,       v => _p.DrawNodeRing = v),
                Check("Bar 3D",      _p.Draw3dBars,         v => _p.Draw3dBars = v),
                Check("Node CS",     _p.DrawNodeCS,         v => _p.DrawNodeCS = v),
                Check("Bar CS",      _p.DrawBarCS,          v => _p.DrawBarCS = v),
                Check("Vertex ID",   _p.DrawVertexID,       v => _p.DrawVertexID = v),
                Check("Edge ID",     _p.DrawEdgeID,         v => _p.DrawEdgeID = v),
                Check("Face ID",     _p.DrawFaceID,         v => _p.DrawFaceID = v),
                Check("Utilisation", _p.DrawUtilisation,    v => _p.DrawUtilisation = v),
                Check("NodeMass",    _p.DrawNodeMass,       v => _p.DrawNodeMass = v),
                Check("NodeLoad",    _p.DrawNodeLoad,       v => _p.DrawNodeLoad = v),
                Check("Arrows",      _p.DrawLoadArrows,     v => _p.DrawLoadArrows = v),
                Check("NodeTS",      _p.DrawNodeTS,         v => _p.DrawNodeTS = v),
                Check("NodeRS",      _p.DrawNodeRS,         v => _p.DrawNodeRS = v),
                Check("ElementTS",   _p.DrawElementTS,      v => _p.DrawElementTS = v),
                Check("ElementRS",   _p.DrawElementRS,      v => _p.DrawElementRS = v),
            };

            DropDown ddColorBy = new DropDown();
            foreach (eColorBy c in Enum.GetValues(typeof(eColorBy)))
                ddColorBy.Items.Add(c.ToString());
            ddColorBy.SelectedIndex = (int)_p.ColorBy;
            ddColorBy.SelectedIndexChanged += (sender, e) =>
            {
                _p.ColorBy = (eColorBy)ddColorBy.SelectedIndex;
                _session.Refresh();
            };

            RadioButtonList rblRange = new RadioButtonList { Orientation = Orientation.Horizontal };
            rblRange.Items.Add("0 - 100");
            rblRange.Items.Add("AutoRange");
            rblRange.SelectedIndex = 0;
            rblRange.SelectedIndexChanged += (sender, e) => SetColorRange(rblRange.SelectedIndex == 1);

            //Three columns of check boxes.
            DynamicLayout grid = new DynamicLayout { DefaultSpacing = new Size(8, 2) };
            for (int i = 0; i < checks.Length; i += 3)
            {
                grid.AddRow(
                    checks[i],
                    i + 1 < checks.Length ? checks[i + 1] : null,
                    i + 2 < checks.Length ? checks[i + 2] : null);
            }

            DynamicLayout layout = new DynamicLayout { DefaultSpacing = new Size(4, 6) };
            layout.AddRow(grid);
            layout.AddSeparateRow(new Label { Text = "Colour by" }, ddColorBy);
            layout.AddRow(rblRange);
            return layout;
        }

        private Control BuildProgressSection()
        {
            _lblDRCount = new Label();
            _lblSizingCount = new Label();
            _lblOBF = new Label();
            _lblOBM = new Label();
            _lblConvergForce = new Label();
            _lblWeight = new Label();

            DynamicLayout layout = new DynamicLayout { DefaultSpacing = new Size(8, 2) };
            layout.AddRow(new Label { Text = "Relax iteration" }, _lblDRCount);
            layout.AddRow(new Label { Text = "Sizing iteration" }, _lblSizingCount);
            layout.AddRow(new Label { Text = "OOB Force" }, _lblOBF);
            layout.AddRow(new Label { Text = "OOB Moment" }, _lblOBM);
            layout.AddRow(new Label { Text = "Converg Force" }, _lblConvergForce);
            layout.AddRow(new Label { Text = "Weight (tons)" }, _lblWeight);

            UpdateProgress();
            return layout;
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------------------- Behaviour ------------------------------------//
        #region

        /** Check box bound to a Parameters flag; redraws the preview when toggled.
        *
        */
        private CheckBox Check(string text, bool value, Action<bool> setter)
        {
            CheckBox cbx = new CheckBox { Text = text, Checked = value };
            cbx.CheckedChanged += (sender, e) =>
            {
                setter(cbx.Checked == true);
                _session.Refresh();
            };
            return cbx;
        }

        private void EnableBeamSettings(bool enabled)
        {
            _rblBeamMode.Enabled = enabled;
            _sldAxial.Enabled = enabled;
            _sldBending.Enabled = enabled;
            _lblAxial.Enabled = enabled;
            _lblBending.Enabled = enabled;
        }

        private void SetColorRange(bool autoRange)
        {
            if (autoRange)
            {
                Structures.Main.mutex.WaitOne();
                try
                {
                    _p.ShowMin = _p.RealMin;
                    _p.ShowMax = _p.RealMax;
                }
                finally
                {
                    Structures.Main.mutex.ReleaseMutex();
                }
            }
            else
            {
                _p.ShowMin = 0;
                _p.ShowMax = 100;
            }
            _session.Refresh();
        }

        private void UpdateProgress()
        {
            _lblDRCount.Text = _p.DRCounter.ToString(CultureInfo.InvariantCulture);
            _lblSizingCount.Text = _p.SizingCounter.ToString(CultureInfo.InvariantCulture);
            _lblOBF.Text = Format(Math.Round(_p.OutOfBalanceForce, 3));
            _lblOBM.Text = Format(Math.Round(_p.OutOfBalanceMoment, 3));
            _lblConvergForce.Text = Format(Math.Round(_p.ConvergenceForce, 3));
            _lblWeight.Text = Format(Math.Round(_p.StructuralWeight, 2));
        }

        private static string Format(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string Percent(double scaleFactor)
        {
            return Math.Round(scaleFactor * 100).ToString(CultureInfo.InvariantCulture) + " %";
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------- Enum <-> index -----------------------------------//
        #region

        //Order matches the radio button lists above.
        private static readonly eElementType[] ElementTypes = { eElementType.Beam, eElementType.Bar, eElementType.Cable, eElementType.SplineB, eElementType.SplineBT };
        private static readonly eLoadType[] LoadTypes = { eLoadType.AreaLoad, eLoadType.LineLoad, eLoadType.PointLoad };
        private static readonly eBeamMode[] BeamModes = { eBeamMode.PropertyControl, eBeamMode.ForceMomentControl, eBeamMode.LimitControl };

        private static int ElementTypeToIndex(eElementType t) { return Math.Max(0, Array.IndexOf(ElementTypes, t)); }
        private static eElementType IndexToElementType(int i) { return ElementTypes[Math.Max(0, i)]; }
        private static int LoadTypeToIndex(eLoadType t) { return Math.Max(0, Array.IndexOf(LoadTypes, t)); }
        private static eLoadType IndexToLoadType(int i) { return LoadTypes[Math.Max(0, i)]; }
        private static int BeamModeToIndex(eBeamMode m) { return Math.Max(0, Array.IndexOf(BeamModes, m)); }
        private static eBeamMode IndexToBeamMode(int i) { return BeamModes[Math.Max(0, i)]; }

        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------------ IPanel --------------------------------------//
        #region

        public void PanelShown(uint documentSerialNumber, ShowPanelReason reason)
        {
            UpdateProgress();
            _uiTimer.Start();
            _session.ShowPreview();
        }

        public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason)
        {
            _uiTimer.Stop();
        }

        /** Same as closing the old dialog: stop the analysis and hide the preview.
        *
        */
        public void PanelClosing(uint documentSerialNumber, bool onCloseDocument)
        {
            _uiTimer.Stop();
            _session.StopAndHide();
        }

        #endregion
        //----------------------------------------------------------------------------------//
    }
}
