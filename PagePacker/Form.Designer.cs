namespace PagePacker
{
    partial class mainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(mainForm));
            this.label1 = new System.Windows.Forms.Label();
            this.sourceFolderTB = new System.Windows.Forms.TextBox();
            this.selectSourceFolderBtn = new System.Windows.Forms.Button();
            this.filesToPackCLB = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.loadPackingConfigFileBtn = new System.Windows.Forms.Button();
            this.exportPackingConfigBtn = new System.Windows.Forms.Button();
            this.packPageFilesBtn = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsStatusLbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.pageHTMLFileCB = new System.Windows.Forms.ComboBox();
            this.pageCSSFileCB = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.minifyCSSCB = new System.Windows.Forms.CheckBox();
            this.minifyJSONCB = new System.Windows.Forms.CheckBox();
            this.minifyJSCB = new System.Windows.Forms.CheckBox();
            this.filesToMinifyCLB = new System.Windows.Forms.CheckedListBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.jsEntryPointFuncNameTB = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.jsCrunchVarNameCB = new System.Windows.Forms.CheckBox();
            this.jsCrunchParamNameCB = new System.Windows.Forms.CheckBox();
            this.jsCrunchFuncNameCB = new System.Windows.Forms.CheckBox();
            this.jsRemoveLogStmtCB = new System.Windows.Forms.CheckBox();
            this.jsLBRemoveCB = new System.Windows.Forms.CheckBox();
            this.jsCommentRemoveCB = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cssLBRemoveCB = new System.Windows.Forms.CheckBox();
            this.cssCommentRemoveCB = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(145, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source Folder";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // sourceFolderTB
            // 
            this.sourceFolderTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceFolderTB.Enabled = false;
            this.sourceFolderTB.Location = new System.Drawing.Point(193, 12);
            this.sourceFolderTB.Name = "sourceFolderTB";
            this.sourceFolderTB.Size = new System.Drawing.Size(553, 22);
            this.sourceFolderTB.TabIndex = 1;
            // 
            // selectSourceFolderBtn
            // 
            this.selectSourceFolderBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectSourceFolderBtn.Location = new System.Drawing.Point(752, 12);
            this.selectSourceFolderBtn.Name = "selectSourceFolderBtn";
            this.selectSourceFolderBtn.Size = new System.Drawing.Size(43, 23);
            this.selectSourceFolderBtn.TabIndex = 2;
            this.selectSourceFolderBtn.Text = "...";
            this.selectSourceFolderBtn.UseVisualStyleBackColor = true;
            this.selectSourceFolderBtn.Click += new System.EventHandler(this.selectSourceFolderBtn_Click);
            // 
            // filesToPackCLB
            // 
            this.filesToPackCLB.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.filesToPackCLB.CheckOnClick = true;
            this.filesToPackCLB.FormattingEnabled = true;
            this.filesToPackCLB.Location = new System.Drawing.Point(3, 32);
            this.filesToPackCLB.Name = "filesToPackCLB";
            this.filesToPackCLB.Size = new System.Drawing.Size(382, 174);
            this.filesToPackCLB.TabIndex = 3;
            this.filesToPackCLB.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.filesToPackCLB_ItemCheck);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(382, 29);
            this.label2.TabIndex = 4;
            this.label2.Text = "Files To Include";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.loadPackingConfigFileBtn);
            this.groupBox1.Controls.Add(this.exportPackingConfigBtn);
            this.groupBox1.Controls.Add(this.packPageFilesBtn);
            this.groupBox1.Location = new System.Drawing.Point(17, 547);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(777, 128);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Actions";
            // 
            // loadPackingConfigFileBtn
            // 
            this.loadPackingConfigFileBtn.Location = new System.Drawing.Point(119, 21);
            this.loadPackingConfigFileBtn.Name = "loadPackingConfigFileBtn";
            this.loadPackingConfigFileBtn.Size = new System.Drawing.Size(103, 101);
            this.loadPackingConfigFileBtn.TabIndex = 2;
            this.loadPackingConfigFileBtn.Text = "Load Packing Configuration File";
            this.loadPackingConfigFileBtn.UseVisualStyleBackColor = true;
            this.loadPackingConfigFileBtn.Click += new System.EventHandler(this.loadPackingConfigFileBtn_Click);
            // 
            // exportPackingConfigBtn
            // 
            this.exportPackingConfigBtn.Enabled = false;
            this.exportPackingConfigBtn.Location = new System.Drawing.Point(10, 21);
            this.exportPackingConfigBtn.Name = "exportPackingConfigBtn";
            this.exportPackingConfigBtn.Size = new System.Drawing.Size(103, 101);
            this.exportPackingConfigBtn.TabIndex = 1;
            this.exportPackingConfigBtn.Text = "Export Packing Configuration As File";
            this.exportPackingConfigBtn.UseVisualStyleBackColor = true;
            this.exportPackingConfigBtn.Click += new System.EventHandler(this.exportPackingConfigBtn_Click);
            // 
            // packPageFilesBtn
            // 
            this.packPageFilesBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.packPageFilesBtn.Enabled = false;
            this.packPageFilesBtn.Location = new System.Drawing.Point(228, 21);
            this.packPageFilesBtn.Name = "packPageFilesBtn";
            this.packPageFilesBtn.Size = new System.Drawing.Size(543, 101);
            this.packPageFilesBtn.TabIndex = 0;
            this.packPageFilesBtn.Text = "Pack Files";
            this.packPageFilesBtn.UseVisualStyleBackColor = true;
            this.packPageFilesBtn.Click += new System.EventHandler(this.packPageFilesBtn_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsStatusLbl,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 682);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(806, 22);
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsStatusLbl
            // 
            this.tsStatusLbl.Name = "tsStatusLbl";
            this.tsStatusLbl.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label4.Location = new System.Drawing.Point(17, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(166, 24);
            this.label4.TabIndex = 9;
            this.label4.Text = "Page HTML File";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label5.Location = new System.Drawing.Point(17, 91);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(166, 24);
            this.label5.TabIndex = 12;
            this.label5.Text = "Page CSS File";
            // 
            // pageHTMLFileCB
            // 
            this.pageHTMLFileCB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pageHTMLFileCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.pageHTMLFileCB.Enabled = false;
            this.pageHTMLFileCB.FormattingEnabled = true;
            this.pageHTMLFileCB.Location = new System.Drawing.Point(193, 51);
            this.pageHTMLFileCB.Name = "pageHTMLFileCB";
            this.pageHTMLFileCB.Size = new System.Drawing.Size(602, 24);
            this.pageHTMLFileCB.TabIndex = 13;
            this.pageHTMLFileCB.SelectedValueChanged += new System.EventHandler(this.pageHTMLFileCB_SelectedValueChanged);
            // 
            // pageCSSFileCB
            // 
            this.pageCSSFileCB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pageCSSFileCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.pageCSSFileCB.Enabled = false;
            this.pageCSSFileCB.FormattingEnabled = true;
            this.pageCSSFileCB.Location = new System.Drawing.Point(194, 91);
            this.pageCSSFileCB.Name = "pageCSSFileCB";
            this.pageCSSFileCB.Size = new System.Drawing.Size(601, 24);
            this.pageCSSFileCB.TabIndex = 14;
            this.pageCSSFileCB.SelectedIndexChanged += new System.EventHandler(this.pageCSSFileCB_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.groupBox2.Controls.Add(this.minifyCSSCB);
            this.groupBox2.Controls.Add(this.minifyJSONCB);
            this.groupBox2.Controls.Add(this.minifyJSCB);
            this.groupBox2.Location = new System.Drawing.Point(18, 432);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(165, 105);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Minification Settings";
            // 
            // minifyCSSCB
            // 
            this.minifyCSSCB.AutoSize = true;
            this.minifyCSSCB.Checked = true;
            this.minifyCSSCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.minifyCSSCB.Enabled = false;
            this.minifyCSSCB.Location = new System.Drawing.Point(12, 75);
            this.minifyCSSCB.Name = "minifyCSSCB";
            this.minifyCSSCB.Size = new System.Drawing.Size(97, 21);
            this.minifyCSSCB.TabIndex = 21;
            this.minifyCSSCB.Text = "Minify CSS";
            this.minifyCSSCB.UseVisualStyleBackColor = true;
            this.minifyCSSCB.CheckedChanged += new System.EventHandler(this.minifyCSSCB_CheckedChanged);
            this.minifyCSSCB.EnabledChanged += new System.EventHandler(this.minifyCSSCB_EnabledChanged);
            // 
            // minifyJSONCB
            // 
            this.minifyJSONCB.AutoSize = true;
            this.minifyJSONCB.Checked = true;
            this.minifyJSONCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.minifyJSONCB.Enabled = false;
            this.minifyJSONCB.Location = new System.Drawing.Point(12, 48);
            this.minifyJSONCB.Name = "minifyJSONCB";
            this.minifyJSONCB.Size = new System.Drawing.Size(107, 21);
            this.minifyJSONCB.TabIndex = 16;
            this.minifyJSONCB.Text = "Minify JSON";
            this.minifyJSONCB.UseVisualStyleBackColor = true;
            this.minifyJSONCB.CheckedChanged += new System.EventHandler(this.minifyJSONCB_CheckedChanged);
            // 
            // minifyJSCB
            // 
            this.minifyJSCB.AutoSize = true;
            this.minifyJSCB.Checked = true;
            this.minifyJSCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.minifyJSCB.Enabled = false;
            this.minifyJSCB.Location = new System.Drawing.Point(12, 21);
            this.minifyJSCB.Name = "minifyJSCB";
            this.minifyJSCB.Size = new System.Drawing.Size(134, 21);
            this.minifyJSCB.TabIndex = 0;
            this.minifyJSCB.Text = "Minify Javascript";
            this.minifyJSCB.UseVisualStyleBackColor = true;
            this.minifyJSCB.CheckedChanged += new System.EventHandler(this.minifyJSCB_CheckedChanged);
            this.minifyJSCB.EnabledChanged += new System.EventHandler(this.minifyJSCB_EnabledChanged);
            // 
            // filesToMinifyCLB
            // 
            this.filesToMinifyCLB.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.filesToMinifyCLB.FormattingEnabled = true;
            this.filesToMinifyCLB.Location = new System.Drawing.Point(391, 32);
            this.filesToMinifyCLB.Name = "filesToMinifyCLB";
            this.filesToMinifyCLB.Size = new System.Drawing.Size(383, 174);
            this.filesToMinifyCLB.TabIndex = 16;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.label3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.filesToPackCLB, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.filesToMinifyCLB, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(18, 201);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(777, 225);
            this.tableLayoutPanel1.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(391, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(383, 29);
            this.label3.TabIndex = 18;
            this.label3.Text = "Files To Minify";
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label6.Location = new System.Drawing.Point(18, 132);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(777, 28);
            this.label6.TabIndex = 18;
            this.label6.Text = "Javascript Entry Point (Function Name)";
            // 
            // jsEntryPointFuncNameTB
            // 
            this.jsEntryPointFuncNameTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.jsEntryPointFuncNameTB.Enabled = false;
            this.jsEntryPointFuncNameTB.Location = new System.Drawing.Point(18, 163);
            this.jsEntryPointFuncNameTB.Name = "jsEntryPointFuncNameTB";
            this.jsEntryPointFuncNameTB.Size = new System.Drawing.Size(777, 22);
            this.jsEntryPointFuncNameTB.TabIndex = 19;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.groupBox3.Controls.Add(this.jsCrunchVarNameCB);
            this.groupBox3.Controls.Add(this.jsCrunchParamNameCB);
            this.groupBox3.Controls.Add(this.jsCrunchFuncNameCB);
            this.groupBox3.Controls.Add(this.jsRemoveLogStmtCB);
            this.groupBox3.Controls.Add(this.jsLBRemoveCB);
            this.groupBox3.Controls.Add(this.jsCommentRemoveCB);
            this.groupBox3.Location = new System.Drawing.Point(193, 432);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(403, 105);
            this.groupBox3.TabIndex = 20;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Javascript Formatting Options";
            // 
            // jsCrunchVarNameCB
            // 
            this.jsCrunchVarNameCB.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.jsCrunchVarNameCB.AutoSize = true;
            this.jsCrunchVarNameCB.Checked = true;
            this.jsCrunchVarNameCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.jsCrunchVarNameCB.Enabled = false;
            this.jsCrunchVarNameCB.Location = new System.Drawing.Point(6, 49);
            this.jsCrunchVarNameCB.Name = "jsCrunchVarNameCB";
            this.jsCrunchVarNameCB.Size = new System.Drawing.Size(174, 21);
            this.jsCrunchVarNameCB.TabIndex = 23;
            this.jsCrunchVarNameCB.Text = "Shrink Variable Names";
            this.jsCrunchVarNameCB.UseVisualStyleBackColor = true;
            // 
            // jsCrunchParamNameCB
            // 
            this.jsCrunchParamNameCB.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.jsCrunchParamNameCB.AutoSize = true;
            this.jsCrunchParamNameCB.Checked = true;
            this.jsCrunchParamNameCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.jsCrunchParamNameCB.Enabled = false;
            this.jsCrunchParamNameCB.Location = new System.Drawing.Point(188, 75);
            this.jsCrunchParamNameCB.Name = "jsCrunchParamNameCB";
            this.jsCrunchParamNameCB.Size = new System.Drawing.Size(188, 21);
            this.jsCrunchParamNameCB.TabIndex = 22;
            this.jsCrunchParamNameCB.Text = "Shrink Parameter Names";
            this.jsCrunchParamNameCB.UseVisualStyleBackColor = true;
            // 
            // jsCrunchFuncNameCB
            // 
            this.jsCrunchFuncNameCB.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.jsCrunchFuncNameCB.AutoSize = true;
            this.jsCrunchFuncNameCB.Checked = true;
            this.jsCrunchFuncNameCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.jsCrunchFuncNameCB.Enabled = false;
            this.jsCrunchFuncNameCB.Location = new System.Drawing.Point(6, 75);
            this.jsCrunchFuncNameCB.Name = "jsCrunchFuncNameCB";
            this.jsCrunchFuncNameCB.Size = new System.Drawing.Size(176, 21);
            this.jsCrunchFuncNameCB.TabIndex = 21;
            this.jsCrunchFuncNameCB.Text = "Shrink Function Names";
            this.jsCrunchFuncNameCB.UseVisualStyleBackColor = true;
            // 
            // jsRemoveLogStmtCB
            // 
            this.jsRemoveLogStmtCB.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.jsRemoveLogStmtCB.AutoSize = true;
            this.jsRemoveLogStmtCB.Checked = true;
            this.jsRemoveLogStmtCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.jsRemoveLogStmtCB.Enabled = false;
            this.jsRemoveLogStmtCB.Location = new System.Drawing.Point(188, 49);
            this.jsRemoveLogStmtCB.Name = "jsRemoveLogStmtCB";
            this.jsRemoveLogStmtCB.Size = new System.Drawing.Size(205, 21);
            this.jsRemoveLogStmtCB.TabIndex = 23;
            this.jsRemoveLogStmtCB.Text = "Remove logging statements";
            this.jsRemoveLogStmtCB.UseVisualStyleBackColor = true;
            // 
            // jsLBRemoveCB
            // 
            this.jsLBRemoveCB.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.jsLBRemoveCB.AutoSize = true;
            this.jsLBRemoveCB.Checked = true;
            this.jsLBRemoveCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.jsLBRemoveCB.Enabled = false;
            this.jsLBRemoveCB.Location = new System.Drawing.Point(188, 24);
            this.jsLBRemoveCB.Name = "jsLBRemoveCB";
            this.jsLBRemoveCB.Size = new System.Drawing.Size(155, 21);
            this.jsLBRemoveCB.TabIndex = 21;
            this.jsLBRemoveCB.Text = "Remove line breaks";
            this.jsLBRemoveCB.UseVisualStyleBackColor = true;
            // 
            // jsCommentRemoveCB
            // 
            this.jsCommentRemoveCB.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.jsCommentRemoveCB.AutoSize = true;
            this.jsCommentRemoveCB.Checked = true;
            this.jsCommentRemoveCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.jsCommentRemoveCB.Enabled = false;
            this.jsCommentRemoveCB.Location = new System.Drawing.Point(6, 24);
            this.jsCommentRemoveCB.Name = "jsCommentRemoveCB";
            this.jsCommentRemoveCB.Size = new System.Drawing.Size(150, 21);
            this.jsCommentRemoveCB.TabIndex = 1;
            this.jsCommentRemoveCB.Text = "Remove comments";
            this.jsCommentRemoveCB.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.groupBox4.Controls.Add(this.cssLBRemoveCB);
            this.groupBox4.Controls.Add(this.cssCommentRemoveCB);
            this.groupBox4.Location = new System.Drawing.Point(602, 432);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(192, 105);
            this.groupBox4.TabIndex = 21;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "CSS Formatting Options";
            // 
            // cssLBRemoveCB
            // 
            this.cssLBRemoveCB.AutoSize = true;
            this.cssLBRemoveCB.Checked = true;
            this.cssLBRemoveCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cssLBRemoveCB.Enabled = false;
            this.cssLBRemoveCB.Location = new System.Drawing.Point(12, 51);
            this.cssLBRemoveCB.Name = "cssLBRemoveCB";
            this.cssLBRemoveCB.Size = new System.Drawing.Size(155, 21);
            this.cssLBRemoveCB.TabIndex = 22;
            this.cssLBRemoveCB.Text = "Remove line breaks";
            this.cssLBRemoveCB.UseVisualStyleBackColor = true;
            // 
            // cssCommentRemoveCB
            // 
            this.cssCommentRemoveCB.AutoSize = true;
            this.cssCommentRemoveCB.Checked = true;
            this.cssCommentRemoveCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cssCommentRemoveCB.Enabled = false;
            this.cssCommentRemoveCB.Location = new System.Drawing.Point(12, 24);
            this.cssCommentRemoveCB.Name = "cssCommentRemoveCB";
            this.cssCommentRemoveCB.Size = new System.Drawing.Size(150, 21);
            this.cssCommentRemoveCB.TabIndex = 22;
            this.cssCommentRemoveCB.Text = "Remove comments";
            this.cssCommentRemoveCB.UseVisualStyleBackColor = true;
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(806, 704);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.jsEntryPointFuncNameTB);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.pageCSSFileCB);
            this.Controls.Add(this.pageHTMLFileCB);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.selectSourceFolderBtn);
            this.Controls.Add(this.sourceFolderTB);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(824, 751);
            this.Name = "mainForm";
            this.Text = "PagePacker v1.0 by Jacob Micallef (Finalized 17 March 2026)";
            this.groupBox1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox sourceFolderTB;
        private System.Windows.Forms.Button selectSourceFolderBtn;
        private System.Windows.Forms.CheckedListBox filesToPackCLB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button loadPackingConfigFileBtn;
        private System.Windows.Forms.Button exportPackingConfigBtn;
        private System.Windows.Forms.Button packPageFilesBtn;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolStripStatusLabel tsStatusLbl;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox pageHTMLFileCB;
        private System.Windows.Forms.ComboBox pageCSSFileCB;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox minifyJSONCB;
        private System.Windows.Forms.CheckBox minifyJSCB;
        private System.Windows.Forms.CheckedListBox filesToMinifyCLB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox jsEntryPointFuncNameTB;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox jsLBRemoveCB;
        private System.Windows.Forms.CheckBox jsCommentRemoveCB;
        private System.Windows.Forms.CheckBox jsRemoveLogStmtCB;
        private System.Windows.Forms.CheckBox jsCrunchFuncNameCB;
        private System.Windows.Forms.CheckBox jsCrunchParamNameCB;
        private System.Windows.Forms.CheckBox jsCrunchVarNameCB;
        private System.Windows.Forms.CheckBox minifyCSSCB;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox cssLBRemoveCB;
        private System.Windows.Forms.CheckBox cssCommentRemoveCB;
    }
}

