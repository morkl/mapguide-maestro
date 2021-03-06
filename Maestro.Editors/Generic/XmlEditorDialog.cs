﻿#region Disclaimer / License

// Copyright (C) 2010, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
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

#endregion Disclaimer / License

using OSGeo.MapGuide.MaestroAPI;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;

#pragma warning disable 1591

namespace Maestro.Editors.Generic
{
    /// <summary>
    /// A generic XML editor for any resource, housed within a dialog
    /// </summary>
    /// <remarks>
    /// Although public, this class is undocumented and reserved for internal use by built-in Maestro AddIns
    /// </remarks>
    public partial class XmlEditorDialog : Form, INotifyResourceChanged
    {
        private XmlEditorCtrl _ed;
        private IEditorService _edSvc;

        internal XmlEditorDialog()
        {
            InitializeComponent();
            this.OnlyValidateWellFormedness = false;
            _ed = new XmlEditorCtrl();
            _ed.Validator = new XmlValidationCallback(ValidateXml);
            _ed.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(_ed);
            this.XsdPath = XmlEditorSettings.XsdPath;
        }

        public string XsdPath
        {
            get;
            set;
        }

        public XmlEditorDialog(IEditorService edsvc)
            : this()
        {
            //NRE happens if we copy without setting a font first
            _ed.TextFont = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            _edSvc = edsvc;
            _edSvc.RegisterCustomNotifier(this);
            this.Disposed += new EventHandler(OnDisposed);
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            //Same as EditorBindableCollapsiblePanel.UnsubscribeEventHandlers()
            var handler = this.ResourceChanged;
            if (handler != null)
            {
                foreach (var h in handler.GetInvocationList())
                {
                    this.ResourceChanged -= (EventHandler)h;
                }
                //In case we left out something (shouldn't be)
                this.ResourceChanged = null;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (_edSvc != null)
                _ed.InitResourceData(_edSvc);
        }

        /// <summary>
        /// Gets the type of the edited resource
        /// </summary>
        public string ResourceType
        {
            get;
            private set;
        }

        /// <summary>
        /// Sets the XML content and the type of resource this XML content is supposed to represent
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="type"></param>
        public void SetXmlContent(string xml, string type)
        {
            _ed.XmlContent = xml;
            this.ResourceType = type;
        }

        private string _lastSnapshot;

        /// <summary>
        /// Gets or sets the XML content for this dialog.
        /// </summary>
        public string XmlContent
        {
            get { return _ed.XmlContent; }
            set { _ed.XmlContent = _lastSnapshot = value; }
        }

        private XmlSchema GetXsd(string xsdFile)
        {
            string path = xsdFile;

            if (!string.IsNullOrEmpty(this.XsdPath))
                path = Path.Combine(this.XsdPath, xsdFile);

            if (File.Exists(path))
            {
                ValidationEventHandler handler = (s, e) =>
                {
                };
                return XmlSchema.Read(File.OpenRead(path), handler);
            }
            return null;
        }

        private void ValidateXml(out string[] errors, out string[] warnings)
        {
            if (this.OnlyValidateWellFormedness)
            {
                errors = new string[0];
                warnings = new string[0];

                //Test for well-formedness
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(this.XmlContent);
                }
                catch (XmlException ex)
                {
                    errors = new string[] { ex.Message };
                }
            }
            else
            {
                XmlValidator.ValidateResourceXmlContent(this.XmlContent, this.XsdPath, out errors, out warnings);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_ed.PerformValidation(true, true))
            {
                if (_lastSnapshot != _ed.XmlContent)
                    OnResourceChanged();
                this.DialogResult = DialogResult.OK;
            }
        }

        private void OnResourceChanged()
        {
            var handler = this.ResourceChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler ResourceChanged;

        public bool OnlyValidateWellFormedness { get; set; }
    }
}