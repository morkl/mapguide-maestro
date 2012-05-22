﻿#region Disclaimer / License
// Copyright (C) 2010, Jackie Ng
// http://trac.osgeo.org/mapguide/wiki/maestro, jumpinjackie@gmail.com
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using OSGeo.MapGuide.MaestroAPI.Resource;
using OSGeo.MapGuide.MaestroAPI;
using Maestro.Editors;
using ICSharpCode.Core;
using OSGeo.MapGuide.MaestroAPI.Resource.Validation;
using Maestro.Base.UI;
using Maestro.Base.UI.Preferences;
using Maestro.Shared.UI;
using OSGeo.MapGuide.MaestroAPI.Resource.Conversion;
using System.IO;
using Maestro.Base.Services;

namespace Maestro.Base.Editor
{
    public partial class EditorContentBase : ViewContentBase, IEditorViewContent
    {
        public EditorContentBase()
        {
            InitializeComponent();
        }

        public bool CanUpgrade
        {
            get { return upgradePanel.Visible; }
            private set { upgradePanel.Visible = value; }
        }

        private IEditorService _svc;

        public IEditorService EditorService
        {
            get
            {
                return _svc;
            }
            set
            {
                if (_svc != null)
                {
                    //Just being responsible
                    _svc.DirtyStateChanged -= OnDirtyStateChanged;
                    _svc.Saved -= OnSaved;
                    _svc.BeforeSave -= OnBeforeSave;
                }

                _svc = value;
                _svc.DirtyStateChanged += OnDirtyStateChanged;
                _svc.Saved += OnSaved;
                _svc.BeforeSave += OnBeforeSave;

                UpdateTitle();
                
                this.CanUpgrade = _svc.IsUpgradeAvailable;

                Bind(_svc);
                //Do dirty state check
                OnDirtyStateChanged(this, EventArgs.Empty);

                //This is to ensure that save works when returning from
                //XML edit mode
                this.Focus();
            }
        }

        /// <summary>
        /// Gets the XML content of the edited resource
        /// </summary>
        /// <returns></returns>
        public virtual string GetXmlContent()
        {
            using (var sr = new System.IO.StreamReader(ResourceTypeRegistry.Serialize(this.Resource)))
            {
                return sr.ReadToEnd();
            }
        }

        public IResource Resource { get { return this.EditorService.GetEditedResource(); } }

        /// <summary>
        /// Performs any pre-save validation logic. The base implementation performs
        /// a <see cref="ResourceValidatorSet"/> validation (non-casccading) on the 
        /// edited resource before attempting a save into the session repository 
        /// (triggering any errors relating to invalid XML content). Override this
        /// method if the base implementation just described does not cover your 
        /// validation needs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnBeforeSave(object sender, CancelEventArgs e) 
        {
            //We've been editing an in-memory model of the session copy
            //so we need to save this model back to the session copy before Save()
            //commits the changes back to the original resource
            _svc.UpdateResourceContent(GetXmlContent());
            try
            {
                var validate = PropertyService.Get(ConfigProperties.ValidateOnSave, true);
                if (this.IsDirty && validate)
                {
                    BusyWaitDelegate del = () => 
                    {
                        var errors = new List<ValidationIssue>(ValidateEditedResource()).ToArray();
                        return errors;
                    };
                    
                    BusyWaitDialog.Run(Properties.Resources.PrgPreSaveValidation, del, (result) => {
                        ValidationIssue[] errors = result as ValidationIssue[];
                        if (errors.Length > 0)
                        {
                            MessageService.ShowError(Properties.Resources.FixErrorsBeforeSaving);
                            ValidationResultsDialog diag = new ValidationResultsDialog(this.Resource.ResourceID, errors);
                            diag.ShowDialog(Workbench.Instance);
                            e.Cancel = true;
                        }
                        else
                        {
                            e.Cancel = false;
                        }               
                    });
                }
                else
                {
                    LoggingService.Info("Skipping validation on save");
                    e.Cancel = false;
                }
            }
            catch (Exception ex)
            {
                ErrorDialog.Show(ex);
                e.Cancel = true;
            }
        }

        public override bool IsExclusiveToDocumentRegion
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Performs any pre-save validation on the currently edited resource, by default
        /// this returns the results of a non-cascading <see cref="ResourceValidatorSet"/>
        /// validation run. Override this if you have a custom method of validation.
        /// </summary>
        protected virtual ICollection<ValidationIssue> ValidateEditedResource() 
        {
            var context = new ResourceValidationContext(_svc.ResourceService, _svc.FeatureService);
            //Don't recurse as we only want to validate the current resource
            var issues = ResourceValidatorSet.Validate(context, this.Resource, false);
            var set = new ValidationResultSet(issues);

            var errors = set.GetIssuesForResource(this.Resource.ResourceID, ValidationStatus.Error);
            return errors;
        }

