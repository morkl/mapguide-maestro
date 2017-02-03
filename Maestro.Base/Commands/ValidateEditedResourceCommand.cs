﻿#region Disclaimer / License

// Copyright (C) 2011, Jackie Ng
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

#endregion Disclaimer / License

using ICSharpCode.Core;
using Maestro.Base.Editor;
using Maestro.Base.Services;
using Maestro.Shared.UI;
using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.MaestroAPI.Exceptions;
using OSGeo.MapGuide.MaestroAPI.Resource;
using OSGeo.MapGuide.MaestroAPI.Resource.Validation;
using OSGeo.MapGuide.ObjectModels;
using OSGeo.MapGuide.ObjectModels.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Maestro.Base.Commands
{
    internal abstract class BaseValidateCommand : AbstractMenuCommand
    {
        protected IServerConnection _conn;

        protected object BackgroundValidate(BackgroundWorker worker, DoWorkEventArgs e, params object[] args)
        {
            //Collect all documents to be validated. Some of these selected items
            //may be folders.
            var documents = new System.Collections.Generic.List<string>();
            foreach (object a in args)
            {
                string rid = a.ToString();
                if (ResourceIdentifier.Validate(rid))
                {
                    var resId = new ResourceIdentifier(rid);
                    if (resId.IsFolder)
                    {
                        foreach (IRepositoryItem o in _conn.ResourceService.GetRepositoryResources((string)args[0]).Children)
                        {
                            if (!o.IsFolder)
                            {
                                documents.Add(o.ResourceId);
                            }
                        }
                    }
                    else
                    {
                        documents.Add(rid);
                    }
                }
            }

            worker.ReportProgress(0);
            var context = new ResourceValidationContext(_conn);

            var set = new ValidationResultSet();
            int i = 0;
            foreach (string s in documents)
            {
                worker.ReportProgress((int)((i / (double)documents.Count) * 100), s);
                IResource item = null;
                try
                {
                    item = _conn.ResourceService.GetResource(s);
                    set.AddIssues(ResourceValidatorSet.Validate(context, item, true));
                }
                catch (Exception ex)
                {
                    string msg = NestedExceptionMessageProcessor.GetFullMessage(ex);
                    set.AddIssue(new ValidationIssue(item, ValidationStatus.Error, ValidationStatusCode.Error_General_ValidationError, string.Format(Strings.ValidationResourceLoadFailed, msg)));
                }
                i++;
                worker.ReportProgress((int)((i / (double)documents.Count) * 100), s);
            }

            return set.GetAllIssues();
        }

        private static void OpenAffectedResource(IResource res)
        {
            var wb = Workbench.Instance;
            var siteExp = wb.ActiveSiteExplorer;
            var omgr = ServiceRegistry.GetService<OpenResourceManager>();
            var connMgr = ServiceRegistry.GetService<ServerConnectionManager>();
            var conn = connMgr.GetConnection(siteExp.ConnectionName);
            omgr.Open(res, conn, false, siteExp);
        }

        protected static void CollectAndDisplayIssues(ValidationIssue[] issues)
        {
            if (issues != null)
            {
                if (issues.Length > 0)
                {
                    //Sigh! LINQ would've made this code so simple...

                    var sort = new Dictionary<string, System.Collections.Generic.List<ValidationIssue>>();
                    foreach (var issue in issues)
                    {
                        string resId = issue.Resource.ResourceID;
                        if (!sort.ContainsKey(resId))
                            sort[resId] = new System.Collections.Generic.List<ValidationIssue>();

                        sort[resId].Add(issue);
                    }

                    var groupedIssues = new System.Collections.Generic.List<KeyValuePair<string, ValidationIssue[]>>();
                    foreach (var kvp in sort)
                    {
                        groupedIssues.Add(
                            new KeyValuePair<string, ValidationIssue[]>(
                                kvp.Key,
                                kvp.Value.ToArray()));
                    }

                    var resDlg = new ValidationResultsDialog(groupedIssues, OpenAffectedResource);
                    resDlg.Show();
                }
                else
                {
                    MessageService.ShowMessage(Strings.ValidationNoIssues);
                }
            }
        }
    }

    internal class ValidateEditedResourceCommand : BaseValidateCommand
    {
        public override void Run()
        {
            var wb = Workbench.Instance;
            var ed = wb.ActiveEditor;

            if (ed != null)
            {
                _conn = ed.EditorService.CurrentConnection;

                var xed = ed as XmlEditor;
                if (xed != null)
                {
                    xed.EditorService.UpdateResourceContent(xed.GetXmlContent());
                }
                else
                {
                    ed.EditorService.SyncSessionCopy();
                }

                var pdlg = new ProgressDialog();
                pdlg.CancelAbortsThread = true;

                var issues = (ValidationIssue[])pdlg.RunOperationAsync(wb, new ProgressDialog.DoBackgroundWork(BackgroundValidate), ed.EditorService.EditedResourceID);

                CollectAndDisplayIssues(issues);
            }
        }
    }
}