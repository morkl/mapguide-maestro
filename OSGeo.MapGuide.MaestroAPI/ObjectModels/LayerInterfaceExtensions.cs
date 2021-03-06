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

#endregion Disclaimer / License

using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.MaestroAPI.Schema;
using OSGeo.MapGuide.MaestroAPI.Services;
using OSGeo.MapGuide.ObjectModels.Common;
using OSGeo.MapGuide.ObjectModels.DrawingSource;
using OSGeo.MapGuide.ObjectModels.FeatureSource;
using OSGeo.MapGuide.ObjectModels.SymbolDefinition;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace OSGeo.MapGuide.ObjectModels.LayerDefinition
{
    /// <summary>
    /// Extension method clas
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Sets the color of the block.
        /// </summary>
        /// <param name="sym">The sym.</param>
        /// <param name="c">The c.</param>
        public static void SetBlockColor(this IBlockSymbol sym, Color c)
        {
            Check.NotNull(sym, "sym");
            sym.BlockColor = Utility.SerializeHTMLColor(c, true);
        }

        /// <summary>
        /// Sets the color of the layer.
        /// </summary>
        /// <param name="sym">The sym.</param>
        /// <param name="c">The c.</param>
        public static void SetLayerColor(this IBlockSymbol sym, Color c)
        {
            Check.NotNull(sym, "sym"); //NOXLATE
            sym.LayerColor = Utility.SerializeHTMLColor(c, true);
        }

        /// <summary>
        /// Applies properties (name, italic, bold, underline) from the characteristics of the specified font
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="f"></param>
        public static void Apply(this IFontSymbol sym, Font f)
        {
            Check.NotNull(sym, "sym"); //NOXLATE
            sym.FontName = f.Name;
            sym.Italic = f.Italic;
            sym.Bold = f.Bold;
            sym.Underlined = f.Underline;
        }

        /// <summary>
        /// Sets the color of the foreground.
        /// </summary>
        /// <param name="sym">The sym.</param>
        /// <param name="c">The c.</param>
        public static void SetForegroundColor(this IFontSymbol sym, Color c)
        {
            Check.NotNull(sym, "sym"); //NOXLATE
            sym.ForegroundColor = Utility.SerializeHTMLColor(c, true);
        }

        /// <summary>
        /// Determines whether the vector layer has scale ranges
        /// </summary>
        /// <param name="vl">The vl.</param>
        /// <returns>
        /// 	<c>true</c> if vector layer has scale ranges; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasVectorScaleRanges(this IVectorLayerDefinition vl)
        {
            Check.NotNull(vl, "vl"); //NOXLATE
            return vl.GetScaleRangeCount() > 0;
        }

        /// <summary>
        /// Gets the number of scale ranges in this vector layer
        /// </summary>
        /// <param name="vl"></param>
        /// <returns></returns>
        public static int GetScaleRangeCount(this IVectorLayerDefinition vl)
        {
            Check.NotNull(vl, "vl"); //NOXLATE
            var list = new List<IVectorScaleRange>(vl.VectorScaleRange);
            return list.Count;
        }

        /// <summary>
        /// Purge the specified scale range of the following styles
        /// </summary>
        /// <param name="range"></param>
        /// <param name="geomTypes">The geometry types to remove</param>
        public static void RemoveStyles(this IVectorScaleRange range, IEnumerable<string> geomTypes)
        {
            Check.NotNull(range, "range"); //NOXLATE
            Check.NotNull(geomTypes, "geomTypes"); //NOXLATE

            List<IVectorStyle> remove = new List<IVectorStyle>();

            foreach (var geomType in geomTypes)
            {
                if (geomType.ToLower().Equals(FeatureGeometricType.Curve.ToString().ToLower()))
                {
                    range.LineStyle = null;
                }
                else if (geomType.ToLower().Equals(FeatureGeometricType.Point.ToString().ToLower()))
                {
                    range.PointStyle = null;
                }
                else if (geomType.ToLower().Equals(FeatureGeometricType.Surface.ToString().ToLower()))
                {
                    range.AreaStyle = null;
                }
            }
        }

        /// <summary>
        /// Removes the styles.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="geomTypes">The geom types.</param>
        public static void RemoveStyles(this IVectorScaleRange range, params string[] geomTypes)
        {
            range.RemoveStyles(geomTypes);
        }

        /// <summary>
        /// Gets the coordinate system WKT.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns></returns>
        public static string GetCoordinateSystemWkt(this ILayerDefinition layer)
        {
            Check.NotNull(layer, "layer"); //NOXLATE
            if (layer.CurrentConnection == null)
                throw new System.Exception(OSGeo.MapGuide.MaestroAPI.Strings.ErrorNoServerConnectionAttached);

            var conn = layer.CurrentConnection;
            switch (layer.SubLayer.LayerType)
            {
                case LayerType.Raster:
                    {
                        var rl = (IRasterLayerDefinition)layer.SubLayer;
                        var fs = (IFeatureSource)conn.ResourceService.GetResource(rl.ResourceId);
                        var scList = fs.GetSpatialInfo(true);
                        if (scList.SpatialContext.Count > 0)
                            return scList.SpatialContext[0].CoordinateSystemWkt;
                    }
                    break;

                case LayerType.Vector:
                    {
                        var vl = (IVectorLayerDefinition)layer.SubLayer;
                        var fs = (IFeatureSource)conn.ResourceService.GetResource(vl.ResourceId);
                        var scList = fs.GetSpatialInfo(true);
                        if (scList.SpatialContext.Count > 0)
                            return scList.SpatialContext[0].CoordinateSystemWkt;
                    }
                    break;

                case LayerType.Drawing:
                    {
                        int[] services = conn.Capabilities.SupportedServices;
                        if (Array.IndexOf(services, (int)ServiceType.Drawing) >= 0)
                        {
                            var sheet = ((IDrawingLayerDefinition)layer.SubLayer).Sheet;
                            var dws = (IDrawingSource)conn.ResourceService.GetResource(((IDrawingLayerDefinition)layer.SubLayer).ResourceId);

                            //This should already be WKT form
                            return dws.CoordinateSpace;
                        }
                    }
                    break;
            }
            return string.Empty;
        }

        private static IFdoSpatialContext FindSpatialContext(FdoSpatialContextList spatialContexts, string scName)
        {
            foreach (IFdoSpatialContext sc in spatialContexts.SpatialContext)
            {
                if (sc.Name == scName)
                    return sc;
            }
            return null;
        }

        /// <summary>
        /// Returns the associated spatial context for this Layer Definition
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static IFdoSpatialContext GetSpatialContext(this ILayerDefinition layer)
        {
            Check.NotNull(layer, "layer"); //NOXLATE
            if (layer.CurrentConnection == null)
                throw new System.Exception(OSGeo.MapGuide.MaestroAPI.Strings.ErrorNoServerConnectionAttached);

            var conn = layer.CurrentConnection;
            var ltype = layer.SubLayer.LayerType;
            if (ltype == LayerType.Vector ||
                ltype == LayerType.Raster)
            {
                var sContexts = conn.FeatureService.GetSpatialContextInfo(layer.SubLayer.ResourceId, false);
                if (ltype == LayerType.Vector)
                {
                    IVectorLayerDefinition vl = (IVectorLayerDefinition)layer.SubLayer;
                    var clsDef = conn.FeatureService.GetClassDefinition(vl.ResourceId, vl.FeatureName);
                    var geom = clsDef.FindProperty(vl.Geometry) as GeometricPropertyDefinition;
                    if (geom != null)
                    {
                        var sc = FindSpatialContext(sContexts, geom.SpatialContextAssociation);
                        return sc;
                    }
                    return null;
                }
                else if (ltype == LayerType.Raster)
                {
                    IRasterLayerDefinition rl = (IRasterLayerDefinition)layer.SubLayer;
                    var clsDef = conn.FeatureService.GetClassDefinition(rl.ResourceId, rl.FeatureName);
                    var geom = clsDef.FindProperty(rl.Geometry) as RasterPropertyDefinition;
                    if (geom != null)
                    {
                        var sc = FindSpatialContext(sContexts, geom.SpatialContextAssociation);
                        return sc;
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the spatial extent of the data.
        /// This is calculated by asking the underlying featuresource for the minimum rectangle that
        /// contains all the features in the specified table. If the <paramref name="allowFallbackToContextInformation"/>
        /// is set to true, and the query fails, the code will attempt to read this information
        /// from the spatial context information instead.
        /// </summary>
        /// <param name="layer">The layer definition</param>
        /// <param name="allowFallbackToContextInformation">If true, will default to the extents of the active spatial context.</param>
        /// <param name="csWkt">The coordinate system WKT that this extent corresponds to</param>
        /// <returns></returns>
        public static IEnvelope GetSpatialExtent(this ILayerDefinition layer, bool allowFallbackToContextInformation, out string csWkt)
        {
            csWkt = null;
            Check.NotNull(layer, "layer"); //NOXLATE
            if (layer.CurrentConnection == null)
                throw new System.Exception(OSGeo.MapGuide.MaestroAPI.Strings.ErrorNoServerConnectionAttached);

            var conn = layer.CurrentConnection;
            switch (layer.SubLayer.LayerType)
            {
                case LayerType.Vector:
                    {
                        IEnvelope env = null;
                        IFdoSpatialContext activeSc = null;
                        try
                        {
                            activeSc = layer.GetSpatialContext();
                            if (activeSc != null)
                            {
                                //TODO: Check if ones like SQL Server will return the WKT, otherwise we'll need to bring in the
                                //CS catalog to do CS code to WKT conversion.
                                csWkt = activeSc.CoordinateSystemWkt;
                            }

                            //This can fail if SpatialExtents() aggregate function is not supported
                            env = conn.FeatureService.GetSpatialExtent(layer.SubLayer.ResourceId, ((IVectorLayerDefinition)layer.SubLayer).FeatureName, ((IVectorLayerDefinition)layer.SubLayer).Geometry);
                            return env;
                        }
                        catch
                        {
                            //Which in that case, default to extents of active spatial context
                            if (activeSc != null && activeSc.Extent != null)
                                return activeSc.Extent.Clone();
                            else
                                return null;
                        }
                    }
                case LayerType.Raster:
                    {
                        IEnvelope env = null;
                        IFdoSpatialContext activeSc = null;
                        try
                        {
                            var scList = conn.FeatureService.GetSpatialContextInfo(layer.SubLayer.ResourceId, true);
                            if (scList.SpatialContext.Count > 0)
                            {
                                activeSc = scList.SpatialContext[0];
                            }

                            //TODO: Would any raster provider *not* return a WKT?
                            csWkt = activeSc.CoordinateSystemWkt;

                            //Can fail if SpatialExtents() aggregate function is not supported
                            env = conn.FeatureService.GetSpatialExtent(layer.SubLayer.ResourceId, ((IRasterLayerDefinition)layer.SubLayer).FeatureName, ((IRasterLayerDefinition)layer.SubLayer).Geometry);
                            return env;
                        }
                        catch //Default to extents of active spatial context
                        {
                            if (activeSc != null && activeSc.Extent != null)
                                return activeSc.Extent.Clone();
                            else
                                return null;
                        }
                    }
                default:
                    {
                        int[] services = conn.Capabilities.SupportedServices;
                        if (Array.IndexOf(services, (int)ServiceType.Drawing) >= 0)
                        {
                            var sheet = ((IDrawingLayerDefinition)layer.SubLayer).Sheet;
                            var dws = (IDrawingSource)conn.ResourceService.GetResource(((IDrawingLayerDefinition)layer.SubLayer).ResourceId);

                            if (dws.Sheet != null)
                            {
                                //find matching sheet
                                foreach (var sht in dws.Sheet)
                                {
                                    if (sheet.Equals(sht.Name))
                                    {
                                        csWkt = dws.CoordinateSpace;
                                        return ObjectFactory.CreateEnvelope(sht.Extent.MinX, sht.Extent.MinY, sht.Extent.MaxX, sht.Extent.MaxY);
                                    }
                                }
                            }
                        }
                        return null;
                    }
            }
        }

        /// <summary>
        /// Gets the referenced schema of this vector layer
        /// </summary>
        /// <param name="vl"></param>
        /// <returns></returns>
        public static string GetSchema(this IVectorLayerDefinition vl)
        {
            if (string.IsNullOrEmpty(vl.FeatureName) || !vl.FeatureName.Contains(":")) //NOXLATE
                return string.Empty;
            else
                return vl.FeatureName.Split(':')[0]; //NOXLATE
        }

        /// <summary>
        /// Sets the color of the fill.
        /// </summary>
        /// <param name="sym">The sym.</param>
        /// <param name="c">The c.</param>
        public static void SetFillColor(this IW2DSymbol sym, Color c)
        {
            Check.NotNull(sym, "sym"); //NOXLATE
            sym.FillColor = Utility.SerializeHTMLColor(c, true);
        }

        /// <summary>
        /// Sets the color of the line.
        /// </summary>
        /// <param name="sym">The sym.</param>
        /// <param name="c">The c.</param>
        public static void SetLineColor(this IW2DSymbol sym, Color c)
        {
            Check.NotNull(sym, "sym"); //NOXLATE
            sym.LineColor = Utility.SerializeHTMLColor(c, true);
        }

        /// <summary>
        /// Sets the color of the text.
        /// </summary>
        /// <param name="sym">The sym.</param>
        /// <param name="c">The c.</param>
        public static void SetTextColor(this IW2DSymbol sym, Color c)
        {
            Check.NotNull(sym, "sym"); //NOXLATE
            sym.TextColor = Utility.SerializeHTMLColor(c, true);
        }

        /// <summary>
        /// Sets the color of the foreground.
        /// </summary>
        /// <param name="sym">The sym.</param>
        /// <param name="c">The c.</param>
        public static void SetForegroundColor(this ITextSymbol sym, Color c)
        {
            Check.NotNull(sym, "sym"); //NOXLATE
            sym.ForegroundColor = Utility.SerializeHTMLColor(c, true);
        }

        /// <summary>
        /// Sets the color of the background.
        /// </summary>
        /// <param name="sym">The sym.</param>
        /// <param name="c">The c.</param>
        public static void SetBackgroundColor(this ITextSymbol sym, Color c)
        {
            Check.NotNull(sym, "sym"); //NOXLATE
            sym.BackgroundColor = Utility.SerializeHTMLColor(c, true);
        }

        /// <summary>
        /// Applies properties (name, italic, bold, underline) from the characteristics of the specified font
        /// </summary>
        /// <param name="sym"></param>
        /// <param name="f"></param>
        public static void Apply(this ITextSymbol sym, Font f)
        {
            Check.NotNull(sym, "sym"); //NOXLATE
            sym.FontName = f.Name;
            sym.Italic = f.Italic.ToString();
            sym.Bold = f.Bold.ToString();
            sym.Underlined = f.Underline.ToString();
        }

        /// <summary>
        /// Sets the color of the background.
        /// </summary>
        /// <param name="fil">The fil.</param>
        /// <param name="c">The c.</param>
        public static void SetBackgroundColor(this IFill fil, Color c)
        {
            Check.NotNull(fil, "fil"); //NOXLATE
            fil.BackgroundColor = Utility.SerializeHTMLColor(c, true);
        }

        /// <summary>
        /// Sets the color of the foreground.
        /// </summary>
        /// <param name="fil">The fil.</param>
        /// <param name="c">The c.</param>
        public static void SetForegroundColor(this IFill fil, Color c)
        {
            Check.NotNull(fil, "fil"); //NOXLATE
            fil.ForegroundColor = Utility.SerializeHTMLColor(c, true);
        }

        /// <summary>
        /// Sets the color of the transparency.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="c">The c.</param>
        public static void SetTransparencyColor(this IGridColorStyle style, Color c)
        {
            Check.NotNull(style, "style"); //NOXLATE
            style.TransparencyColor = Utility.SerializeHTMLColor(c, true);
        }

        /// <summary>
        /// Sets the default color.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="c">The c.</param>
        public static void SetDefaultColor(this IGridSurfaceStyle style, Color c)
        {
            Check.NotNull(style, "style"); //NOXLATE
            style.DefaultColor = Utility.SerializeHTMLColor(c, true);
        }

        /// <summary>
        /// Removes all rules from a composite style
        /// </summary>
        /// <param name="style"></param>
        public static void RemoveAllRules(this ICompositeTypeStyle style)
        {
            Check.NotNull(style, "style"); //NOXLATE

            var remove = new List<ICompositeRule>();
            foreach (var r in style.CompositeRule)
                remove.Add(r);

            foreach (var r in remove)
            {
                style.RemoveCompositeRule(r);
            }
        }

        /// <summary>
        /// Defines a parameter for a simple symbol definition
        /// </summary>
        /// <param name="simpleSym"></param>
        /// <param name="identifier"></param>
        /// <param name="defaultValue"></param>
        /// <param name="displayName"></param>
        /// <param name="description"></param>
        /// <param name="dataType"></param>
        /// <returns>The defined parameter</returns>
        public static IParameter DefineParameter(this ISimpleSymbolDefinition simpleSym, string identifier, string defaultValue, string displayName, string description, string dataType)
        {
            Check.NotNull(simpleSym, "simpleSym");
            var p = simpleSym.CreateParameter();
            p.Identifier = identifier;
            p.DefaultValue = defaultValue;
            p.DisplayName = displayName;
            p.Description = description;
            p.DataType = dataType;
            simpleSym.ParameterDefinition.AddParameter(p);
            return p;
        }

        /// <summary>
        /// Adds a parameter override
        /// </summary>
        /// <param name="overrides"></param>
        /// <param name="symbolName"></param>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        /// <returns>The added parameter override</returns>
        public static IParameterOverride AddOverride(this IParameterOverrideCollection overrides, string symbolName, string paramName, string paramValue)
        {
            Check.NotNull(overrides, "overrides");
            var ov = overrides.CreateParameterOverride(symbolName, paramName);
            ov.ParameterValue = paramValue;
            overrides.AddOverride(ov);
            return ov;
        }

        /// <summary>
        /// Creates a default point composite rule
        /// </summary>
        /// <param name="fact"></param>
        /// <returns></returns>
        public static ICompositeRule CreateDefaultPointCompositeRule(this ILayerElementFactory fact)
        {
            Check.NotNull(fact, "fact");
            var rule = fact.CreateDefaultCompositeRule();
            //Clear out existing instances
            rule.CompositeSymbolization.RemoveAllSymbolInstances();

            var ldf = (ILayerDefinition)fact;
            var vl = (IVectorLayerDefinition)ldf.SubLayer;

            string symbolName = "Square"; //NOXLATE

            var ssym = ObjectFactory.CreateSimpleSymbol(ldf.CurrentConnection,
                                                        vl.SymbolDefinitionVersion,
                                                        symbolName,
                                                        "Default Point Symbol"); //NOXLATE

            var square = ssym.CreatePathGraphics();
            square.Geometry = "M -1.0,-1.0 L 1.0,-1.0 L 1.0,1.0 L -1.0,1.0 L -1.0,-1.0"; //NOXLATE
            square.FillColor = "%FILLCOLOR%"; //NOXLATE
            square.LineColor = "%LINECOLOR%"; //NOXLATE
            square.LineWeight = "%LINEWEIGHT%"; //NOXLATE
            ssym.AddGraphics(square);

            ssym.PointUsage = ssym.CreatePointUsage();
            ssym.PointUsage.Angle = "%ROTATION%"; //NOXLATE

            ssym.DefineParameter("FILLCOLOR", "0xffffffff", "&amp;Fill Color", "Fill Color", "FillColor"); //NOXLATE
            ssym.DefineParameter("LINECOLOR", "0xff000000", "Line &amp;Color", "Line Color", "LineColor"); //NOXLATE
            ssym.DefineParameter("LINEWEIGHT", "0.0", "Line &amp; Thickness", "Line Thickness", "LineWeight"); //NOXLATE
            ssym.DefineParameter("ROTATION", "0.0", "Line &amp; Thickness", "Line Thickness", "Angle"); //NOXLATE

            var instance = rule.CompositeSymbolization.CreateInlineSimpleSymbol(ssym);
            var overrides = instance.ParameterOverrides;

            overrides.AddOverride(symbolName, "FILLCOLOR", "0xffffffff"); //NOXLATE
            overrides.AddOverride(symbolName, "LINECOLOR", "0xff000000"); //NOXLATE
            overrides.AddOverride(symbolName, "LINEWEIGHT", "0.0"); //NOXLATE
            overrides.AddOverride(symbolName, "ROTATION", "0.0"); //NOXLATE

            instance.AddToExclusionRegion = "true"; //NOXLATE
            var inst2 = instance as ISymbolInstance2;
            if (inst2 != null)
            {
                inst2.UsageContext = UsageContextType.Point;
                inst2.GeometryContext = GeometryContextType.Point;
            }

            rule.CompositeSymbolization.AddSymbolInstance(instance);
            return rule;
        }

        /// <summary>
        /// Creates a default line composite rule
        /// </summary>
        /// <param name="fact"></param>
        /// <returns></returns>
        public static ICompositeRule CreateDefaultLineCompositeRule(this ILayerElementFactory fact)
        {
            Check.NotNull(fact, "fact");
            var rule = fact.CreateDefaultCompositeRule();
            //Clear out existing instances
            rule.CompositeSymbolization.RemoveAllSymbolInstances();

            var ldf = (ILayerDefinition)fact;
            var vl = (IVectorLayerDefinition)ldf.SubLayer;

            string symbolName = "Solid Line"; //NOXLATE

            var ssym = ObjectFactory.CreateSimpleSymbol(ldf.CurrentConnection,
                                                        vl.SymbolDefinitionVersion,
                                                        symbolName,
                                                        "Default Line Symbol"); //NOXLATE

            var line = ssym.CreatePathGraphics();
            line.Geometry = "M 0.0,0.0 L 1.0,0.0"; //NOXLATE
            line.LineColor = "%LINECOLOR%"; //NOXLATE
            line.LineWeight = "%LINEWEIGHT%"; //NOXLATE
            line.LineWeightScalable = "true"; //NOXLATE
            ssym.AddGraphics(line);

            ssym.LineUsage = ssym.CreateLineUsage();
            ssym.LineUsage.Repeat = "1.0";

            ssym.DefineParameter("LINECOLOR", "0xff000000", "Line &amp;Color", "Line Color", "LineColor"); //NOXLATE
            ssym.DefineParameter("LINEWEIGHT", "0.0", "Line &amp; Thickness", "Line Thickness", "LineWeight"); //NOXLATE

            var instance = rule.CompositeSymbolization.CreateInlineSimpleSymbol(ssym);
            var overrides = instance.ParameterOverrides;

            overrides.AddOverride(symbolName, "LINECOLOR", "0xff000000"); //NOXLATE
            overrides.AddOverride(symbolName, "LINEWEIGHT", "0.0"); //NOXLATE

            var inst2 = instance as ISymbolInstance2;
            if (inst2 != null)
            {
                inst2.UsageContext = UsageContextType.Line;
                inst2.GeometryContext = GeometryContextType.LineString;
            }

            rule.CompositeSymbolization.AddSymbolInstance(instance);
            return rule;
        }

        /// <summary>
        /// Creates a default area composite rule
        /// </summary>
        /// <param name="fact"></param>
        /// <returns></returns>
        public static ICompositeRule CreateDefaultAreaCompositeRule(this ILayerElementFactory fact)
        {
            Check.NotNull(fact, "fact");
            var rule = fact.CreateDefaultCompositeRule();
            //Clear out existing instances
            rule.CompositeSymbolization.RemoveAllSymbolInstances();

            var ldf = (ILayerDefinition)fact;
            var vl = (IVectorLayerDefinition)ldf.SubLayer;

            string fillSymbolName = "Solid Fill"; //NOXLATE
            var fillSym = ObjectFactory.CreateSimpleSymbol(ldf.CurrentConnection,
                                                           vl.SymbolDefinitionVersion,
                                                           fillSymbolName,
                                                           "Default Area Symbol"); //NOXLATE

            var fill = fillSym.CreatePathGraphics();
            fill.Geometry = "M 0.0,0.0 h 100.0 v 100.0 h -100.0 z";
            fill.FillColor = "%FILLCOLOR%";
            fillSym.AddGraphics(fill);

            fillSym.AreaUsage = fillSym.CreateAreaUsage();
            fillSym.AreaUsage.RepeatX = "100.0"; //NOXLATE
            fillSym.AreaUsage.RepeatY = "100.0"; //NOXLATE

            fillSym.DefineParameter("FILLCOLOR", "0xffbfbfbf", "&amp;Fill Color", "Fill Color", "FillColor"); //NOXLATE

            var fillInstance = rule.CompositeSymbolization.CreateInlineSimpleSymbol(fillSym);
            var fillOverrides = fillInstance.ParameterOverrides;

            var fillInst2 = fillInstance as ISymbolInstance2;
            if (fillInst2 != null)
            {
                fillInst2.GeometryContext = GeometryContextType.Polygon;
            }

            fillOverrides.AddOverride(fillSymbolName, "FILLCOLOR", "0xffbfbfbf");

            string lineSymbolName = "Solid Line"; //NOXLATE
            var lineSym = ObjectFactory.CreateSimpleSymbol(ldf.CurrentConnection,
                                                           vl.SymbolDefinitionVersion,
                                                           lineSymbolName,
                                                           "Default Line Symbol"); //NOXLATE

            var line = lineSym.CreatePathGraphics();
            line.Geometry = "M 0.0,0.0 L 1.0,0.0"; //NOXLATE
            line.LineColor = "%LINECOLOR%"; //NOXLATE
            line.LineWeight = "%LINEWEIGHT%"; //NOXLATE
            line.LineWeightScalable = "false"; //NOXLATE
            lineSym.AddGraphics(line);

            lineSym.LineUsage = lineSym.CreateLineUsage();
            lineSym.LineUsage.Repeat = "1.0";

            lineSym.DefineParameter("LINECOLOR", "0xff000000", "Line &amp;Color", "Line Color", "LineColor"); //NOXLATE
            lineSym.DefineParameter("LINEWEIGHT", "0.0", "Line &amp; Thickness", "Line Thickness", "LineWeight"); //NOXLATE

            var lineInstance = rule.CompositeSymbolization.CreateInlineSimpleSymbol(lineSym);
            var lineOverrides = lineInstance.ParameterOverrides;

            lineOverrides.AddOverride(lineSymbolName, "LINECOLOR", "0xff000000"); //NOXLATE
            lineOverrides.AddOverride(lineSymbolName, "LINEWEIGHT", "0.0"); //NOXLATE

            var lineInst2 = lineInstance as ISymbolInstance2;
            if (lineInst2 != null)
            {
                lineInst2.GeometryContext = GeometryContextType.Polygon;
            }

            rule.CompositeSymbolization.AddSymbolInstance(fillInstance);
            rule.CompositeSymbolization.AddSymbolInstance(lineInstance);
            return rule;
        }

        /// <summary>
        /// Creates a default point composite style
        /// </summary>
        /// <param name="fact"></param>
        /// <returns></returns>
        public static ICompositeTypeStyle CreateDefaultPointCompositeStyle(this ILayerElementFactory fact)
        {
            Check.NotNull(fact, "fact");
            var style = fact.CreateDefaultCompositeStyle();
            style.RemoveAllRules();
            style.AddCompositeRule(fact.CreateDefaultPointCompositeRule());
            return style;
        }

        /// <summary>
        /// Creates a default line composite style
        /// </summary>
        /// <param name="fact"></param>
        /// <returns></returns>
        public static ICompositeTypeStyle CreateDefaultLineCompositeStyle(this ILayerElementFactory fact)
        {
            Check.NotNull(fact, "fact");
            var style = fact.CreateDefaultCompositeStyle();
            style.RemoveAllRules();
            style.AddCompositeRule(fact.CreateDefaultLineCompositeRule());
            return style;
        }

        /// <summary>
        /// Creates a default area composite style
        /// </summary>
        /// <param name="fact"></param>
        /// <returns></returns>
        public static ICompositeTypeStyle CreateDefaultAreaCompositeStyle(this ILayerElementFactory fact)
        {
            Check.NotNull(fact, "fact");
            var style = fact.CreateDefaultCompositeStyle();
            style.RemoveAllRules();
            style.AddCompositeRule(fact.CreateDefaultAreaCompositeRule());
            return style;
        }
    }
}