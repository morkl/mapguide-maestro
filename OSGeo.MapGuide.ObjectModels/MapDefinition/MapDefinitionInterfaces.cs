﻿#region Disclaimer / License

// Copyright (C) 2014, Jackie Ng
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

using OSGeo.MapGuide.ObjectModels.Common;
using OSGeo.MapGuide.ObjectModels.TileSetDefinition;
using OSGeo.MapGuide.ObjectModels.WatermarkDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

#pragma warning disable 0108

namespace OSGeo.MapGuide.ObjectModels.MapDefinition
{
    /// <summary>
    /// Describes the extent of a given layer
    /// </summary>
    public class LayerExtent
    {
        /// <summary>
        /// The extent of the layer
        /// </summary>
        public IEnvelope Extent { get; set; }

        /// <summary>
        /// The layer's coordinate system
        /// </summary>
        public string LayerCoordinateSystem { get; set; }
    }

    /// <summary>
    /// Computes the extent for a given layer definition
    /// </summary>
    public interface ILayerExtentCalculator
    {
        /// <summary>
        /// Gets the extent of the given layer definition
        /// </summary>
        /// <param name="resourceID"></param>
        /// <param name="mapCoordSys"></param>
        /// <returns></returns>
        LayerExtent GetLayerExtent(string resourceID, string mapCoordSys);
    }

