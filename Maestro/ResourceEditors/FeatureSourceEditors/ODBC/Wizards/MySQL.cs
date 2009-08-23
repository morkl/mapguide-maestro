#region Disclaimer / License
// Copyright (C) 2009, Kenneth Skovhede
// http://www.hexad.dk, opensource@hexad.dk
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
// 
#endregion
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Collections.Specialized;
using OSGeo.MapGuide.Maestro;

namespace OSGeo.MapGuide.Maestro.ResourceEditors.FeatureSourceEditors.ODBC.Wizards
{
	/// <summary>
	/// Summary description for MySQL.
	/// </summary>
	public class MySQL : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.TextBox Database;
		private System.Windows.Forms.TextBox Server;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private ResourceEditors.EditorInterface m_editor = null;
		private OSGeo.MapGuide.MaestroAPI.FeatureSource m_item = null;
		private System.Windows.Forms.ComboBox Option;
		private System.Windows.Forms.TextBox Port;
		private bool m_isUpdating = false;
		public event FeatureSourceEditorODBC.ConnectionStringUpdatedDelegate ConnectionStringUpdated;

		public MySQL()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		public void SetItem(ResourceEditors.EditorInterface editor, OSGeo.MapGuide.MaestroAPI.FeatureSource item)
		{
			m_editor = editor;
			m_item = item;
			UpdateDisplay();
		}

		public void UpdateDisplay()
		{
			if (m_item == null)
				return;

			try
			{
				m_isUpdating = true;
				NameValueCollection nv = ConnectionStringManager.SplitConnectionString(m_item.Parameter["ConnectionString"]);
				ConnectionStringManager.InsertDefaultValues(nv, "{MySQL ODBC 3.51 Driver}");
				Server.Text = nv["Server"];
				Port.Text = nv["Port"];
				Database.Text = nv["Database"];
				Option.Text = nv["Option"];
			}
			finally
			{
				m_isUpdating = false;
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Option = new System.Windows.Forms.ComboBox();
			this.Database = new System.Windows.Forms.TextBox();
			this.Port = new System.Windows.Forms.TextBox();
			this.Server = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// Option
			// 
			this.Option.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.Option.Items.AddRange(new object[] {
														"3"});
			this.Option.Location = new System.Drawing.Point(152, 78);
			this.Option.Name = "Option";
			this.Option.Size = new System.Drawing.Size(200, 21);
			this.Option.TabIndex = 20;
			this.Option.Text = "3";
			this.Option.TextChanged += new System.EventHandler(this.PropertyText_Changed);
			// 
			// Database
			// 
			this.Database.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.Database.Location = new System.Drawing.Point(152, 56);
			this.Database.Name = "Database";
			this.Database.Size = new System.Drawing.Size(200, 20);
			this.Database.TabIndex = 19;
			this.Database.Text = "textBox5";
			this.Database.TextChanged += new System.EventHandler(this.PropertyText_Changed);
			// 
			// Port
			// 
			this.Port.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.Port.Location = new System.Drawing.Point(152, 32);
			this.Port.Name = "Port";
			this.Port.Size = new System.Drawing.Size(200, 20);
			this.Port.TabIndex = 18;
			this.Port.Text = "textBox3";
			this.Port.TextChanged += new System.EventHandler(this.PropertyText_Changed);
			// 
			// Server
			// 
			this.Server.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.Server.Location = new System.Drawing.Point(152, 8);
			this.Server.Name = "Server";
			this.Server.Size = new System.Drawing.Size(200, 20);
			this.Server.TabIndex = 17;
			this.Server.Text = "textBox2";
			this.Server.TextChanged += new System.EventHandler(this.PropertyText_Changed);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 56);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(136, 16);
			this.label5.TabIndex = 15;
			this.label5.Text = "Database";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 78);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(136, 16);
			this.label4.TabIndex = 14;
			this.label4.Text = "Option";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 32);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(136, 16);
			this.label3.TabIndex = 13;
			this.label3.Text = "Port";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(136, 16);
			this.label2.TabIndex = 12;
			this.label2.Text = "Server";
			// 
			// MySQL
			// 
			this.Controls.Add(this.Option);
			this.Controls.Add(this.Database);
			this.Controls.Add(this.Port);
			this.Controls.Add(this.Server);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Name = "MySQL";
			this.Size = new System.Drawing.Size(360, 104);
			this.ResumeLayout(false);

		}
		#endregion

		private void PropertyText_Changed(object sender, System.EventArgs e)
		{
			if (m_item == null || m_isUpdating)
				return;

			NameValueCollection prev = ConnectionStringManager.SplitConnectionString(m_item.Parameter["ConnectionString"]);
			NameValueCollection nv = new NameValueCollection();
			nv["Driver"] = "{MySQL ODBC 3.51 Driver}";
			nv["Server"] = Server.Text;
			nv["Port"] = Port.Text;
			nv["Option"] = Option.Text;
			nv["Database"] = Database.Text;
			ConnectionStringManager.MergeCredentialsIntoConnectionString(prev, nv);

			m_item.Parameter["ConnectionString"] = ConnectionStringManager.JoinConnectionString(nv);

			if (ConnectionStringUpdated != null)
				ConnectionStringUpdated(m_item.Parameter["ConnectionString"]);

			m_editor.HasChanged();
		
		}
	}
}
