namespace JeuxDePoints {
    partial class MenuPanel {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.controlsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.controlsHeaderLabel = new System.Windows.Forms.Label();
            this.newGameBtn = new System.Windows.Forms.Button();
            this.saveBtn = new System.Windows.Forms.Button();
            this.loadBtn = new System.Windows.Forms.Button();
            this.persistenceModeLabel = new System.Windows.Forms.Label();
            this.historyPanel = new System.Windows.Forms.TableLayoutPanel();
            this.historyHeaderLabel = new System.Windows.Forms.Label();
            this.undoBtn = new System.Windows.Forms.Button();
            this.redoBtn = new System.Windows.Forms.Button();
            this.goToStateBtn = new System.Windows.Forms.Button();
            this.moveHistoryListBox = new System.Windows.Forms.ListBox();
            this.rulesHeaderLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.rootLayout.SuspendLayout();
            this.controlsPanel.SuspendLayout();
            this.historyPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // rootLayout
            // 
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.controlsPanel, 0, 0);
            this.rootLayout.Controls.Add(this.historyPanel, 0, 1);
            this.rootLayout.Controls.Add(this.rulesHeaderLabel, 0, 2);
            this.rootLayout.Controls.Add(this.flowLayoutPanel1, 0, 3);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(0, 0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.Padding = new System.Windows.Forms.Padding(10, 10, 10, 10);
            this.rootLayout.RowCount = 4;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 176F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 280F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Size = new System.Drawing.Size(326, 705);
            this.rootLayout.TabIndex = 0;
            // 
            // controlsPanel
            // 
            this.controlsPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.controlsPanel.ColumnCount = 1;
            this.controlsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.controlsPanel.Controls.Add(this.controlsHeaderLabel, 0, 0);
            this.controlsPanel.Controls.Add(this.newGameBtn, 0, 1);
            this.controlsPanel.Controls.Add(this.saveBtn, 0, 2);
            this.controlsPanel.Controls.Add(this.loadBtn, 0, 3);
            this.controlsPanel.Controls.Add(this.persistenceModeLabel, 0, 4);
            this.controlsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlsPanel.Location = new System.Drawing.Point(13, 13);
            this.controlsPanel.Name = "controlsPanel";
            this.controlsPanel.Padding = new System.Windows.Forms.Padding(8);
            this.controlsPanel.RowCount = 5;
            this.controlsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.controlsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.controlsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.controlsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.controlsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.controlsPanel.Size = new System.Drawing.Size(300, 170);
            this.controlsPanel.TabIndex = 0;
            // 
            // controlsHeaderLabel
            // 
            this.controlsHeaderLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlsHeaderLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.controlsHeaderLabel.Location = new System.Drawing.Point(11, 8);
            this.controlsHeaderLabel.Name = "controlsHeaderLabel";
            this.controlsHeaderLabel.Size = new System.Drawing.Size(278, 24);
            this.controlsHeaderLabel.TabIndex = 0;
            this.controlsHeaderLabel.Text = "Game Controls";
            this.controlsHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // newGameBtn
            // 
            this.newGameBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.newGameBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.newGameBtn.Location = new System.Drawing.Point(11, 35);
            this.newGameBtn.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.newGameBtn.Name = "newGameBtn";
            this.newGameBtn.Size = new System.Drawing.Size(278, 25);
            this.newGameBtn.TabIndex = 1;
            this.newGameBtn.Text = "New Game";
            this.newGameBtn.UseVisualStyleBackColor = true;
            this.newGameBtn.Click += new System.EventHandler(this.newGameBtn_Click);
            // 
            // saveBtn
            // 
            this.saveBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.saveBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveBtn.Location = new System.Drawing.Point(11, 69);
            this.saveBtn.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(278, 25);
            this.saveBtn.TabIndex = 2;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // loadBtn
            // 
            this.loadBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.loadBtn.Location = new System.Drawing.Point(11, 103);
            this.loadBtn.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.loadBtn.Name = "loadBtn";
            this.loadBtn.Size = new System.Drawing.Size(278, 25);
            this.loadBtn.TabIndex = 3;
            this.loadBtn.Text = "Load";
            this.loadBtn.UseVisualStyleBackColor = true;
            this.loadBtn.Click += new System.EventHandler(this.loadBtn_Click);
            // 
            // persistenceModeLabel
            // 
            this.persistenceModeLabel.BackColor = System.Drawing.Color.White;
            this.persistenceModeLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.persistenceModeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.persistenceModeLabel.Location = new System.Drawing.Point(11, 137);
            this.persistenceModeLabel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.persistenceModeLabel.Name = "persistenceModeLabel";
            this.persistenceModeLabel.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.persistenceModeLabel.Size = new System.Drawing.Size(278, 33);
            this.persistenceModeLabel.TabIndex = 4;
            this.persistenceModeLabel.Text = "Persistence: Memory only";
            this.persistenceModeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // historyPanel
            // 
            this.historyPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.historyPanel.ColumnCount = 1;
            this.historyPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.historyPanel.Controls.Add(this.historyHeaderLabel, 0, 0);
            this.historyPanel.Controls.Add(this.undoBtn, 0, 1);
            this.historyPanel.Controls.Add(this.redoBtn, 0, 2);
            this.historyPanel.Controls.Add(this.goToStateBtn, 0, 3);
            this.historyPanel.Controls.Add(this.moveHistoryListBox, 0, 4);
            this.historyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyPanel.Location = new System.Drawing.Point(13, 189);
            this.historyPanel.Name = "historyPanel";
            this.historyPanel.Padding = new System.Windows.Forms.Padding(8);
            this.historyPanel.RowCount = 5;
            this.historyPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.historyPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.historyPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.historyPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.historyPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.historyPanel.Size = new System.Drawing.Size(300, 274);
            this.historyPanel.TabIndex = 1;
            // 
            // historyHeaderLabel
            // 
            this.historyHeaderLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyHeaderLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.historyHeaderLabel.Location = new System.Drawing.Point(11, 8);
            this.historyHeaderLabel.Name = "historyHeaderLabel";
            this.historyHeaderLabel.Size = new System.Drawing.Size(278, 24);
            this.historyHeaderLabel.TabIndex = 0;
            this.historyHeaderLabel.Text = "Move History";
            this.historyHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // undoBtn
            // 
            this.undoBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.undoBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.undoBtn.Location = new System.Drawing.Point(11, 35);
            this.undoBtn.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.undoBtn.Name = "undoBtn";
            this.undoBtn.Size = new System.Drawing.Size(278, 25);
            this.undoBtn.TabIndex = 1;
            this.undoBtn.Text = "Undo";
            this.undoBtn.UseVisualStyleBackColor = true;
            this.undoBtn.Click += new System.EventHandler(this.undoBtn_Click);
            // 
            // redoBtn
            // 
            this.redoBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.redoBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.redoBtn.Location = new System.Drawing.Point(11, 69);
            this.redoBtn.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.redoBtn.Name = "redoBtn";
            this.redoBtn.Size = new System.Drawing.Size(278, 25);
            this.redoBtn.TabIndex = 2;
            this.redoBtn.Text = "Redo";
            this.redoBtn.UseVisualStyleBackColor = true;
            this.redoBtn.Click += new System.EventHandler(this.redoBtn_Click);
            // 
            // goToStateBtn
            // 
            this.goToStateBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.goToStateBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.goToStateBtn.Location = new System.Drawing.Point(11, 103);
            this.goToStateBtn.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.goToStateBtn.Name = "goToStateBtn";
            this.goToStateBtn.Size = new System.Drawing.Size(278, 25);
            this.goToStateBtn.TabIndex = 3;
            this.goToStateBtn.Text = "Go To Selected State";
            this.goToStateBtn.UseVisualStyleBackColor = true;
            this.goToStateBtn.Click += new System.EventHandler(this.goToStateBtn_Click);
            // 
            // moveHistoryListBox
            // 
            this.moveHistoryListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.moveHistoryListBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.moveHistoryListBox.FormattingEnabled = true;
            this.moveHistoryListBox.IntegralHeight = false;
            this.moveHistoryListBox.ItemHeight = 13;
            this.moveHistoryListBox.Location = new System.Drawing.Point(11, 137);
            this.moveHistoryListBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.moveHistoryListBox.Name = "moveHistoryListBox";
            this.moveHistoryListBox.Size = new System.Drawing.Size(278, 129);
            this.moveHistoryListBox.TabIndex = 4;
            this.moveHistoryListBox.SelectedIndexChanged += new System.EventHandler(this.moveHistoryListBox_SelectedIndexChanged);
            // 
            // rulesHeaderLabel
            // 
            this.rulesHeaderLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rulesHeaderLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rulesHeaderLabel.Location = new System.Drawing.Point(13, 466);
            this.rulesHeaderLabel.Name = "rulesHeaderLabel";
            this.rulesHeaderLabel.Size = new System.Drawing.Size(300, 28);
            this.rulesHeaderLabel.TabIndex = 2;
            this.rulesHeaderLabel.Text = "Rules";
            this.rulesHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(13, 497);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(300, 198);
            this.flowLayoutPanel1.TabIndex = 3;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // MenuPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rootLayout);
            this.Name = "MenuPanel";
            this.Size = new System.Drawing.Size(326, 705);
            this.rootLayout.ResumeLayout(false);
            this.controlsPanel.ResumeLayout(false);
            this.historyPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private System.Windows.Forms.TableLayoutPanel controlsPanel;
        private System.Windows.Forms.Label controlsHeaderLabel;
        private System.Windows.Forms.Button newGameBtn;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.Button loadBtn;
        private System.Windows.Forms.Label persistenceModeLabel;
        private System.Windows.Forms.TableLayoutPanel historyPanel;
        private System.Windows.Forms.Label historyHeaderLabel;
        private System.Windows.Forms.Button undoBtn;
        private System.Windows.Forms.Button redoBtn;
        private System.Windows.Forms.Button goToStateBtn;
        private System.Windows.Forms.ListBox moveHistoryListBox;
        private System.Windows.Forms.Label rulesHeaderLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
