namespace Maestro.Editors.Fusion
{
    partial class WidgetReferenceCtrl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WidgetReferenceCtrl));
            this.label1 = new System.Windows.Forms.Label();
            this.cmbWidgetRefs = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cmbWidgetRefs
            // 
            resources.ApplyResources(this.cmbWidgetRefs, "cmbWidgetRefs");
            this.cmbWidgetRefs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbWidgetRefs.FormattingEnabled = true;
            this.cmbWidgetRefs.Name = "cmbWidgetRefs";
            this.cmbWidgetRefs.SelectedIndexChanged += new System.EventHandler(this.cmbWidgetRefs_SelectedIndexChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // WidgetReferenceCtrl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbWidgetRefs);
            this.Controls.Add(this.label1);
            this.Name = "WidgetReferenceCtrl";
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbWidgetRefs;
        private System.Windows.Forms.Label label2;
    }
}
