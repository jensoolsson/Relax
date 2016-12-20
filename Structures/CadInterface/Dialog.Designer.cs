namespace Structures
{
    partial class Dialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      this.btnSelectGeometry = new System.Windows.Forms.Button();
      this.btnPreprocess = new System.Windows.Forms.Button();
      this.btnRunDR = new System.Windows.Forms.Button();
      this.cbxShowVertexNormals = new System.Windows.Forms.CheckBox();
      this.cbxShowFaceNormals = new System.Windows.Forms.CheckBox();
      this.btnBake = new System.Windows.Forms.Button();
      this.cbxDrawVertices = new System.Windows.Forms.CheckBox();
      this.cbxDrawLines = new System.Windows.Forms.CheckBox();
      this.cbxDrawMeshShaded = new System.Windows.Forms.CheckBox();
      this.cbxDrawNodeCS = new System.Windows.Forms.CheckBox();
      this.cbxDrawBarCS = new System.Windows.Forms.CheckBox();
      this.cbxDrawNodeRing = new System.Windows.Forms.CheckBox();
      this.cbxDraw3dBar = new System.Windows.Forms.CheckBox();
      this.cbxDrawEdgeID = new System.Windows.Forms.CheckBox();
      this.cbxDrawVertexID = new System.Windows.Forms.CheckBox();
      this.cbxDrawFaceID = new System.Windows.Forms.CheckBox();
      this.btnSizing = new System.Windows.Forms.Button();
      this.cbxDrawDebug = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.tbxSizingCount = new System.Windows.Forms.TextBox();
      this.tbxDRCount = new System.Windows.Forms.TextBox();
      this.tbxOBF = new System.Windows.Forms.TextBox();
      this.tbxConvergForce = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.label6 = new System.Windows.Forms.Label();
      this.tbxStructuralWeight = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.tbxOBM = new System.Windows.Forms.TextBox();
      this.cmbColorBy = new System.Windows.Forms.ComboBox();
      this.rbtn0100 = new System.Windows.Forms.RadioButton();
      this.rbtnMinMax = new System.Windows.Forms.RadioButton();
      this.rbtnBeams = new System.Windows.Forms.RadioButton();
      this.rbtnBars = new System.Windows.Forms.RadioButton();
      this.rbtnCables = new System.Windows.Forms.RadioButton();
      this.rbtnBSplines = new System.Windows.Forms.RadioButton();
      this.groupBox4 = new System.Windows.Forms.GroupBox();
      this.rbtnBTSplines = new System.Windows.Forms.RadioButton();
      this.sldbrLoadSF = new System.Windows.Forms.TrackBar();
      this.label7 = new System.Windows.Forms.Label();
      this.tbxLoadSF = new System.Windows.Forms.TextBox();
      this.btnRewind = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox4.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.sldbrLoadSF)).BeginInit();
      this.SuspendLayout();
      // 
      // btnSelectGeometry
      // 
      this.btnSelectGeometry.Location = new System.Drawing.Point(14, 19);
      this.btnSelectGeometry.Name = "btnSelectGeometry";
      this.btnSelectGeometry.Size = new System.Drawing.Size(100, 24);
      this.btnSelectGeometry.TabIndex = 0;
      this.btnSelectGeometry.Text = "SelectGeometry";
      this.btnSelectGeometry.UseVisualStyleBackColor = true;
      this.btnSelectGeometry.Click += new System.EventHandler(this.btnSelectGeometry_Click);
      // 
      // btnPreprocess
      // 
      this.btnPreprocess.Location = new System.Drawing.Point(14, 155);
      this.btnPreprocess.Name = "btnPreprocess";
      this.btnPreprocess.Size = new System.Drawing.Size(100, 24);
      this.btnPreprocess.TabIndex = 1;
      this.btnPreprocess.Text = "Create Objects";
      this.btnPreprocess.UseVisualStyleBackColor = true;
      this.btnPreprocess.Click += new System.EventHandler(this.btnCreateObjects_Click);
      // 
      // btnRunDR
      // 
      this.btnRunDR.Location = new System.Drawing.Point(14, 193);
      this.btnRunDR.Name = "btnRunDR";
      this.btnRunDR.Size = new System.Drawing.Size(100, 24);
      this.btnRunDR.TabIndex = 2;
      this.btnRunDR.Text = "RunDR";
      this.btnRunDR.UseVisualStyleBackColor = true;
      this.btnRunDR.Click += new System.EventHandler(this.btnRunDR_Click);
      // 
      // cbxShowVertexNormals
      // 
      this.cbxShowVertexNormals.AutoSize = true;
      this.cbxShowVertexNormals.Location = new System.Drawing.Point(10, 17);
      this.cbxShowVertexNormals.Name = "cbxShowVertexNormals";
      this.cbxShowVertexNormals.Size = new System.Drawing.Size(74, 17);
      this.cbxShowVertexNormals.TabIndex = 3;
      this.cbxShowVertexNormals.Text = "V Normals";
      this.cbxShowVertexNormals.UseVisualStyleBackColor = true;
      this.cbxShowVertexNormals.CheckedChanged += new System.EventHandler(this.cbxShowVertexNormals_CheckedChanged);
      // 
      // cbxShowFaceNormals
      // 
      this.cbxShowFaceNormals.AutoSize = true;
      this.cbxShowFaceNormals.Location = new System.Drawing.Point(10, 35);
      this.cbxShowFaceNormals.Name = "cbxShowFaceNormals";
      this.cbxShowFaceNormals.Size = new System.Drawing.Size(73, 17);
      this.cbxShowFaceNormals.TabIndex = 4;
      this.cbxShowFaceNormals.Text = "F Normals";
      this.cbxShowFaceNormals.UseVisualStyleBackColor = true;
      this.cbxShowFaceNormals.CheckedChanged += new System.EventHandler(this.cbxShowFaceNormals_CheckedChanged);
      // 
      // btnBake
      // 
      this.btnBake.Location = new System.Drawing.Point(14, 251);
      this.btnBake.Name = "btnBake";
      this.btnBake.Size = new System.Drawing.Size(100, 24);
      this.btnBake.TabIndex = 5;
      this.btnBake.Text = "Bake";
      this.btnBake.UseVisualStyleBackColor = true;
      this.btnBake.Click += new System.EventHandler(this.btnBake_Click);
      // 
      // cbxDrawVertices
      // 
      this.cbxDrawVertices.AutoSize = true;
      this.cbxDrawVertices.Location = new System.Drawing.Point(10, 53);
      this.cbxDrawVertices.Name = "cbxDrawVertices";
      this.cbxDrawVertices.Size = new System.Drawing.Size(64, 17);
      this.cbxDrawVertices.TabIndex = 6;
      this.cbxDrawVertices.Text = "Vertices";
      this.cbxDrawVertices.UseVisualStyleBackColor = true;
      this.cbxDrawVertices.CheckedChanged += new System.EventHandler(this.cbxDrawVertices_CheckedChanged);
      // 
      // cbxDrawLines
      // 
      this.cbxDrawLines.AutoSize = true;
      this.cbxDrawLines.Location = new System.Drawing.Point(10, 71);
      this.cbxDrawLines.Name = "cbxDrawLines";
      this.cbxDrawLines.Size = new System.Drawing.Size(51, 17);
      this.cbxDrawLines.TabIndex = 7;
      this.cbxDrawLines.Text = "Lines";
      this.cbxDrawLines.UseVisualStyleBackColor = true;
      this.cbxDrawLines.CheckStateChanged += new System.EventHandler(this.cbxDrawLines_CheckStateChanged);
      // 
      // cbxDrawMeshShaded
      // 
      this.cbxDrawMeshShaded.AutoSize = true;
      this.cbxDrawMeshShaded.Location = new System.Drawing.Point(10, 90);
      this.cbxDrawMeshShaded.Name = "cbxDrawMeshShaded";
      this.cbxDrawMeshShaded.Size = new System.Drawing.Size(52, 17);
      this.cbxDrawMeshShaded.TabIndex = 9;
      this.cbxDrawMeshShaded.Text = "Mesh";
      this.cbxDrawMeshShaded.UseVisualStyleBackColor = true;
      this.cbxDrawMeshShaded.CheckedChanged += new System.EventHandler(this.cbxDrawMeshShaded_CheckedChanged);
      // 
      // cbxDrawNodeCS
      // 
      this.cbxDrawNodeCS.AutoSize = true;
      this.cbxDrawNodeCS.Location = new System.Drawing.Point(10, 19);
      this.cbxDrawNodeCS.Name = "cbxDrawNodeCS";
      this.cbxDrawNodeCS.Size = new System.Drawing.Size(69, 17);
      this.cbxDrawNodeCS.TabIndex = 10;
      this.cbxDrawNodeCS.Text = "Node CS";
      this.cbxDrawNodeCS.UseVisualStyleBackColor = true;
      this.cbxDrawNodeCS.CheckedChanged += new System.EventHandler(this.cbxDrawNodeCS_CheckedChanged);
      // 
      // cbxDrawBarCS
      // 
      this.cbxDrawBarCS.AutoSize = true;
      this.cbxDrawBarCS.Location = new System.Drawing.Point(10, 37);
      this.cbxDrawBarCS.Name = "cbxDrawBarCS";
      this.cbxDrawBarCS.Size = new System.Drawing.Size(59, 17);
      this.cbxDrawBarCS.TabIndex = 11;
      this.cbxDrawBarCS.Text = "Bar CS";
      this.cbxDrawBarCS.UseVisualStyleBackColor = true;
      this.cbxDrawBarCS.CheckedChanged += new System.EventHandler(this.cbxDrawBarCS_CheckedChanged);
      // 
      // cbxDrawNodeRing
      // 
      this.cbxDrawNodeRing.AutoSize = true;
      this.cbxDrawNodeRing.Location = new System.Drawing.Point(10, 108);
      this.cbxDrawNodeRing.Name = "cbxDrawNodeRing";
      this.cbxDrawNodeRing.Size = new System.Drawing.Size(77, 17);
      this.cbxDrawNodeRing.TabIndex = 12;
      this.cbxDrawNodeRing.Text = "Node Ring";
      this.cbxDrawNodeRing.UseVisualStyleBackColor = true;
      this.cbxDrawNodeRing.CheckedChanged += new System.EventHandler(this.cbxDrawNodeRing_CheckedChanged);
      // 
      // cbxDraw3dBar
      // 
      this.cbxDraw3dBar.AutoSize = true;
      this.cbxDraw3dBar.Location = new System.Drawing.Point(10, 126);
      this.cbxDraw3dBar.Name = "cbxDraw3dBar";
      this.cbxDraw3dBar.Size = new System.Drawing.Size(59, 17);
      this.cbxDraw3dBar.TabIndex = 13;
      this.cbxDraw3dBar.Text = "Bar 3D";
      this.cbxDraw3dBar.UseVisualStyleBackColor = true;
      this.cbxDraw3dBar.CheckedChanged += new System.EventHandler(this.cbxDraw3dBar_CheckedChanged);
      // 
      // cbxDrawEdgeID
      // 
      this.cbxDrawEdgeID.AutoSize = true;
      this.cbxDrawEdgeID.Location = new System.Drawing.Point(10, 74);
      this.cbxDrawEdgeID.Name = "cbxDrawEdgeID";
      this.cbxDrawEdgeID.Size = new System.Drawing.Size(65, 17);
      this.cbxDrawEdgeID.TabIndex = 14;
      this.cbxDrawEdgeID.Text = "Edge ID";
      this.cbxDrawEdgeID.UseVisualStyleBackColor = true;
      this.cbxDrawEdgeID.CheckedChanged += new System.EventHandler(this.cbxDrawEdgeID_CheckedChanged);
      // 
      // cbxDrawVertexID
      // 
      this.cbxDrawVertexID.AutoSize = true;
      this.cbxDrawVertexID.Location = new System.Drawing.Point(10, 56);
      this.cbxDrawVertexID.Name = "cbxDrawVertexID";
      this.cbxDrawVertexID.Size = new System.Drawing.Size(70, 17);
      this.cbxDrawVertexID.TabIndex = 15;
      this.cbxDrawVertexID.Text = "Vertex ID";
      this.cbxDrawVertexID.UseVisualStyleBackColor = true;
      this.cbxDrawVertexID.CheckedChanged += new System.EventHandler(this.cbxDrawVertexID_CheckedChanged);
      // 
      // cbxDrawFaceID
      // 
      this.cbxDrawFaceID.AutoSize = true;
      this.cbxDrawFaceID.Location = new System.Drawing.Point(10, 92);
      this.cbxDrawFaceID.Name = "cbxDrawFaceID";
      this.cbxDrawFaceID.Size = new System.Drawing.Size(64, 17);
      this.cbxDrawFaceID.TabIndex = 16;
      this.cbxDrawFaceID.Text = "Face ID";
      this.cbxDrawFaceID.UseVisualStyleBackColor = true;
      this.cbxDrawFaceID.CheckedChanged += new System.EventHandler(this.cbxDrawFaceID_CheckedChanged);
      // 
      // btnSizing
      // 
      this.btnSizing.Location = new System.Drawing.Point(14, 222);
      this.btnSizing.Name = "btnSizing";
      this.btnSizing.Size = new System.Drawing.Size(100, 24);
      this.btnSizing.TabIndex = 17;
      this.btnSizing.Text = "Sizing";
      this.btnSizing.UseVisualStyleBackColor = true;
      this.btnSizing.Click += new System.EventHandler(this.btnSizing_Click);
      // 
      // cbxDrawDebug
      // 
      this.cbxDrawDebug.AutoSize = true;
      this.cbxDrawDebug.Location = new System.Drawing.Point(10, 110);
      this.cbxDrawDebug.Name = "cbxDrawDebug";
      this.cbxDrawDebug.Size = new System.Drawing.Size(58, 17);
      this.cbxDrawDebug.TabIndex = 18;
      this.cbxDrawDebug.Text = "Debug";
      this.cbxDrawDebug.UseVisualStyleBackColor = true;
      this.cbxDrawDebug.CheckedChanged += new System.EventHandler(this.cbxDrawDebug_CheckedChanged);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbxShowVertexNormals);
      this.groupBox1.Controls.Add(this.cbxShowFaceNormals);
      this.groupBox1.Controls.Add(this.cbxDrawVertices);
      this.groupBox1.Controls.Add(this.cbxDrawLines);
      this.groupBox1.Controls.Add(this.cbxDrawMeshShaded);
      this.groupBox1.Controls.Add(this.cbxDrawNodeRing);
      this.groupBox1.Controls.Add(this.cbxDraw3dBar);
      this.groupBox1.Location = new System.Drawing.Point(120, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(90, 169);
      this.groupBox1.TabIndex = 21;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Geometry";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.cbxDrawVertexID);
      this.groupBox2.Controls.Add(this.cbxDrawDebug);
      this.groupBox2.Controls.Add(this.cbxDrawEdgeID);
      this.groupBox2.Controls.Add(this.cbxDrawFaceID);
      this.groupBox2.Controls.Add(this.cbxDrawNodeCS);
      this.groupBox2.Controls.Add(this.cbxDrawBarCS);
      this.groupBox2.Location = new System.Drawing.Point(216, 12);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(90, 169);
      this.groupBox2.TabIndex = 22;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Show";
      // 
      // tbxSizingCount
      // 
      this.tbxSizingCount.Location = new System.Drawing.Point(101, 38);
      this.tbxSizingCount.Name = "tbxSizingCount";
      this.tbxSizingCount.Size = new System.Drawing.Size(75, 20);
      this.tbxSizingCount.TabIndex = 23;
      // 
      // tbxDRCount
      // 
      this.tbxDRCount.Location = new System.Drawing.Point(101, 14);
      this.tbxDRCount.Name = "tbxDRCount";
      this.tbxDRCount.Size = new System.Drawing.Size(75, 20);
      this.tbxDRCount.TabIndex = 24;
      // 
      // tbxOBF
      // 
      this.tbxOBF.Location = new System.Drawing.Point(101, 86);
      this.tbxOBF.Name = "tbxOBF";
      this.tbxOBF.Size = new System.Drawing.Size(74, 20);
      this.tbxOBF.TabIndex = 25;
      // 
      // tbxConvergForce
      // 
      this.tbxConvergForce.Location = new System.Drawing.Point(101, 110);
      this.tbxConvergForce.Name = "tbxConvergForce";
      this.tbxConvergForce.Size = new System.Drawing.Size(74, 20);
      this.tbxConvergForce.TabIndex = 26;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(10, 19);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(74, 13);
      this.label1.TabIndex = 27;
      this.label1.Text = "Relax iteration";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(10, 42);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(75, 13);
      this.label2.TabIndex = 28;
      this.label2.Text = "Sizing iteration";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(10, 88);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(60, 13);
      this.label3.TabIndex = 29;
      this.label3.Text = "OOB Force";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(10, 111);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(77, 13);
      this.label4.TabIndex = 30;
      this.label4.Text = "Converg Force";
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.label6);
      this.groupBox3.Controls.Add(this.tbxStructuralWeight);
      this.groupBox3.Controls.Add(this.label5);
      this.groupBox3.Controls.Add(this.tbxOBM);
      this.groupBox3.Controls.Add(this.label1);
      this.groupBox3.Controls.Add(this.label4);
      this.groupBox3.Controls.Add(this.tbxSizingCount);
      this.groupBox3.Controls.Add(this.label3);
      this.groupBox3.Controls.Add(this.tbxDRCount);
      this.groupBox3.Controls.Add(this.label2);
      this.groupBox3.Controls.Add(this.tbxOBF);
      this.groupBox3.Controls.Add(this.tbxConvergForce);
      this.groupBox3.Location = new System.Drawing.Point(120, 187);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(186, 161);
      this.groupBox3.TabIndex = 31;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Progress";
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(10, 134);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(70, 13);
      this.label6.TabIndex = 34;
      this.label6.Text = "Weight (tons)";
      // 
      // tbxStructuralWeight
      // 
      this.tbxStructuralWeight.Location = new System.Drawing.Point(101, 134);
      this.tbxStructuralWeight.Name = "tbxStructuralWeight";
      this.tbxStructuralWeight.Size = new System.Drawing.Size(74, 20);
      this.tbxStructuralWeight.TabIndex = 33;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(10, 65);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(71, 13);
      this.label5.TabIndex = 32;
      this.label5.Text = "OOB Moment";
      // 
      // tbxOBM
      // 
      this.tbxOBM.Location = new System.Drawing.Point(101, 62);
      this.tbxOBM.Name = "tbxOBM";
      this.tbxOBM.Size = new System.Drawing.Size(74, 20);
      this.tbxOBM.TabIndex = 31;
      // 
      // cmbColorBy
      // 
      this.cmbColorBy.FormattingEnabled = true;
      this.cmbColorBy.Location = new System.Drawing.Point(14, 282);
      this.cmbColorBy.Name = "cmbColorBy";
      this.cmbColorBy.Size = new System.Drawing.Size(100, 21);
      this.cmbColorBy.TabIndex = 32;
      this.cmbColorBy.SelectedIndexChanged += new System.EventHandler(this.cmbColorBy_SelectedIndexChanged);
      // 
      // rbtn0100
      // 
      this.rbtn0100.AutoSize = true;
      this.rbtn0100.Checked = true;
      this.rbtn0100.Location = new System.Drawing.Point(18, 308);
      this.rbtn0100.Name = "rbtn0100";
      this.rbtn0100.Size = new System.Drawing.Size(58, 17);
      this.rbtn0100.TabIndex = 33;
      this.rbtn0100.TabStop = true;
      this.rbtn0100.Text = "0 - 100";
      this.rbtn0100.UseVisualStyleBackColor = true;
      this.rbtn0100.CheckedChanged += new System.EventHandler(this.rbtn0100_CheckedChanged);
      // 
      // rbtnMinMax
      // 
      this.rbtnMinMax.AutoSize = true;
      this.rbtnMinMax.Location = new System.Drawing.Point(18, 325);
      this.rbtnMinMax.Name = "rbtnMinMax";
      this.rbtnMinMax.Size = new System.Drawing.Size(79, 17);
      this.rbtnMinMax.TabIndex = 34;
      this.rbtnMinMax.Text = "AutoRange";
      this.rbtnMinMax.UseVisualStyleBackColor = true;
      this.rbtnMinMax.CheckedChanged += new System.EventHandler(this.rbtnMinMax_CheckedChanged);
      // 
      // rbtnBeams
      // 
      this.rbtnBeams.AutoSize = true;
      this.rbtnBeams.Checked = true;
      this.rbtnBeams.Location = new System.Drawing.Point(6, 11);
      this.rbtnBeams.Name = "rbtnBeams";
      this.rbtnBeams.Size = new System.Drawing.Size(57, 17);
      this.rbtnBeams.TabIndex = 35;
      this.rbtnBeams.TabStop = true;
      this.rbtnBeams.Text = "Beams";
      this.rbtnBeams.UseVisualStyleBackColor = true;
      this.rbtnBeams.CheckedChanged += new System.EventHandler(this.rbtnBeams_CheckedChanged);
      // 
      // rbtnBars
      // 
      this.rbtnBars.AutoSize = true;
      this.rbtnBars.Location = new System.Drawing.Point(6, 29);
      this.rbtnBars.Name = "rbtnBars";
      this.rbtnBars.Size = new System.Drawing.Size(46, 17);
      this.rbtnBars.TabIndex = 36;
      this.rbtnBars.Text = "Bars";
      this.rbtnBars.UseVisualStyleBackColor = true;
      this.rbtnBars.CheckedChanged += new System.EventHandler(this.rbtnBars_CheckedChanged);
      // 
      // rbtnCables
      // 
      this.rbtnCables.AutoSize = true;
      this.rbtnCables.Location = new System.Drawing.Point(6, 47);
      this.rbtnCables.Name = "rbtnCables";
      this.rbtnCables.Size = new System.Drawing.Size(57, 17);
      this.rbtnCables.TabIndex = 37;
      this.rbtnCables.Text = "Cables";
      this.rbtnCables.UseVisualStyleBackColor = true;
      this.rbtnCables.CheckedChanged += new System.EventHandler(this.rbtnCables_CheckedChanged);
      // 
      // rbtnBSplines
      // 
      this.rbtnBSplines.AutoSize = true;
      this.rbtnBSplines.Location = new System.Drawing.Point(6, 65);
      this.rbtnBSplines.Name = "rbtnBSplines";
      this.rbtnBSplines.Size = new System.Drawing.Size(72, 17);
      this.rbtnBSplines.TabIndex = 38;
      this.rbtnBSplines.Text = "B_Splines";
      this.rbtnBSplines.UseVisualStyleBackColor = true;
      this.rbtnBSplines.CheckedChanged += new System.EventHandler(this.rbtnSplines_CheckedChanged);
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.rbtnBTSplines);
      this.groupBox4.Controls.Add(this.rbtnBeams);
      this.groupBox4.Controls.Add(this.rbtnBSplines);
      this.groupBox4.Controls.Add(this.rbtnBars);
      this.groupBox4.Controls.Add(this.rbtnCables);
      this.groupBox4.Location = new System.Drawing.Point(14, 43);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(100, 106);
      this.groupBox4.TabIndex = 39;
      this.groupBox4.TabStop = false;
      // 
      // rbtnBTSplines
      // 
      this.rbtnBTSplines.AutoSize = true;
      this.rbtnBTSplines.Location = new System.Drawing.Point(6, 82);
      this.rbtnBTSplines.Name = "rbtnBTSplines";
      this.rbtnBTSplines.Size = new System.Drawing.Size(79, 17);
      this.rbtnBTSplines.TabIndex = 39;
      this.rbtnBTSplines.Text = "BT_Splines";
      this.rbtnBTSplines.UseVisualStyleBackColor = true;
      this.rbtnBTSplines.CheckedChanged += new System.EventHandler(this.rbtnBTSplines_CheckedChanged);
      // 
      // sldbrLoadSF
      // 
      this.sldbrLoadSF.LargeChange = 2;
      this.sldbrLoadSF.Location = new System.Drawing.Point(120, 353);
      this.sldbrLoadSF.Minimum = -10;
      this.sldbrLoadSF.Name = "sldbrLoadSF";
      this.sldbrLoadSF.Size = new System.Drawing.Size(191, 42);
      this.sldbrLoadSF.TabIndex = 40;
      this.sldbrLoadSF.Value = 1;
      this.sldbrLoadSF.Scroll += new System.EventHandler(this.sldbrLoadSF_Scroll);
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(15, 353);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(89, 13);
      this.label7.TabIndex = 35;
      this.label7.Text = "Load scale factor";
      // 
      // tbxLoadSF
      // 
      this.tbxLoadSF.Location = new System.Drawing.Point(14, 369);
      this.tbxLoadSF.Name = "tbxLoadSF";
      this.tbxLoadSF.Size = new System.Drawing.Size(100, 20);
      this.tbxLoadSF.TabIndex = 35;
      // 
      // btnRewind
      // 
      this.btnRewind.Location = new System.Drawing.Point(14, 405);
      this.btnRewind.Name = "btnRewind";
      this.btnRewind.Size = new System.Drawing.Size(100, 23);
      this.btnRewind.TabIndex = 41;
      this.btnRewind.Text = "Rewind";
      this.btnRewind.UseVisualStyleBackColor = true;
      this.btnRewind.Click += new System.EventHandler(this.btnRewind_Click);
      // 
      // Dialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(326, 454);
      this.Controls.Add(this.btnRewind);
      this.Controls.Add(this.tbxLoadSF);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.sldbrLoadSF);
      this.Controls.Add(this.groupBox4);
      this.Controls.Add(this.rbtnMinMax);
      this.Controls.Add(this.rbtn0100);
      this.Controls.Add(this.cmbColorBy);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.btnSizing);
      this.Controls.Add(this.btnBake);
      this.Controls.Add(this.btnRunDR);
      this.Controls.Add(this.btnPreprocess);
      this.Controls.Add(this.btnSelectGeometry);
      this.Location = new System.Drawing.Point(1300, 200);
      this.Name = "Dialog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Structures";
      this.TopMost = true;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Dialog_FormClosing);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.sldbrLoadSF)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSelectGeometry;
        private System.Windows.Forms.Button btnPreprocess;
        private System.Windows.Forms.Button btnRunDR;
        private System.Windows.Forms.CheckBox cbxShowVertexNormals;
        private System.Windows.Forms.CheckBox cbxShowFaceNormals;
        private System.Windows.Forms.Button btnBake;
        private System.Windows.Forms.CheckBox cbxDrawVertices;
        private System.Windows.Forms.CheckBox cbxDrawLines;
        private System.Windows.Forms.CheckBox cbxDrawMeshShaded;
        private System.Windows.Forms.CheckBox cbxDrawNodeCS;
        private System.Windows.Forms.CheckBox cbxDrawBarCS;
        private System.Windows.Forms.CheckBox cbxDrawNodeRing;
        private System.Windows.Forms.CheckBox cbxDraw3dBar;
        private System.Windows.Forms.CheckBox cbxDrawEdgeID;
        private System.Windows.Forms.CheckBox cbxDrawVertexID;
        private System.Windows.Forms.CheckBox cbxDrawFaceID;
        private System.Windows.Forms.Button btnSizing;
        private System.Windows.Forms.CheckBox cbxDrawDebug;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tbxSizingCount;
        private System.Windows.Forms.TextBox tbxDRCount;
        private System.Windows.Forms.TextBox tbxOBF;
        private System.Windows.Forms.TextBox tbxConvergForce;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbxOBM;
        private System.Windows.Forms.ComboBox cmbColorBy;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbxStructuralWeight;
        private System.Windows.Forms.RadioButton rbtn0100;
        private System.Windows.Forms.RadioButton rbtnMinMax;
        private System.Windows.Forms.RadioButton rbtnBeams;
        private System.Windows.Forms.RadioButton rbtnBars;
        private System.Windows.Forms.RadioButton rbtnCables;
        private System.Windows.Forms.RadioButton rbtnBSplines;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TrackBar sldbrLoadSF;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbxLoadSF;
        private System.Windows.Forms.RadioButton rbtnBTSplines;
        private System.Windows.Forms.Button btnRewind;
    }
}