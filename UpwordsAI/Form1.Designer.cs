namespace UpwordsAI
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.giveaitilesBUT = new System.Windows.Forms.Button();
            this.aiplacewordBUT = new System.Windows.Forms.Button();
            this.clearaitilesBUT = new System.Windows.Forms.Button();
            this.clearboardBUT = new System.Windows.Forms.Button();
            this.mouseposLBL = new System.Windows.Forms.Label();
            this.aiplayallBUT = new System.Windows.Forms.Button();
            this.logboxTB = new System.Windows.Forms.TextBox();
            this.resetgameBUT = new System.Windows.Forms.Button();
            this.stopautoplayBUT = new System.Windows.Forms.Button();
            this.aicontplayBUT = new System.Windows.Forms.Button();
            this.aiplaystackBUT = new System.Windows.Forms.Button();
            this.scoreLBL = new System.Windows.Forms.Label();
            this.ipaddressTB = new System.Windows.Forms.TextBox();
            this.portUD = new System.Windows.Forms.NumericUpDown();
            this.sendgetBUT = new System.Windows.Forms.Button();
            this.sendmoveBUT = new System.Windows.Forms.Button();
            this.getstateBUT = new System.Windows.Forms.Button();
            this.aiplaybestBUT = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.tournamentscoreLBL = new System.Windows.Forms.Label();
            this.sendonemoveBUT = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.portUD)).BeginInit();
            this.SuspendLayout();
            // 
            // giveaitilesBUT
            // 
            this.giveaitilesBUT.Location = new System.Drawing.Point(347, 13);
            this.giveaitilesBUT.Name = "giveaitilesBUT";
            this.giveaitilesBUT.Size = new System.Drawing.Size(84, 23);
            this.giveaitilesBUT.TabIndex = 0;
            this.giveaitilesBUT.Text = "Give AI Tiles";
            this.giveaitilesBUT.UseVisualStyleBackColor = true;
            this.giveaitilesBUT.Click += new System.EventHandler(this.giveaitilesBUT_Click);
            // 
            // aiplacewordBUT
            // 
            this.aiplacewordBUT.Location = new System.Drawing.Point(347, 71);
            this.aiplacewordBUT.Name = "aiplacewordBUT";
            this.aiplacewordBUT.Size = new System.Drawing.Size(84, 23);
            this.aiplacewordBUT.TabIndex = 1;
            this.aiplacewordBUT.Text = "AI Place Word";
            this.aiplacewordBUT.UseVisualStyleBackColor = true;
            this.aiplacewordBUT.Click += new System.EventHandler(this.aiplacewordBUT_Click);
            // 
            // clearaitilesBUT
            // 
            this.clearaitilesBUT.Location = new System.Drawing.Point(347, 42);
            this.clearaitilesBUT.Name = "clearaitilesBUT";
            this.clearaitilesBUT.Size = new System.Drawing.Size(84, 23);
            this.clearaitilesBUT.TabIndex = 2;
            this.clearaitilesBUT.Text = "Clear AI Tiles";
            this.clearaitilesBUT.UseVisualStyleBackColor = true;
            this.clearaitilesBUT.Click += new System.EventHandler(this.clearaitilesBUT_Click);
            // 
            // clearboardBUT
            // 
            this.clearboardBUT.Location = new System.Drawing.Point(347, 237);
            this.clearboardBUT.Name = "clearboardBUT";
            this.clearboardBUT.Size = new System.Drawing.Size(84, 23);
            this.clearboardBUT.TabIndex = 3;
            this.clearboardBUT.Text = "Clear Board";
            this.clearboardBUT.UseVisualStyleBackColor = true;
            this.clearboardBUT.Click += new System.EventHandler(this.clearboardBUT_Click);
            // 
            // mouseposLBL
            // 
            this.mouseposLBL.AutoSize = true;
            this.mouseposLBL.Location = new System.Drawing.Point(344, 268);
            this.mouseposLBL.Name = "mouseposLBL";
            this.mouseposLBL.Size = new System.Drawing.Size(71, 13);
            this.mouseposLBL.TabIndex = 4;
            this.mouseposLBL.Text = "Position: (0,0)";
            // 
            // aiplayallBUT
            // 
            this.aiplayallBUT.Location = new System.Drawing.Point(347, 179);
            this.aiplayallBUT.Name = "aiplayallBUT";
            this.aiplayallBUT.Size = new System.Drawing.Size(84, 23);
            this.aiplayallBUT.TabIndex = 7;
            this.aiplayallBUT.Text = "AI Play All";
            this.aiplayallBUT.UseVisualStyleBackColor = true;
            this.aiplayallBUT.Click += new System.EventHandler(this.aiplayallBUT_Click);
            // 
            // logboxTB
            // 
            this.logboxTB.Location = new System.Drawing.Point(438, 13);
            this.logboxTB.Multiline = true;
            this.logboxTB.Name = "logboxTB";
            this.logboxTB.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logboxTB.Size = new System.Drawing.Size(280, 346);
            this.logboxTB.TabIndex = 8;
            // 
            // resetgameBUT
            // 
            this.resetgameBUT.Location = new System.Drawing.Point(347, 208);
            this.resetgameBUT.Name = "resetgameBUT";
            this.resetgameBUT.Size = new System.Drawing.Size(84, 23);
            this.resetgameBUT.TabIndex = 9;
            this.resetgameBUT.Text = "Reset Game";
            this.resetgameBUT.UseVisualStyleBackColor = true;
            this.resetgameBUT.Click += new System.EventHandler(this.resetgameBUT_Click);
            // 
            // stopautoplayBUT
            // 
            this.stopautoplayBUT.Location = new System.Drawing.Point(347, 336);
            this.stopautoplayBUT.Name = "stopautoplayBUT";
            this.stopautoplayBUT.Size = new System.Drawing.Size(84, 23);
            this.stopautoplayBUT.TabIndex = 10;
            this.stopautoplayBUT.Text = "STOP";
            this.stopautoplayBUT.UseVisualStyleBackColor = true;
            this.stopautoplayBUT.Click += new System.EventHandler(this.stopautoplayBUT_Click);
            // 
            // aicontplayBUT
            // 
            this.aicontplayBUT.Location = new System.Drawing.Point(347, 307);
            this.aicontplayBUT.Name = "aicontplayBUT";
            this.aicontplayBUT.Size = new System.Drawing.Size(84, 23);
            this.aicontplayBUT.TabIndex = 11;
            this.aicontplayBUT.Text = "AI Cont Play";
            this.aicontplayBUT.UseVisualStyleBackColor = true;
            this.aicontplayBUT.Click += new System.EventHandler(this.aicontplayBUT_Click);
            // 
            // aiplaystackBUT
            // 
            this.aiplaystackBUT.Location = new System.Drawing.Point(347, 100);
            this.aiplaystackBUT.Name = "aiplaystackBUT";
            this.aiplaystackBUT.Size = new System.Drawing.Size(84, 23);
            this.aiplaystackBUT.TabIndex = 12;
            this.aiplaystackBUT.Text = "AI Play Stack";
            this.aiplaystackBUT.UseVisualStyleBackColor = true;
            this.aiplaystackBUT.Click += new System.EventHandler(this.aiplaystackBUT_Click);
            // 
            // scoreLBL
            // 
            this.scoreLBL.AutoSize = true;
            this.scoreLBL.Location = new System.Drawing.Point(435, 362);
            this.scoreLBL.Name = "scoreLBL";
            this.scoreLBL.Size = new System.Drawing.Size(41, 13);
            this.scoreLBL.TabIndex = 13;
            this.scoreLBL.Text = "Score: ";
            // 
            // ipaddressTB
            // 
            this.ipaddressTB.Location = new System.Drawing.Point(725, 13);
            this.ipaddressTB.Name = "ipaddressTB";
            this.ipaddressTB.Size = new System.Drawing.Size(159, 20);
            this.ipaddressTB.TabIndex = 14;
            this.ipaddressTB.Text = "localhost";
            // 
            // portUD
            // 
            this.portUD.Location = new System.Drawing.Point(725, 39);
            this.portUD.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.portUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.portUD.Name = "portUD";
            this.portUD.Size = new System.Drawing.Size(120, 20);
            this.portUD.TabIndex = 15;
            this.portUD.Value = new decimal(new int[] {
            62027,
            0,
            0,
            0});
            // 
            // sendgetBUT
            // 
            this.sendgetBUT.Location = new System.Drawing.Point(725, 66);
            this.sendgetBUT.Name = "sendgetBUT";
            this.sendgetBUT.Size = new System.Drawing.Size(75, 23);
            this.sendgetBUT.TabIndex = 16;
            this.sendgetBUT.Text = "Join Game";
            this.sendgetBUT.UseVisualStyleBackColor = true;
            this.sendgetBUT.Click += new System.EventHandler(this.sendgetBUT_Click);
            // 
            // sendmoveBUT
            // 
            this.sendmoveBUT.Location = new System.Drawing.Point(725, 124);
            this.sendmoveBUT.Name = "sendmoveBUT";
            this.sendmoveBUT.Size = new System.Drawing.Size(75, 23);
            this.sendmoveBUT.TabIndex = 17;
            this.sendmoveBUT.Text = "Send Move";
            this.sendmoveBUT.UseVisualStyleBackColor = true;
            this.sendmoveBUT.Click += new System.EventHandler(this.sendmoveBUT_Click);
            // 
            // getstateBUT
            // 
            this.getstateBUT.Location = new System.Drawing.Point(725, 95);
            this.getstateBUT.Name = "getstateBUT";
            this.getstateBUT.Size = new System.Drawing.Size(75, 23);
            this.getstateBUT.TabIndex = 18;
            this.getstateBUT.Text = "Get State";
            this.getstateBUT.UseVisualStyleBackColor = true;
            this.getstateBUT.Click += new System.EventHandler(this.getstateBUT_Click);
            // 
            // aiplaybestBUT
            // 
            this.aiplaybestBUT.Location = new System.Drawing.Point(347, 130);
            this.aiplaybestBUT.Name = "aiplaybestBUT";
            this.aiplaybestBUT.Size = new System.Drawing.Size(84, 23);
            this.aiplaybestBUT.TabIndex = 19;
            this.aiplaybestBUT.Text = "AI Play Best";
            this.aiplaybestBUT.UseVisualStyleBackColor = true;
            this.aiplaybestBUT.Click += new System.EventHandler(this.aiplaybestBUT_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Red;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(725, 208);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(85, 23);
            this.button1.TabIndex = 20;
            this.button1.Text = "TRIGGERED";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tournamentscoreLBL
            // 
            this.tournamentscoreLBL.AutoSize = true;
            this.tournamentscoreLBL.Location = new System.Drawing.Point(438, 379);
            this.tournamentscoreLBL.Name = "tournamentscoreLBL";
            this.tournamentscoreLBL.Size = new System.Drawing.Size(51, 13);
            this.tournamentscoreLBL.TabIndex = 21;
            this.tournamentscoreLBL.Text = "T Score: ";
            // 
            // sendonemoveBUT
            // 
            this.sendonemoveBUT.Location = new System.Drawing.Point(807, 95);
            this.sendonemoveBUT.Name = "sendonemoveBUT";
            this.sendonemoveBUT.Size = new System.Drawing.Size(96, 23);
            this.sendonemoveBUT.TabIndex = 22;
            this.sendonemoveBUT.Text = "Send ONE Move";
            this.sendonemoveBUT.UseVisualStyleBackColor = true;
            this.sendonemoveBUT.Click += new System.EventHandler(this.sendonemoveBUT_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(973, 420);
            this.Controls.Add(this.sendonemoveBUT);
            this.Controls.Add(this.tournamentscoreLBL);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.aiplaybestBUT);
            this.Controls.Add(this.getstateBUT);
            this.Controls.Add(this.sendmoveBUT);
            this.Controls.Add(this.sendgetBUT);
            this.Controls.Add(this.portUD);
            this.Controls.Add(this.ipaddressTB);
            this.Controls.Add(this.scoreLBL);
            this.Controls.Add(this.aiplaystackBUT);
            this.Controls.Add(this.aicontplayBUT);
            this.Controls.Add(this.stopautoplayBUT);
            this.Controls.Add(this.resetgameBUT);
            this.Controls.Add(this.logboxTB);
            this.Controls.Add(this.aiplayallBUT);
            this.Controls.Add(this.mouseposLBL);
            this.Controls.Add(this.clearboardBUT);
            this.Controls.Add(this.clearaitilesBUT);
            this.Controls.Add(this.aiplacewordBUT);
            this.Controls.Add(this.giveaitilesBUT);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Upwords - AI Player";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.portUD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button giveaitilesBUT;
        private System.Windows.Forms.Button aiplacewordBUT;
        private System.Windows.Forms.Button clearaitilesBUT;
        private System.Windows.Forms.Button clearboardBUT;
        private System.Windows.Forms.Label mouseposLBL;
        private System.Windows.Forms.Button aiplayallBUT;
        private System.Windows.Forms.TextBox logboxTB;
        private System.Windows.Forms.Button resetgameBUT;
        private System.Windows.Forms.Button stopautoplayBUT;
        private System.Windows.Forms.Button aicontplayBUT;
        private System.Windows.Forms.Button aiplaystackBUT;
        private System.Windows.Forms.Label scoreLBL;
        private System.Windows.Forms.TextBox ipaddressTB;
        private System.Windows.Forms.NumericUpDown portUD;
        private System.Windows.Forms.Button sendgetBUT;
        private System.Windows.Forms.Button sendmoveBUT;
        private System.Windows.Forms.Button getstateBUT;
        private System.Windows.Forms.Button aiplaybestBUT;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label tournamentscoreLBL;
        private System.Windows.Forms.Button sendonemoveBUT;
    }
}