    /// <summary>
    /// Represents the base interface of map definitions and their runtime forms
    /// </summary>
    public interface IMapDefinitionBase
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the coordinate system. Layers whose coordinate system does
        /// not match will be re-projecte to this coordinate system when rendering
        /// </summary>
        /// <value>The coordinate system.</value>
        string CoordinateSystem { get; }

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>The color of the background.</value>
        ColorInfo BackgroundColor { get; set; }
    }

    /// <summary>
    /// Represents a Map Definition
    /// </summary>
    public interface IMapDefinition : IResource, IMapDefinitionBase, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the coordinate system. Layers whose coordinate system does
        /// not match will be re-projected to this coordinate system when rendering
        /// </summary>
        /// <value>The coordinate system.</value>
        string CoordinateSystem { get; set; }

        /// <summary>
        /// Gets or sets the extents.
        /// </summary>
        /// <value>The extents.</value>
        IEnvelope Extents { get; set; }

        /// <summary>
        /// Sets the extents.
        /// </summary>
        /// <param name="minx">The minx.</param>
        /// <param name="miny">The miny.</param>
        /// <param name="maxx">The maxx.</param>
        /// <param name="maxy">The maxy.</param>
        void SetExtents(double minx, double miny, double maxx, double maxy);

        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        /// <value>The metadata.</value>
        string Metadata { get; set; }

        /// <summary>
        /// Returns the base map section of this map definition. Ensure <see cref="InitBaseMap"/>
        /// is called first before accessing this property
        /// </summary>
        IBaseMapDefinition BaseMap { get; }

        /// <summary>
        /// Attaches the given base map section to this map definition. If an existing base map section
        /// exists, it is replaced
        /// </summary>
        /// <param name="baseMap"></param>
        void AttachBaseMap(IBaseMapDefinition baseMap);

        /// <summary>
        /// Initializes the base map section of this map definition. Subsequent calls
        /// do nothing, unless you have cleared the section via <see cref="RemoveBaseMap"/>
        /// </summary>
        void InitBaseMap();

        /// <summary>
        /// Clears the base map section of this map definition. If you want to rebuild
        /// this section, ensure <see cref="InitBaseMap"/> is called
        /// </summary>
        void RemoveBaseMap();

        /// <summary>
        /// Gets the map layers.
        /// </summary>
        /// <value>The map layers.</value>
        IEnumerable<IMapLayer> MapLayer { get; }

        /// <summary>
        /// Gets or sets a layer extent calculator
        /// </summary>
        ILayerExtentCalculator ExtentCalculator { get; set; }

        /// <summary>
        /// Inserts a layer into this map at the specified index in the map's layer collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="groupName"></param>
        /// <param name="layerName"></param>
        /// <param name="layerDefinitionId"></param>
        /// <returns></returns>
        IMapLayer InsertLayer(int index, string groupName, string layerName, string layerDefinitionId);

        /// <summary>
        /// Adds a layer to this map. If this is the first layer to be added, the coordinate system
        /// of this map and its extents will be set to the coordinate system and extents of this layer
        /// if this has not been set already.
        /// </summary>
        /// <remarks>
        /// The layer is added to the beginning of the list. That is, if you called <see cref="M:OSGeo.MapGuide.ObjectModels.MapDefinition.IMapDefinition.GetIndex(OSGeo.MapGuide.ObjectModels.MapDefinition.IMapLayer)"/>
        /// on your newly added layer, it will return 0. From a display perspective, your newly added layer will be at the top of the map's draw order when you create a runtime map from this map definition
        /// </remarks>
        /// <param name="groupName"></param>
        /// <param name="layerName"></param>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        IMapLayer AddLayer(string groupName, string layerName, string resourceId);

        /// <summary>
        /// Adds a layer to this map. If this is the first layer to be added, the coordinate system
        /// of this map and its extents will be set to the coordinate system and extents of this layer
        /// if this has not been set already.
        /// </summary>
        /// <param name="layerToInsertAbove">The layer to insert above in the draw order</param>
        /// <param name="groupName">The name of the group this layer belongs to. If null or empty, this layer will not belong to any group</param>
        /// <param name="layerName">The name of this layer. This must be unique</param>
        /// <param name="resourceId">The layer definition id</param>
        /// <returns>The added layer</returns>
        IMapLayer AddLayer(IMapLayer layerToInsertAbove, string groupName, string layerName, string resourceId);

        /// <summary>
        /// Removes the layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        void RemoveLayer(IMapLayer layer);

        /// <summary>
        /// Gets the index of the specified layer
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns></returns>
        int GetIndex(IMapLayer layer);

        /// <summary>
        /// Moves the layer up the draw order
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns>The new index of the moved layer. -1 is returned if the layer does not belong to the map</returns>
        int MoveUp(IMapLayer layer);

        /// <summary>
        /// Moves the layer down the draw order.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns>The new index of the moved layer. -1 is returned if the layer does not belong to the map</returns>
        int MoveDown(IMapLayer layer);

        /// <summary>
        /// Gets the map layer groups.
        /// </summary>
        /// <value>The map layer groups.</value>
        IEnumerable<IMapLayerGroup> MapLayerGroup { get; }

        /// <summary>
        /// Adds the group. The group will be added to the end of the list
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <returns></returns>
        IMapLayerGroup AddGroup(string groupName);

        /// <summary>
        /// Removes the group
        /// </summary>
        /// <param name="group"></param>
        void RemoveGroup(IMapLayerGroup group);

        /// <summary>
        /// Gets the index of the specified group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        int GetIndex(IMapLayerGroup group);

        /// <summary>
        /// Moves the specified layer to the top of the draw order
        /// </summary>
        /// <param name="layer"></param>
        void SetTopDrawOrder(IMapLayer layer);

        /// <summary>
        /// Moves the specified layer to the bottom of the draw order
        /// </summary>
        /// <param name="layer"></param>
        void SetBottomDrawOrder(IMapLayer layer);

        /// <summary>
        /// Inserts the layer at the specified index
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="layer"></param>
        void InsertLayer(int idx, IMapLayer layer);

        /// <summary>
        /// Moves a Map Group down the presentation order
        /// </summary>
        /// <param name="group"></param>
        /// <returns>The new index of the moved group. -1 is returned if the group does not belong to the map</returns>
        int MoveDownGroup(IMapLayerGroup group);

        /// <summary>
        /// Moves a Map Group up the presentation order
        /// </summary>
        /// <param name="group"></param>
        /// <returns>The new index of the moved group. -1 is returned if the group does not belong to the map</returns>
        int MoveUpGroup(IMapLayerGroup group);

        /// <summary>
        /// Removes all dynamic groups from this Map Definition
        /// </summary>
        void RemoveAllGroups();

        /// <summary>
        /// Removes all dynamic layers from this Map Definition
        /// </summary>
        void RemoveAllLayers();
    }

    /// <summary>
    /// Represents a Map Definition with support for watermarks. Corresponds to schema version 2.3.0
    /// </summary>
    public interface IMapDefinition2 : IMapDefinition, IWatermarkCollection
    {
    }

    /// <summary>
    /// Represents a Map Definition with support for linking to tile sets. Corresponds to schema version 3.0.0
    /// </summary>
    public interface IMapDefinition3 : IMapDefinition2
    {
        /// <summary>
        /// Gets or sets the Tile Set Definition. When setting a tile set, any existing base map settings are removed
        /// </summary>
        string TileSetDefinitionID { get; set; }

        /// <summary>
        /// Gets or sets the type of tile source
        /// </summary>
        TileSourceType TileSourceType { get; set; }
    }

    /// <summary>
    /// Defines the type of tile source
    /// </summary>
    public enum TileSourceType
    {
        /// <summary>
        /// The Map Definition links to an external tile set
        /// </summary>
        External,
        /// <summary>
        /// The tile layer/group settings are defined within the Map Definition
        /// </summary>
        Inline,
        /// <summary>
        /// No tile layer/group settings are defined
        /// </summary>
        None
    }

    /// <summary>
    /// Extension method class
    /// </summary>
    public static class MapDefinitionExtensions
    {
        /// <summary>
        /// Sets the extents of the map definition from the id of the the given layer definition
        /// Does nothing if the extent is already set
        /// </summary>
        /// <param name="mdf"></param>
        /// <param name="layerDefinitionId"></param>
        public static void AutoSetExtentsFromLayer(this IMapDefinition mdf, string layerDefinitionId)
        {
            //Do nothing if this is false
            if (!mdf.Extents.IsEmpty())
                return;

            var calc = mdf.ExtentCalculator;
            if (calc != null)
            {
                var res = calc.GetLayerExtent(layerDefinitionId, mdf.CoordinateSystem);
                if (res != null)
                {
                    //Set the coordinate system if empty
                    if (string.IsNullOrEmpty(mdf.CoordinateSystem))
                    {
                        mdf.CoordinateSystem = res.LayerCoordinateSystem;
                    }
                    //Set the bounds if empty
                    if (mdf.Extents.IsEmpty())
                    {
                        var env = res.Extent;
                        mdf.SetExtents(env.MinX, env.MinY, env.MaxX, env.MaxY);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the specified finite display scale to the Map Definition
        /// </summary>
        /// <param name="map"></param>
        /// <param name="scale"></param>
        public static void AddFiniteDisplayScale(this IMapDefinition map, double scale)
        {
            Check.ArgumentNotNull(map, nameof(map));

            if (map.BaseMap != null)
                map.InitBaseMap();

            map.BaseMap.AddFiniteDisplayScale(scale);
        }

        /// <summary>
        /// Removes the specified finite display scale from the Map Definition
        /// </summary>
        /// <param name="map"></param>
        /// <param name="scale"></param>
        /// <param name="bDetachIfEmpty"></param>
        public static void RemoveFiniteDisplayScale(this IMapDefinition map, double scale, bool bDetachIfEmpty)
        {
            Check.ArgumentNotNull(map, nameof(map));

            if (map.BaseMap == null)
                return;

            map.BaseMap.RemoveFiniteDisplayScale(scale);
            if (map.BaseMap.GroupCount == 0 && map.BaseMap.ScaleCount == 0 && bDetachIfEmpty)
                map.RemoveBaseMap();
        }

        /// <summary>
        /// Removes all finite display scales from the Map Definition
        /// </summary>
        /// <param name="map"></param>
        /// <param name="bDetachIfEmpty"></param>
        public static void RemoveAllFiniteDisplayScales(this IMapDefinition map, bool bDetachIfEmpty)
        {
            Check.ArgumentNotNull(map, nameof(map));

            if (map.BaseMap == null)
                return;

            map.BaseMap.RemoveAllScales();
            if (map.BaseMap.GroupCount == 0 && map.BaseMap.ScaleCount == 0 && bDetachIfEmpty)
                map.RemoveBaseMap();
        }

        /// <summary>
        /// Adds the specified base layer group to the map definition
        /// </summary>
        /// <param name="map"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IBaseMapGroup AddBaseLayerGroup(this IMapDefinition map, string name)
        {
            Check.ArgumentNotNull(map, nameof(map));
            Check.ArgumentNotEmpty(name, nameof(name));

            if (map.BaseMap == null)
                map.InitBaseMap();

            return map.BaseMap.AddBaseLayerGroup(name);
        }

        /// <summary>
        /// Removes the given base layer group from the Map Definition
        /// </summary>
        /// <param name="map"></param>
        /// <param name="group"></param>
        /// <param name="bDetachIfEmpty"></param>
        public static void RemoveBaseLayerGroup(this IMapDefinition map, IBaseMapGroup group, bool bDetachIfEmpty)
        {
            Check.ArgumentNotNull(map, nameof(map));
            if (null == group)
                return;

            if (map.BaseMap == null)
                return;

            map.BaseMap.RemoveBaseLayerGroup(group);
            if (map.BaseMap.GroupCount == 0 && map.BaseMap.GroupCount == 0 && bDetachIfEmpty)
                map.RemoveBaseMap();
        }

        /// <summary>
        /// Updates the group name references of all layers belonging to a particular group
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="oldGroupName">Old name of the group.</param>
        /// <param name="newGroupName">New name of the group.</param>
        public static void UpdateDynamicGroupName(this IMapDefinition map, string oldGroupName, string newGroupName)
        {
            Check.ArgumentNotNull(map, nameof(map));
            Check.ArgumentNotEmpty(oldGroupName, nameof(oldGroupName));
            Check.ArgumentNotEmpty(newGroupName, nameof(newGroupName));
            var layers = map.GetLayersForGroup(oldGroupName);
            var groups = map.GetGroupsForGroup(oldGroupName);
            foreach (var l in layers)
            {
                l.Group = newGroupName;
            }
            foreach (var g in groups)
            {
                g.Group = newGroupName;
            }
        }

        /// <summary>
        /// Removes a layer group and all layers associated with this group
        /// </summary>
        /// <param name="map"></param>
        /// <param name="groupName"></param>
        /// <returns>The number of layers removed. Returns 0 if the group is empty or does not exist</returns>
        public static int RemoveLayerGroupAndChildLayers(this IMapDefinition map, string groupName)
        {
            Check.ArgumentNotNull(map, nameof(map));
            Check.ArgumentNotEmpty(groupName, nameof(groupName));

            var affectedParentGroups = new Dictionary<string, List<IMapLayerGroup>>();
            IMapLayerGroup group = null;
            foreach (var grp in map.MapLayerGroup)
            {
                if (grp.Name == groupName)
                    group = grp;

                string parentGroupName = grp.Group;
                if (!string.IsNullOrEmpty(parentGroupName))
                {
                    if (!affectedParentGroups.ContainsKey(parentGroupName))
                        affectedParentGroups[parentGroupName] = new List<IMapLayerGroup>();
                    affectedParentGroups[parentGroupName].Add(grp);
                }
            }

            if (group != null)
            {
                List<IMapLayer> layers = new List<IMapLayer>(map.GetLayersForGroup(groupName));

                int removed = 0;
                //Remove layers first
                foreach (var l in layers)
                {
                    map.RemoveLayer(l);
                    removed++;
                }
                //Then the group
                map.RemoveGroup(group);

                //Then see if any child groups are under this group and remove them too
                if (affectedParentGroups.ContainsKey(group.Name))
                {
                    for (int i = 0; i < affectedParentGroups[group.Name].Count; i++)
                    {
                        var removeMe = affectedParentGroups[group.Name][i];
                        removed += map.RemoveLayerGroupAndChildLayers(removeMe.Name);
                    }
                }

                return removed;
            }
            return 0;
        }

        /// <summary>
        /// Get a layer by its name
        /// </summary>
        /// <param name="map"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IMapLayer GetLayerByName(this IMapDefinition map, string name)
        {
            Check.ArgumentNotNull(map, nameof(map));
            Check.ArgumentNotEmpty(name, nameof(name));
            foreach (var layer in map.MapLayer)
            {
                if (name.Equals(layer.Name))
                    return layer;
            }
            return null;
        }

        /// <summary>
        /// Gets a group by its name
        /// </summary>
        /// <param name="map"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IMapLayerGroup GetGroupByName(this IMapDefinition map, string name)
        {
            Check.ArgumentNotNull(map, nameof(map));
            foreach (var group in map.MapLayerGroup)
            {
                if (name.Equals(group.Name))
                    return group;
            }
            return null;
        }

        /// <summary>
        /// Gets the number of layers (non-tiled) on this map
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static int GetDynamicLayerCount(this IMapDefinition map)
        {
            Check.ArgumentNotNull(map, nameof(map));
            return new List<IMapLayer>(map.MapLayer).Count;
        }

        /// <summary>
        /// Gets the number of groups (non-tiled) on this map
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static int GetGroupCount(this IMapDefinition map)
        {
            Check.ArgumentNotNull(map, nameof(map));
            return new List<IMapLayerGroup>(map.MapLayerGroup).Count;
        }

        /// <summary>
        /// Gets all the layers that belong to the specified group
        /// </summary>
        /// <param name="map"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<IMapLayer> GetLayersForGroup(this IMapDefinition map, string name)
        {
            Check.ArgumentNotNull(map, nameof(map));
            foreach (var layer in map.MapLayer)
            {
                if (name.Equals(layer.Group))
                    yield return layer;
            }
        }

        /// <summary>
        /// Gets all the groups that belong to the specified group
        /// </summary>
        /// <param name="map"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<IMapLayerGroup> GetGroupsForGroup(this IMapDefinition map, string name)
        {
            Check.ArgumentNotNull(map, nameof(map));
            foreach (var group in map.MapLayerGroup)
            {
                if (name.Equals(group.Group))
                    yield return group;
            }
        }

        /// <summary>
        /// Gets all that layers that do not belong to a group
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IEnumerable<IMapLayer> GetLayersWithoutGroups(this IMapDefinition map)
        {
            Check.ArgumentNotNull(map, nameof(map));
            foreach (var layer in map.MapLayer)
            {
                if (string.IsNullOrEmpty(layer.Group))
                    yield return layer;
            }
        }

        /// <summary>
        /// Convers the given base map section to a Tile Set Definition reference
        /// </summary>
        /// <param name="map"></param>
        /// <param name="schemaVersion"></param>
        /// <returns></returns>
        public static ITileSetDefinition ConvertToTileSet(this IMapDefinition map, Version schemaVersion)
        {
            Check.ArgumentNotNull(map, nameof(map));

            var tsd = ObjectFactory.CreateTileSetDefinition(schemaVersion, map.Extents.Clone());

            var baseMap = map.BaseMap;
            if (baseMap == null)
                throw new InvalidOperationException(Strings.MapDefinitionHasNoBaseMapSection);

            //Clear any existing default groups created
            foreach(var grp in tsd.BaseMapLayerGroups.ToArray())
            {
                tsd.RemoveBaseLayerGroup(grp);
            }
            tsd.SetDefaultProviderParameters(300, 300, map.CoordinateSystem, baseMap.FiniteDisplayScale.ToArray());

            foreach (var grp in baseMap.BaseMapLayerGroups)
            {
                var tsdGroup = tsd.AddBaseLayerGroup(grp.Name);
                tsdGroup.ExpandInLegend = grp.ExpandInLegend;
                tsdGroup.LegendLabel = grp.LegendLabel;
                tsdGroup.ShowInLegend = grp.ShowInLegend;
                tsdGroup.Visible = grp.Visible;

                foreach (var layer in grp.BaseMapLayer)
                {
                    var tsdLayer = tsdGroup.AddLayer(layer.Name, layer.ResourceId);
                    tsdLayer.ExpandInLegend = layer.ExpandInLegend;
                    tsdLayer.LegendLabel = layer.LegendLabel;
                    tsdLayer.Selectable = layer.Selectable;
                    tsdLayer.ShowInLegend = layer.ShowInLegend;
                }
            }

            return tsd;
        }
    }

    /// <summary>
    /// Represents the tiled map portion of the Map Definition
    /// </summary>
    public interface IBaseMapDefinition : ITileSetAbstract
    {
        
    }

    /// <summary>
    /// Base legend element
    /// </summary>
    public interface IMapLegendElementBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in legend].
        /// </summary>
        /// <value><c>true</c> if [show in legend]; otherwise, <c>false</c>.</value>
        bool ShowInLegend { get; set; }

        /// <summary>
        /// Gets or sets the legend label.
        /// </summary>
        /// <value>The legend label.</value>
        string LegendLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [expand in legend].
        /// </summary>
        /// <value><c>true</c> if [expand in legend]; otherwise, <c>false</c>.</value>
        bool ExpandInLegend { get; set; }
    }

    /// <summary>
    /// Base layer interface
    /// </summary>
    public interface IBaseMapLayer : IMapLegendElementBase
    {
        /// <summary>
        /// Gets or sets the resource id.
        /// </summary>
        /// <value>The resource id.</value>
        string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IBaseMapLayer"/> is selectable.
        /// </summary>
        /// <value><c>true</c> if selectable; otherwise, <c>false</c>.</value>
        bool Selectable { get; set; }
    }

    /// <summary>
    /// Tiled map group
    /// </summary>
    public interface IBaseMapGroup : IMapLegendElementBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IBaseMapGroup"/> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        bool Visible { get; set; }

        /// <summary>
        /// Gets the base map layers.
        /// </summary>
        /// <value>The base map layers.</value>
        IEnumerable<IBaseMapLayer> BaseMapLayer { get; }

        /// <summary>
        /// Adds the layer.
        /// </summary>
        /// <param name="layerName">Name of the layer.</param>
        /// <param name="resourceId">The resource id.</param>
        /// <returns></returns>
        IBaseMapLayer AddLayer(string layerName, string resourceId);

        /// <summary>
        /// Removes the base map layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        void RemoveBaseMapLayer(IBaseMapLayer layer);

        /// <summary>
        /// Insert the base map layer at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="layer"></param>
        void InsertLayer(int index, IBaseMapLayer layer);

        /// <summary>
        /// Gets the index of the specified layer
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        int GetIndex(IBaseMapLayer layer);

        /// <summary>
        /// Moves the specified layer up.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns></returns>
        int MoveUp(IBaseMapLayer layer);

        /// <summary>
        /// Moves the specified layer down.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns></returns>
        int MoveDown(IBaseMapLayer layer);
    }

    /// <summary>
    /// A dynamic map layer
    /// </summary>
    public interface IMapLayer : IBaseMapLayer
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IMapLayer"/> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>The group.</value>
        string Group { get; set; }
    }

    /// <summary>
    /// A dynamic map layer group
    /// </summary>
    public interface IMapLayerGroup : IMapLegendElementBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IMapLayerGroup"/> is visible.
        /// </summary>
        /// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
        bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the group name. If null, it means this layer doesn't belong to any group
        /// </summary>
        /// <value>The group.</value>
        string Group { get; set; }

        /// <summary>
        /// Gets the parent map definition
        /// </summary>
        /// <value>The parent map definition.</value>
        IMapDefinition Parent { get; }
    }
}