        void OnSaved(object sender, EventArgs e)
        {
            UpdateTitle();
        }

        private string GetTooltip(string item)
        {
            return string.Format(Properties.Resources.EditorTitleTemplate, item, Environment.NewLine, this.Resource.CurrentConnection.DisplayName, this.Resource.ResourceVersion);
        }

        private void UpdateTitle()
        {
            this.Title = this.IsNew ? Properties.Resources.NewResource : ResourceIdentifier.GetName(_svc.ResourceID);
            this.Description = GetTooltip(this.IsNew ? Properties.Resources.NewResource : _svc.ResourceID);
        }

        const string DIRTY_PREFIX = "* ";

        void OnDirtyStateChanged(object sender, EventArgs e)
        {
            this.IsDirty = _svc.IsDirty; //Sync states
            if (_svc.IsDirty)
            {
                if (!this.Title.StartsWith(DIRTY_PREFIX))
                    this.Title = DIRTY_PREFIX + this.Title;
            }
            else
            {
                if (this.Title.StartsWith(DIRTY_PREFIX))
                    this.Title = this.Title.Substring(1);
            }
        }

        /// <summary>
        /// Binds the specified resource to this control. This effectively initializes
        /// all the fields in this control and sets up databinding on all fields. All
        /// subclasses *must* override this method. 
        /// 
        /// Also note that this method may be called more than once (e.g. Returning from
        /// and XML edit of this resource). Thus subclasses must take this scenario into
        /// account when implementing
        /// </summary>
        /// <param name="value"></param>
        protected virtual void Bind(IEditorService service) 
        {
            throw new NotImplementedException();
        }

        public virtual bool CanBePreviewed
        {
            get
            {
                var res = this.Resource;
                if (res != null)
                {
                    var type = res.CurrentConnection.ProviderName;
                    var previewer = ResourcePreviewerFactory.GetPreviewer(type);
                    if (previewer != null)
                    {
                        return previewer.IsPreviewable(res);
                    }
                }
                return false;
            }
        }

        public event EventHandler DirtyStateChanged;

        private bool _dirty;

        public bool IsDirty
        {
            get { return _dirty; }
            set
            {
                _dirty = value;
                var handler = this.DirtyStateChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        public bool IsNew
        {
            get { return _svc != null ? _svc.IsNew : true; } //Mono
        }

        public virtual bool CanProfile
        {
            get { return false; }
        }

        public virtual bool CanBeValidated
        {
            get { return true; }
        }

        public virtual bool CanEditAsXml
        {
            get { return true; }
        }

        public virtual void Preview()
        {
            var conn = this.Resource.CurrentConnection;
            var previewer = ResourcePreviewerFactory.GetPreviewer(conn.ProviderName);
            if (previewer != null)
                previewer.Preview(this.Resource, this.EditorService);
        }

        public virtual void SyncSessionCopy()
        {
            this.EditorService.SyncSessionCopy();
        }

        public bool DiscardChangesOnClose
        {
            get;
            private set;
        }

        public virtual void Close(bool discardChanges)
        {
            this.DiscardChangesOnClose = discardChanges;
            base.Close();
        }

        private void btnUpgrade_Click(object sender, EventArgs e)
        {
            var res = _svc.GetEditedResource();
            var conn = res.CurrentConnection;
            var ver = conn.Capabilities.GetMaxSupportedResourceVersion(res.ResourceType);

            using (new WaitCursor(this))
            {
                var conv = new ResourceObjectConverter();
                var res2 = conv.Convert(res, ver);
                
                using (var stream = ResourceTypeRegistry.Serialize(res2))
                {
                    using (var sr =new StreamReader(stream))
                    {
                        _svc.UpdateResourceContent(sr.ReadToEnd());
                        ((ResourceEditorService)_svc).ReReadSessionResource();
                    }
                }
                
                //This will re-init everything
                this.EditorService = this.EditorService;
                MessageBox.Show(string.Format(Properties.Resources.ResourceUpgraded, ver.Major, ver.Minor, ver.Build));
                this.EditorService.MarkDirty(); //It gets re-init with a clean slate, but an in-place upgrade is a dirty operation
            }
        }
    }
}
