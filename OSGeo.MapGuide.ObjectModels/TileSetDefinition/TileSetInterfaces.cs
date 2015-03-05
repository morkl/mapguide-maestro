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
using OSGeo.MapGuide.ObjectModels.LayerDefinition;
using OSGeo.MapGuide.ObjectModels.MapDefinition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace OSGeo.MapGuide.ObjectModels.TileSetDefinition
{
    public interface ITileSetDefinition : IResource, ITileSetAbstract
    {
        ITileStoreParameters TileStoreParameters { get; }

        IEnvelope Extents { get; set; }
    }

    public interface ITileStoreParameters
    {
        string TileProvider { get; set; }

        void AddParameter(string name, string value);

        void SetParameter(string name, string value);

        IEnumerable<INameStringPair> Parameters { get; }

        void ClearParameters();

        event EventHandler ParametersChanged;
    }

    public interface ITileSetAbstract
    {
        void SetFiniteDisplayScales(IEnumerable<double> scales);

        /// <summary>
        /// Adds the finite display scale. The implementation may internally sort after adding the added item
        /// </summary>
        /// <param name="value">The value.</param>
        void AddFiniteDisplayScale(double value);

        /// <summary>
        /// Removes the finite display scale.
        /// </summary>
        /// <param name="value">The value.</param>
        void RemoveFiniteDisplayScale(double value);

        /// <summary>
        /// Gets the scale count.
        /// </summary>
        /// <value>The scale count.</value>
        int ScaleCount { get; }

        /// <summary>
        /// Removes the scale at the specified index
        /// </summary>
        /// <param name="index">The index.</param>
        void RemoveScaleAt(int index);

        /// <summary>
        /// Gets the scale at the specified index
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        double GetScaleAt(int index);

        /// <summary>
        /// Removes all scales.
        /// </summary>
        void RemoveAllScales();

        IEnumerable<double> FiniteDisplayScale { get; }

        /// <summary>
        /// Gets whether this tile set supports under certain conditions. If false, the caller
        /// should check if <see cref="SupportsCustomFiniteDisplayScales"/> is true in order to
        /// safely invoke any scale based operations
        /// </summary>
        bool SupportsCustomFiniteDisplayScalesUnconditionally { get; }

        /// <summary>
        /// Gets whether this tile set supports custom finite display scales. If false, none
        /// of the scale operations should be used
        /// </summary>
        bool SupportsCustomFiniteDisplayScales { get; }

        IEnumerable<IBaseMapGroup> BaseMapLayerGroups { get; }

        IEnumerable<IBaseMapLayer> GetLayersForGroup(string groupName);

        /// <summary>
        /// Gets the group count.
        /// </summary>
        /// <value>The group count.</value>
        int GroupCount { get; }

        /// <summary>
        /// Gets the group at the specified index
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        IBaseMapGroup GetGroupAt(int index);

        /// <summary>
        /// Adds the base layer group.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        IBaseMapGroup AddBaseLayerGroup(string name);

        /// <summary>
        /// Removes the base layer group.
        /// </summary>
        /// <param name="group">The group.</param>
        void RemoveBaseLayerGroup(IBaseMapGroup group);
    }

    public static class ExtensionMethods
    {
        /// <summary>
        /// Gets the number of base layers
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static int GetBaseLayerCount(this ITileSetAbstract map)
        {
            Check.ArgumentNotNull(map, "map"); //NOXLATE
            return map.BaseMapLayerGroups.SelectMany(g => g.BaseMapLayer).Count();
        }

        /// <summary>
        /// Gets the minimum finite display scale
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static double GetMinScale(this ITileSetAbstract map)
        {
            Check.ArgumentNotNull(map, "map"); //NOXLATE
            if (map.ScaleCount == 0)
                return 0.0;

            return map.FiniteDisplayScale.First();
        }

        /// <summary>
        /// Gets the maximum finite display scale
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static double GetMaxScale(this ITileSetAbstract map)
        {
            Check.ArgumentNotNull(map, "map"); //NOXLATE
            if (map.ScaleCount == 0)
                return 0.0;

            return map.FiniteDisplayScale.Last();
        }

        /// <summary>
        /// Gets the parent group for the specified layer
        /// </summary>
        /// <param name="map"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static IBaseMapGroup GetGroupForLayer(this ITileSetAbstract map, IBaseMapLayer layer)
        {
            Check.ArgumentNotNull(map, "map"); //NOXLATE
            foreach (var group in map.BaseMapLayerGroups)
            {
                foreach (var tl in group.BaseMapLayer)
                {
                    if (tl == layer)
                        return group;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets whether this base map group has tiled layers
        /// </summary>
        /// <param name="grp"></param>
        /// <returns></returns>
        public static bool HasLayers(this IBaseMapGroup grp)
        {
            Check.ArgumentNotNull(grp, "grp"); //NOXLATE
            return new List<IBaseMapLayer>(grp.BaseMapLayer).Count > 0;
        }

        /// <summary>
        /// Gets whether this base map has tiled layers
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static bool HasLayers(this ITileSetAbstract map)
        {
            Check.ArgumentNotNull(map, "map"); //NOXLATE
            if (!map.HasGroups())
                return false;

            foreach (var group in map.BaseMapLayerGroups)
            {
                if (group.HasLayers())
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets whether this base map has groups
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static bool HasGroups(this ITileSetAbstract map)
        {
            Check.ArgumentNotNull(map, "map"); //NOXLATE
            return new List<IBaseMapGroup>(map.BaseMapLayerGroups).Count > 0;
        }

        /// <summary>
        /// Gets the first base map group
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IBaseMapGroup GetFirstGroup(this ITileSetAbstract map)
        {
            Check.ArgumentNotNull(map, "map"); //NOXLATE
            var list = new List<IBaseMapGroup>(map.BaseMapLayerGroups);
            if (list.Count > 0)
                return list[0];
            return null;
        }

        /// <summary>
        /// Gets the base layer of the specified name
        /// </summary>
        /// <param name="map"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static IBaseMapLayer GetBaseLayerByName(this ITileSetAbstract map, string layerName)
        {
            Check.ArgumentNotNull(map, "map"); //NOXLATE
            Check.ArgumentNotEmpty(layerName, "layerName"); //NOXLATE

            foreach (var group in map.BaseMapLayerGroups)
            {
                foreach (var layer in group.BaseMapLayer)
                {
                    if (layerName.Equals(layer.Name))
                        return layer;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets whether a base layer of the specified name exists.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static bool LayerExists(this ITileSetAbstract map, string layerName)
        {
            Check.ArgumentNotNull(map, "map"); //NOXLATE
            Check.ArgumentNotEmpty(layerName, "layerName"); //NOXLATE

            return map.GetBaseLayerByName(layerName) != null;
        }

        /// <summary>
        /// Gets the base map group of the specified name
        /// </summary>
        /// <param name="map"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static IBaseMapGroup GetGroup(this ITileSetAbstract map, string groupName)
        {
            Check.ArgumentNotNull(map, "map"); //NOXLATE
            Check.ArgumentNotEmpty(groupName, "groupName"); //NOXLATE
            foreach (var group in map.BaseMapLayerGroups)
            {
                if (groupName.Equals(group.Name))
                    return group;
            }
            return null;
        }

        /// <summary>
        /// Gets whether the specified base map group exists
        /// </summary>
        /// <param name="map"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static bool GroupExists(this ITileSetAbstract map, string groupName)
        {
            Check.ArgumentNotNull(map, "map"); //NOXLATE
            Check.ArgumentNotEmpty(groupName, "groupName"); //NOXLATE
            foreach (var group in map.BaseMapLayerGroups)
            {
                if (groupName.Equals(group.Name))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the tiled layers for the specified base map group
        /// </summary>
        /// <param name="map"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static IEnumerable<IBaseMapLayer> GetLayersForGroup(this ITileSetAbstract map, string groupName)
        {
            Check.ArgumentNotNull(map, "map"); //NOXLATE
            Check.ArgumentNotEmpty(groupName, "groupName"); //NOXLATE

            foreach (var group in map.BaseMapLayerGroups)
            {
                if (groupName.Equals(group.Name))
                {
                    return group.BaseMapLayer;
                }
            }

            return new IBaseMapLayer[0];
        }

        /// <summary>
        /// Removes the given base layer group from the Map Definition
        /// </summary>
        /// <param name="map"></param>
        /// <param name="group"></param>
        /// <param name="bDetachIfEmpty"></param>
        public static void RemoveBaseLayerGroup(this ITileSetDefinition map, IBaseMapGroup group)
        {
            Check.ArgumentNotNull(map, "map"); //NOXLATE
            if (null == group)
                return;

            map.RemoveBaseLayerGroup(group);
        }

        /// <summary>
        /// Gets whether the specified base group exists in the tile set
        /// </summary>
        /// <param name="tileSet"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static bool GroupExists(this ITileSetDefinition tileSet, string groupName)
        {
            Check.ArgumentNotNull(tileSet, "map"); //NOXLATE
            Check.ArgumentNotEmpty(groupName, "groupName"); //NOXLATE
            foreach (var group in tileSet.BaseMapLayerGroups)
            {
                if (groupName.Equals(group.Name))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the tile set parameter of the specified name
        /// </summary>
        /// <param name="tileSet"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static INameStringPair GetParameter(this ITileSetDefinition tileSet, string name)
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE

            return tileSet.TileStoreParameters.Parameters.FirstOrDefault(p => p.Name == name);
        }

        /// <summary>
        /// Gets the coordinate system of this tile set. Must be using the default tile provider
        /// </summary>
        /// <param name="tileSet"></param>
        /// <returns></returns>
        public static string GetDefaultCoordinateSystem(this ITileSetDefinition tileSet)
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE

            var p = tileSet.GetParameter("CoordinateSystem"); //NOXLATE
            if (p != null)
                return p.Value;
            return null;
        }

        /// <summary>
        /// Sets the coordinate system of this tile set. Must be using the default tile provider.
        /// </summary>
        /// <param name="tileSet"></param>
        /// <param name="coordinateSystem"></param>
        public static void SetDefaultCoordinateSystem(this ITileSetDefinition tileSet, string coordinateSystem)
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE
            Check.ArgumentNotEmpty(coordinateSystem, "coordinateSystem"); //NOXLATE

            if (tileSet.TileStoreParameters.TileProvider != "Default") //NOXLATE
                throw new InvalidOperationException(string.Format(Strings.ParameterNotApplicableForTileProvider, "CoordinateSystem", tileSet.TileStoreParameters.TileProvider));

            tileSet.TileStoreParameters.SetParameter("CoordinateSystem", coordinateSystem); //NOXLATE
        }

        /// <summary>
        /// Gets the finite scale list of this tile set. Must be using the default tile provider.
        /// </summary>
        /// <param name="tileSet"></param>
        /// <returns></returns>
        public static double[] GetDefaultFiniteScaleList(this ITileSetDefinition tileSet)
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE
            var p = tileSet.GetParameter("FiniteScaleList"); //NOXLATE
            if (p != null && !string.IsNullOrEmpty(p.Value))
                return p.Value.Split(',').Select(x => x.Trim()).Select(x => Convert.ToDouble(x)).OrderBy(s => s).ToArray();
            return new double[0];
        }

        /// <summary>
        /// Sets the finite scale list of this tile set. Must be using the default tile provider.
        /// </summary>
        /// <param name="tileSet"></param>
        /// <param name="scales"></param>
        public static void SetDefaultFiniteScaleList(this ITileSetDefinition tileSet, IEnumerable<double> scales)
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE
            Check.ArgumentNotNull(scales, "scales"); //NOXLATE

            if (tileSet.TileStoreParameters.TileProvider != "Default") //NOXLATE
                throw new InvalidOperationException(string.Format(Strings.ParameterNotApplicableForTileProvider, "FiniteScaleList", tileSet.TileStoreParameters.TileProvider)); //NOXLATE

            string str = string.Join(",", scales.OrderByDescending(x => x).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
            tileSet.TileStoreParameters.SetParameter("FiniteScaleList", str); //NOXLATE
        }

        /// <summary>
        /// Gets the tile path of this tile set
        /// </summary>
        /// <param name="tileSet"></param>
        /// <returns></returns>
        public static string GetTilePath(this ITileSetDefinition tileSet)
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE
            var p = tileSet.GetParameter("TilePath"); //NOXLATE
            if (p != null)
                return p.Value;
            return null;
        }

        /// <summary>
        /// Sets the path of this tile set
        /// </summary>
        /// <param name="tileSet"></param>
        /// <param name="path"></param>
        public static void SetTilePath(this ITileSetDefinition tileSet, string path)
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE
            Check.ArgumentNotEmpty(path, "path"); //NOXLATE
            tileSet.TileStoreParameters.SetParameter("TilePath", path); //NOXLATE
        }

        /// <summary>
        /// Gets the width of this tile set. Must be using the default tile provider.
        /// </summary>
        /// <param name="tileSet"></param>
        /// <returns></returns>
        public static int? GetDefaultTileWidth(this ITileSetDefinition tileSet)
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE
            var p = tileSet.GetParameter("TileWidth"); //NOXLATE
            if (p != null)
                return Convert.ToInt32(p.Value);
            return null;
        }

        /// <summary>
        /// Sets the width of this tile set. Must be using the default tile provider.
        /// </summary>
        /// <param name="tileSet"></param>
        /// <param name="value"></param>
        public static void SetDefaultTileWidth(this ITileSetDefinition tileSet, int value)
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE
            if (tileSet.TileStoreParameters.TileProvider == "XYZ") //NOXLATE
                throw new InvalidOperationException(string.Format(Strings.ParameterNotApplicableForTileProvider, "TileWidth", "XYZ")); //NOXLATE
            tileSet.TileStoreParameters.SetParameter("TileWidth", value.ToString(CultureInfo.InvariantCulture)); //NOXLATE
        }

        /// <summary>
        /// Gets the width of this tile set. Must be using the default tile provider.
        /// </summary>
        /// <param name="tileSet"></param>
        /// <returns></returns>
        public static int? GetDefaultTileHeight(this ITileSetDefinition tileSet)
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE
            var p = tileSet.GetParameter("TileHeight"); //NOXLATE
            if (p != null)
                return Convert.ToInt32(p.Value);
            return null;
        }

        /// <summary>
        /// Sets the height of this tile set. Must be using the default tile provider.
        /// </summary>
        /// <param name="tileSet"></param>
        /// <param name="value"></param>
        public static void SetDefaultTileHeight(this ITileSetDefinition tileSet, int value)
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE
            if (tileSet.TileStoreParameters.TileProvider == "XYZ")
                throw new InvalidOperationException(string.Format(Strings.ParameterNotApplicableForTileProvider, "TileHeight", "XYZ")); //NOXLATE
            tileSet.TileStoreParameters.SetParameter("TileHeight", value.ToString(CultureInfo.InvariantCulture)); //NOXLATE
        }

        /// <summary>
        /// Gets the image format of this tile set
        /// </summary>
        /// <param name="tileSet"></param>
        /// <returns></returns>
        public static string GetTileFormat(this ITileSetDefinition tileSet)
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE
            var p = tileSet.GetParameter("TileFormat"); //NOXLATE
            if (p != null)
                return p.Value;
            return null;
        }

        /// <summary>
        /// Sets the image format of this tile set
        /// </summary>
        /// <param name="tileSet"></param>
        /// <param name="format"></param>
        public static void SetTileFormat(this ITileSetDefinition tileSet, string format)
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE
            Check.ArgumentNotEmpty(format, "format"); //NOXLATE
            tileSet.TileStoreParameters.SetParameter("TileFormat", format); //NOXLATE
        }

        /// <summary>
        /// Sets XYZ provider parameters. Any existing parameters are cleared
        /// </summary>
        /// <param name="tileSet"></param>
        /// <param name="tileFormat"></param>
        /// <param name="tilePath"></param>
        public static void SetXYZProviderParameters(this ITileSetDefinition tileSet,
                                                    string tileFormat = "PNG",
                                                    string tilePath = "%MG_TILE_CACHE_PATH%")
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE

            var param = tileSet.TileStoreParameters;
            param.TileProvider = "XYZ"; //NOXLATE
            param.ClearParameters();

            param.AddParameter("TileFormat", tileFormat); //NOXLATE
            param.AddParameter("TilePath", tilePath); //NOXLATE
        }

        /// <summary>
        /// Sets default provider parameters. Any existing parameters are cleared
        /// </summary>
        /// <param name="tileSet"></param>
        /// <param name="tileWidth"></param>
        /// <param name="tileHeight"></param>
        /// <param name="coordinateSystem"></param>
        /// <param name="finiteScaleList"></param>
        /// <param name="tileFormat"></param>
        /// <param name="tilePath"></param>
        public static void SetDefaultProviderParameters(this ITileSetDefinition tileSet, 
                                                        int tileWidth,
                                                        int tileHeight,
                                                        string coordinateSystem,
                                                        double [] finiteScaleList,
                                                        string tileFormat = "PNG", //NOXLATE
                                                        string tilePath = "%MG_TILE_CACHE_PATH%") //NOXLATE
        {
            Check.ArgumentNotNull(tileSet, "tileSet"); //NOXLATE

            var param = tileSet.TileStoreParameters;
            param.TileProvider = "Default"; //NOXLATE
            param.ClearParameters();

            param.AddParameter("TileWidth", tileWidth.ToString(CultureInfo.InvariantCulture)); //NOXLATE
            param.AddParameter("TileHeight", tileHeight.ToString(CultureInfo.InvariantCulture)); //NOXLATE
            param.AddParameter("TileFormat", tileFormat); //NOXLATE
            param.AddParameter("CoordinateSystem", coordinateSystem); //NOXLATE
            string str = string.Join(",", finiteScaleList.OrderByDescending(x => x).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray()); //NOXLATE
            param.AddParameter("FiniteScaleList", str); //NOXLATE
            param.AddParameter("TilePath", tilePath); //NOXLATE
        }
    }
}