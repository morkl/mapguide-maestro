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

using OSGeo.MapGuide.ObjectModels;
using System;
using System.Windows.Forms;

namespace Maestro.Base.UI
{
    /// <summary>
    /// Defines a cache of icons for resource types
    /// </summary>
    public interface IResourceIconCache : IDisposable
    {
        /// <summary>
        /// Gets the small resource icon list
        /// </summary>
        ImageList SmallImageList { get; }

        /// <summary>
        /// Gets the large resource icon list
        /// </summary>
        ImageList LargeImageList { get; }

        /// <summary>
        /// Gets the specified image list key for the given resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        string GetImageKeyFromResourceID(string resourceId);

        /// <summary>
        /// Gets the specified image list index for the given resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        int GetImageIndexFromResourceID(string resourceId);
    }

    /// <summary>
    /// A cache of icons for resource types
    /// </summary>
    public class ResourceIconCache : IResourceIconCache
    {
        private ImageList _small;
        private ImageList _large;

        private ResourceIconCache()
        {
            _small = new ImageList();
            _large = new ImageList();
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~ResourceIconCache()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose of this instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _small?.Dispose();
                _small = null;
                _large?.Dispose();
                _large = null;
            }
        }

        private const string UNKNOWN = nameof(UNKNOWN);

        /// <summary>
        /// Creates the default image lists
        /// </summary>
        /// <returns></returns>
        public static ResourceIconCache CreateDefault()
        {
            var icons = new ResourceIconCache();

            //TODO: Externalize
            icons._small.Images.Add(ResourceTypes.DrawingSource.ToString(), Properties.Resources.blueprints);
            icons._small.Images.Add(ResourceTypes.FeatureSource.ToString(), Properties.Resources.database_share);
            icons._small.Images.Add(ResourceTypes.Folder.ToString(), Properties.Resources.folder_horizontal);
            icons._small.Images.Add(ResourceTypes.LayerDefinition.ToString(), Properties.Resources.layer);
            icons._small.Images.Add(ResourceTypes.MapDefinition.ToString(), Properties.Resources.map);
            icons._small.Images.Add(ResourceTypes.WebLayout.ToString(), Properties.Resources.application_browser);
            icons._small.Images.Add(ResourceTypes.ApplicationDefinition.ToString(), Properties.Resources.applications_stack);
            icons._small.Images.Add(ResourceTypes.SymbolLibrary.ToString(), Properties.Resources.images_stack);
            icons._small.Images.Add(ResourceTypes.PrintLayout.ToString(), Properties.Resources.printer);
            icons._small.Images.Add(ResourceTypes.TileSetDefinition.ToString(), Properties.Resources.grid);
            icons._small.Images.Add(Properties.Resources.document);

            icons._large.Images.Add(ResourceTypes.DrawingSource.ToString(), Properties.Resources.blueprints);
            icons._large.Images.Add(ResourceTypes.FeatureSource.ToString(), Properties.Resources.database_share);
            icons._large.Images.Add(ResourceTypes.Folder.ToString(), Properties.Resources.folder_horizontal);
            icons._large.Images.Add(ResourceTypes.LayerDefinition.ToString(), Properties.Resources.layer);
            icons._large.Images.Add(ResourceTypes.MapDefinition.ToString(), Properties.Resources.map);
            icons._large.Images.Add(ResourceTypes.WebLayout.ToString(), Properties.Resources.application_browser);
            icons._large.Images.Add(ResourceTypes.ApplicationDefinition.ToString(), Properties.Resources.applications_stack);
            icons._large.Images.Add(ResourceTypes.SymbolLibrary.ToString(), Properties.Resources.images_stack);
            icons._large.Images.Add(ResourceTypes.PrintLayout.ToString(), Properties.Resources.printer);
            icons._large.Images.Add(ResourceTypes.TileSetDefinition.ToString(), Properties.Resources.grid);
            icons._large.Images.Add(Properties.Resources.document);

            return icons;
        }

        /// <summary>
        /// Gets the specified image list key for the given resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public string GetImageKeyFromResourceID(string resourceId)
        {
            var rt = ResourceIdentifier.GetResourceTypeAsString(resourceId);
            switch (rt)
            {
                case "DrawingSource":
                case "FeatureSource":
                case "Folder":
                case "LayerDefinition":
                case "MapDefinition":
                case "WebLayout":
                case "ApplicationDefinition":
                case "SymbolLibrary":
                case "PrintLayout":
                case "TileSetDefinition":
                    return rt.ToString();

                default:
                    return UNKNOWN;
            }
        }

        /// <summary>
        /// Gets the specified image list index for the given resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public int GetImageIndexFromResourceID(string resourceId)
        {
            int idx = _small.Images.IndexOfKey(ResourceIdentifier.GetResourceTypeAsString(resourceId));

            if (idx < 0)
                return _small.Images.IndexOfKey(UNKNOWN);

            return idx;
        }

        /// <summary>
        /// Gets the small resource icon list
        /// </summary>
        public ImageList SmallImageList
        {
            get { return _small; }
        }

        /// <summary>
        /// Gets the large resource icon list
        /// </summary>
        public ImageList LargeImageList
        {
            get { return _large; }
        }
    }
}