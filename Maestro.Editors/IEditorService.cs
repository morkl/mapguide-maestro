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

using Maestro.Editors.Common;
using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.MaestroAPI.Schema;
using OSGeo.MapGuide.MaestroAPI.Services;
using OSGeo.MapGuide.ObjectModels;
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Maestro.Editors
{
    /// <summary>
    /// Defines an interface that provides common services for resource editors.
    /// </summary>
    public interface IEditorService
    {
        /// <summary>
        /// Gets the locale for previewing
        /// </summary>
        string PreviewLocale { get; set; }

        /// <summary>
        /// Gets the connection instance associated with this editor service
        /// </summary>
        IServerConnection CurrentConnection { get; }

        /// <summary>
        /// Gets the session id
        /// </summary>
        string SessionID { get; }

        /// <summary>
        /// Gets the suggested save folder for a "save as" operation
        /// </summary>
        string SuggestedSaveFolder { get; set; }

        /// <summary>
        /// Registers a custom notifier
        /// </summary>
        /// <param name="irc"></param>
        void RegisterCustomNotifier(INotifyResourceChanged irc);

        /// <summary>
        /// Indicates whether an upgrade for this resource is available
        /// </summary>
        bool IsUpgradeAvailable { get; }

        /// <summary>
        /// Invokes a prompt to select a resource of any type
        /// </summary>
        /// <returns></returns>
        string SelectAnyResource();

        /// <summary>
        /// Invokes a prompt to select a resource of the specified type
        /// </summary>
        /// <param name="resType"></param>
        /// <returns></returns>
        string SelectResource(string resType);

        /// <summary>
        /// Invokes a prompt to select a folder
        /// </summary>
        /// <returns></returns>
        string SelectFolder();

        /// <summary>
        /// Updates the session copy's resource content
        /// </summary>
        /// <param name="xml"></param>
        void UpdateResourceContent(string xml);

        /// <summary>
        /// Invokes a prompt to select a file from an unmanaged alias
        /// </summary>
        /// <param name="startPath"></param>
        /// <param name="fileTypes"></param>
        /// <returns></returns>
        string SelectUnmanagedData(string startPath, NameValueCollection fileTypes);

        /// <summary>
        /// Invokes the expression editor
        /// </summary>
        /// <param name="currentExpr"></param>
        /// <param name="schema"></param>
        /// <param name="providerName"></param>
        /// <param name="featureSourceId"></param>
        /// <param name="mode"></param>
        /// <param name="attachStylizationFunctions"></param>
        /// <returns></returns>
        string EditExpression(string currentExpr, ClassDefinition schema, string providerName, string featureSourceId, ExpressionEditorMode mode, bool attachStylizationFunctions);

        /// <summary>
        /// Gets the resource ID of the resource, whose session-copy is being edited
        /// </summary>
        string ResourceID { get; }

        /// <summary>
        /// Gets the resource ID of the actively edited resource
        /// </summary>
        string EditedResourceID { get; }

        /// <summary>
        /// Initiates the editing process. The resource to be edited is copied to the session repository and
        /// a deserialized version is returned from this copy. Subsequent calls will return the same reference
        /// to this resource object.
        /// </summary>
        /// <returns>A deserialized version of a session-copy of the resource to be edited</returns>
        IResource GetEditedResource();

        /// <summary>
        /// Raises the <see cref="E:Maestro.Editors.IEditorService.BeforePreview"/> event and performs any other pre-preview
        /// processing tasks
        /// </summary>
        void PrePreviewProcess();

        /// <summary>
        /// Raised before a preview occurs
        /// </summary>
        event EventHandler BeforePreview;

        /// <summary>
        /// Raised before a save operation commences
        /// </summary>
        event CancelEventHandler BeforeSave;

        /// <summary>
        /// Saves the edited resource. The session copy, which holds the current changes is copied back
        /// to the original resource ID.
        /// </summary>
        void Save();

        /// <summary>
        /// Saves the edited resource under a different resource ID. The session copy, which holds the current changes is copied back
        /// to the specified resource ID
        /// </summary>
        void SaveAs(string resourceID);

        /// <summary>
        /// Opens the specified URL
        /// </summary>
        /// <param name="url"></param>
        void OpenUrl(string url);

        /// <summary>
        /// Indicates whether the edited resource is a new resource
        /// </summary>
        bool IsNew { get; }

        /// <summary>
        /// Indicates whether the edited resource has unsaved changes
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Forces the edited resource to be marked as dirty
        /// </summary>
        void MarkDirty();

        /// <summary>
        /// Raised when the edited resource has changed
        /// </summary>
        event EventHandler DirtyStateChanged;

        /// <summary>
        /// Invokes a prompt to select the coordinate system
        /// </summary>
        /// <returns></returns>
        string GetCoordinateSystem();

        /// <summary>
        /// Forces the the <see cref="DirtyStateChanged"/> event. Normally the databinding
        /// system should auto-flag dirty state, only call this if you don't utilise this
        /// databinding system.
        /// </summary>
        void HasChanged();

        /// <summary>
        /// Raises a request to refresh the Site Explorer
        /// </summary>
        void RequestRefresh();

        /// <summary>
        /// Raises a request to refresh the Site Explorer at the specified folder id
        /// </summary>
        /// <param name="folderId"></param>
        void RequestRefresh(string folderId);

        /// <summary>
        /// Raised when the edited resource is saved
        /// </summary>
        event EventHandler Saved;

        /// <summary>
        /// Synchronises changes in the in-memory resource back to the session repository. This is usually called
        /// before validation of the edited resource begins. Call this method if you require changes in your in-memory
        /// resources to be flushed back to the session repository so that future API calls on this resource are working
        /// with up-to-date information
        /// </summary>
        void SyncSessionCopy();

        /// <summary>
        /// Gets the MapGuide Server version
        /// </summary>
        Version SiteVersion { get; }

        /// <summary>
        /// Opens the specified resource
        /// </summary>
        /// <param name="resourceId"></param>
        void OpenResource(string resourceId);

        /// <summary>
        /// Gets the supported services
        /// </summary>
        int[] SupportedServiceTypes { get; }

        /// <summary>
        /// Gets the service of the specified type
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        IService GetService(int serviceType);

        /// <summary>
        /// Gets the value of a custom connection property
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object GetCustomProperty(string name);

        /// <summary>
        /// Invokes the specified process name with the specified arguments
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="args"></param>
        void RunProcess(string processName, params string[] args);
    }
}