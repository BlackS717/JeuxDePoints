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
            this.newGameBtn = new System.Windows.Forms.Button();
            this.saveBtn = new System.Windows.Forms.Button();
            this.loadBtn = new System.Windows.Forms.Button();
            this.persistenceModeLabel = new System.Windows.Forms.Label();
            this.undoBtn = new System.Windows.Forms.Button();
            this.redoBtn = new System.Windows.Forms.Button();
            this.goToStateBtn = new System.Windows.Forms.Button();
            this.moveHistoryListBox = new System.Windows.Forms.ListBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // newGameBtn
            // 
            this.newGameBtn.AutoSize = true;
            this.newGameBtn.Dock = System.Windows.Forms.DockStyle.Top;
            this.newGameBtn.Location = new System.Drawing.Point(0, 0);
            this.newGameBtn.Name = "newGameBtn";
            this.newGameBtn.Size = new System.Drawing.Size(326, 40);
            this.newGameBtn.TabIndex = 0;
            this.newGameBtn.Text = "Nouvelle Partie";
            this.newGameBtn.UseVisualStyleBackColor = true;
            this.newGameBtn.Click += new System.EventHandler(this.newGameBtn_Click);
            // 
            // saveBtn
            // 
            this.saveBtn.AutoSize = true;
            this.saveBtn.Dock = System.Windows.Forms.DockStyle.Top;
            this.saveBtn.Location = new System.Drawing.Point(0, 40);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(326, 40);
            this.saveBtn.TabIndex = 1;
            this.saveBtn.Text = "Sauvegarder";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // loadBtn
            // 
            this.loadBtn.AutoSize = true;
            this.loadBtn.Dock = System.Windows.Forms.DockStyle.Top;
            this.loadBtn.Location = new System.Drawing.Point(0, 80);
            this.loadBtn.Name = "loadBtn";
            this.loadBtn.Size = new System.Drawing.Size(326, 40);
            this.loadBtn.TabIndex = 2;
            this.loadBtn.Text = "Charger";
            this.loadBtn.UseVisualStyleBackColor = true;
            this.loadBtn.Click += new System.EventHandler(this.loadBtn_Click);
            // 
            // persistenceModeLabel
            // 
            this.persistenceModeLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.persistenceModeLabel.Location = new System.Drawing.Point(0, 120);
            this.persistenceModeLabel.Name = "persistenceModeLabel";
            this.persistenceModeLabel.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.persistenceModeLabel.Size = new System.Drawing.Size(326, 24);
            this.persistenceModeLabel.TabIndex = 3;
            this.persistenceModeLabel.Text = "Persistence: Memory only";
            // 
            // undoBtn
            // 
            this.undoBtn.AutoSize = true;
            this.undoBtn.Dock = System.Windows.Forms.DockStyle.Top;
            this.undoBtn.Location = new System.Drawing.Point(0, 144);
            this.undoBtn.Name = "undoBtn";
            this.undoBtn.Size = new System.Drawing.Size(326, 40);
            this.undoBtn.TabIndex = 4;
            this.undoBtn.Text = "Undo";
            this.undoBtn.UseVisualStyleBackColor = true;
            this.undoBtn.Click += new System.EventHandler(this.undoBtn_Click);
            // 
            // redoBtn
            // 
            this.redoBtn.AutoSize = true;
            this.redoBtn.Dock = System.Windows.Forms.DockStyle.Top;
            this.redoBtn.Location = new System.Drawing.Point(0, 184);
            this.redoBtn.Name = "redoBtn";
            this.redoBtn.Size = new System.Drawing.Size(326, 40);
            this.redoBtn.TabIndex = 5;
            this.redoBtn.Text = "Redo";
            this.redoBtn.UseVisualStyleBackColor = true;
            this.redoBtn.Click += new System.EventHandler(this.redoBtn_Click);
            // 
            // goToStateBtn
            // 
            this.goToStateBtn.AutoSize = true;
            this.goToStateBtn.Dock = System.Windows.Forms.DockStyle.Top;
            this.goToStateBtn.Location = new System.Drawing.Point(0, 224);
            this.goToStateBtn.Name = "goToStateBtn";
            this.goToStateBtn.Size = new System.Drawing.Size(326, 40);
            this.goToStateBtn.TabIndex = 6;
            this.goToStateBtn.Text = "Go To Selected State";
            this.goToStateBtn.UseVisualStyleBackColor = true;
            this.goToStateBtn.Click += new System.EventHandler(this.goToStateBtn_Click);
            // 
            // moveHistoryListBox
            // 
            this.moveHistoryListBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.moveHistoryListBox.FormattingEnabled = true;
            this.moveHistoryListBox.Location = new System.Drawing.Point(0, 264);
            this.moveHistoryListBox.Name = "moveHistoryListBox";
            this.moveHistoryListBox.Size = new System.Drawing.Size(326, 95);
            this.moveHistoryListBox.TabIndex = 7;
            this.moveHistoryListBox.SelectedIndexChanged += new System.EventHandler(this.moveHistoryListBox_SelectedIndexChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 359);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(326, 346);
            this.flowLayoutPanel1.TabIndex = 8;
            // 
            // MenuPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.moveHistoryListBox);
            this.Controls.Add(this.goToStateBtn);
            this.Controls.Add(this.redoBtn);
            this.Controls.Add(this.undoBtn);
            this.Controls.Add(this.persistenceModeLabel);
            this.Controls.Add(this.loadBtn);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.newGameBtn);
            this.Name = "MenuPanel";
            this.Size = new System.Drawing.Size(326, 705);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button newGameBtn;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.Button loadBtn;
        private System.Windows.Forms.Label persistenceModeLabel;
        private System.Windows.Forms.Button undoBtn;
        private System.Windows.Forms.Button redoBtn;
        private System.Windows.Forms.Button goToStateBtn;
        private System.Windows.Forms.ListBox moveHistoryListBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
