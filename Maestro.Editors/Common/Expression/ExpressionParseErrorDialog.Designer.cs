﻿namespace Maestro.Editors.Common.Expression
{
    partial class ExpressionParseErrorDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExpressionParseErrorDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.txtErrorDetails = new System.Windows.Forms.TextBox();
            this.btnGotoError = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // txtErrorDetails
            // 
            resources.ApplyResources(this.txtErrorDetails, "txtErrorDetails");
            this.txtErrorDetails.Name = "txtErrorDetails";
            this.txtErrorDetails.ReadOnly = true;
            // 
            // btnGotoError
            // 
            resources.ApplyResources(this.btnGotoError, "btnGotoError");
            this.btnGotoError.Name = "btnGotoError";
            this.btnGotoError.UseVisualStyleBackColor = true;
            this.btnGotoError.Click += new System.EventHandler(this.btnGotoError_Click);
            // 
            // btnClose
            // 
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.Name = "btnClose";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // ExpressionParseErrorDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnGotoError);
            this.Controls.Add(this.txtErrorDetails);
            this.Controls.Add(this.label1);
            this.Name = "ExpressionParseErrorDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtErrorDetails;
        private System.Windows.Forms.Button btnGotoError;
        private System.Windows.Forms.Button btnClose;
    }
}