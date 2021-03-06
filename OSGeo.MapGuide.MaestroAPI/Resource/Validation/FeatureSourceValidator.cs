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

using OSGeo.MapGuide.MaestroAPI.Exceptions;
using OSGeo.MapGuide.MaestroAPI.Schema;
using OSGeo.MapGuide.MaestroAPI.SchemaOverrides;
using OSGeo.MapGuide.MaestroAPI.Services;
using OSGeo.MapGuide.ObjectModels;
using OSGeo.MapGuide.ObjectModels.Common;
using OSGeo.MapGuide.ObjectModels.FeatureSource;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSGeo.MapGuide.MaestroAPI.Resource.Validation
{
    /// <summary>
    /// Resource validator for Feature Sources
    /// </summary>
    public class FeatureSourceValidator : IResourceValidator
    {
        private readonly string _version;

        internal FeatureSourceValidator(string version = "1.0.0")
        {
            _version = version;
        }

        /// <summary>
        /// The server connection which validation will be performed against
        /// </summary>
        public IServerConnection Connection { get; set; }

        /// <summary>
        /// Validats the specified resources for common issues associated with this
        /// resource type
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resource"></param>
        /// <param name="recurse"></param>
        /// <returns></returns>
        public ValidationIssue[] Validate(ResourceValidationContext context, IResource resource, bool recurse)
        {
            Check.ArgumentNotNull(context, nameof(context));

            if (context.IsAlreadyValidated(resource.ResourceID))
                return null;

            if (resource.ResourceType != ResourceTypes.FeatureSource.ToString())
                return null;

            List<ValidationIssue> issues = new List<ValidationIssue>();

            IFeatureSource feature = (IFeatureSource)resource;
            IFeatureService featSvc = this.Connection.FeatureService;

            //Feature Join Optimization check
            foreach (var ext in feature.Extension)
            {
                foreach (var rel in ext.AttributeRelate)
                {
                    if (string.IsNullOrEmpty(rel.Name))
                    {
                        issues.Add(new ValidationIssue(resource, ValidationStatus.Warning, ValidationStatusCode.Warning_FeatureSource_EmptyJoinPrefix, string.Format(Strings.FS_EmptyJoinPrefix, ext.Name)));
                    }

                    if (rel.RelatePropertyCount > 0)
                    {
                        if (rel.RelatePropertyCount == 1)
                        {
                            var srcFs = feature;
                            var dstFs = (IFeatureSource)context.GetResource(rel.ResourceId);

                            var leftProvider = srcFs.Provider.ToUpper();
                            var rightProvider = dstFs.Provider.ToUpper();

                            //FDO Join optimization check
                            if (leftProvider.Contains("OSGEO.SQLITE") && rightProvider.Contains("OSGEO.SQLITE") && srcFs.ResourceID == rel.ResourceId) //NOXLATE
                                continue;

                            //FDO Join optimization check
                            if (leftProvider.Contains("OSGEO.SQLSERVERSPATIAL") && rightProvider.Contains("OSGEO.SQLSERVERSPATIAL") && srcFs.ResourceID == rel.ResourceId) //NOXLATE
                                continue;

                            //TODO: Fix the capabilities response. Because it's not telling us enough information!
                            //Anyways, these are the providers known to provide sorted query results.
                            bool bLeftSortable = leftProvider.Contains("OSGEO.SDF") || //NOXLATE
                                                 leftProvider.Contains("OSGEO.SHP") || //NOXLATE
                                                 leftProvider.Contains("OSGEO.SQLITE") || //NOXLATE
                                                 leftProvider.Contains("OSGEO.ODBC") || //NOXLATE
                                                 leftProvider.Contains("OSGEO.SQLSERVERSPATIAL") || //NOXLATE
                                                 leftProvider.Contains("OSGEO.MYSQL") || //NOXLATE
                                                 leftProvider.Contains("OSGEO.POSTGRESQL"); //NOXLATE

                            bool bRightSortable = leftProvider.Contains("OSGEO.SDF") || //NOXLATE
                                                 leftProvider.Contains("OSGEO.SHP") || //NOXLATE
                                                 leftProvider.Contains("OSGEO.SQLITE") || //NOXLATE
                                                 leftProvider.Contains("OSGEO.ODBC") || //NOXLATE
                                                 leftProvider.Contains("OSGEO.SQLSERVERSPATIAL") || //NOXLATE
                                                 leftProvider.Contains("OSGEO.MYSQL") || //NOXLATE
                                                 leftProvider.Contains("OSGEO.POSTGRESQL"); //NOXLATE

                            if (!bLeftSortable || !bRightSortable)
                            {
                                issues.Add(new ValidationIssue(resource, ValidationStatus.Warning, ValidationStatusCode.Warning_FeatureSource_Potential_Bad_Join_Performance, string.Format(Strings.FS_PotentialBadJoinPerformance, ext.Name, bLeftSortable, bRightSortable)));
                            }
                        }
                        else
                        {
                            issues.Add(new ValidationIssue(resource, ValidationStatus.Warning, ValidationStatusCode.Warning_FeatureSource_Potential_Bad_Join_Performance, string.Format(Strings.FS_PotentialBadJoinPerformance2, ext.Name)));
                        }
                    }
                }
            }

            //Plaintext credential check
            string providerNameUpper = feature.Provider.ToUpper();
            string fsXml = feature.Serialize().ToUpper();

            //You'll get warnings either way
            if (providerNameUpper == "OSGEO.SQLSERVERSPATIAL" || //NOXLATE
                providerNameUpper == "OSGEO.MYSQL" || //NOXLATE
                providerNameUpper == "OSGEO.POSTGRESQL" || //NOXLATE
                providerNameUpper == "OSGEO.ODBC" || //NOXLATE
                providerNameUpper == "OSGEO.ARCSDE" || //NOXLATE
                providerNameUpper == "OSGEO.WFS" || //NOXLATE
                providerNameUpper == "OSGEO.WMS" || //NOXLATE
                providerNameUpper == "KING.ORACLE" || //NOXLATE
                providerNameUpper == "AUTODESK.ORACLE") //NOXLATE
            {
                //Fortunately, all the above providers are universal in the naming choice of credential connection parameters
                if ((fsXml.Contains("<NAME>USERNAME</NAME>") && !fsXml.Contains(StringConstants.MgUsernamePlaceholder)) || (fsXml.Contains("<NAME>PASSWORD</NAME>") && !fsXml.Contains(StringConstants.MgPasswordPlaceholder))) //NOXLATE
                    issues.Add(new ValidationIssue(feature, ValidationStatus.Warning, ValidationStatusCode.Warning_FeatureSource_Plaintext_Credentials, Strings.FS_PlaintextCredentials));
                else
                    issues.Add(new ValidationIssue(feature, ValidationStatus.Warning, ValidationStatusCode.Warning_FeatureSource_Cannot_Package_Secured_Credentials, Strings.FS_CannotPackageSecuredCredentials));

                //Has the placeholder token(s)
                if (fsXml.Contains(StringConstants.MgUsernamePlaceholder) || fsXml.Contains(StringConstants.MgPasswordPlaceholder))
                {
                    //Find the MG_USER_CREDENTIALS resource data item
                    bool bFound = false;
                    var resData = this.Connection.ResourceService.EnumerateResourceData(feature.ResourceID);
                    foreach (var data in resData.ResourceData)
                    {
                        if (data.Name == StringConstants.MgUserCredentialsResourceData)
                        {
                            bFound = true;
                        }
                    }

                    if (!bFound)
                    {
                        issues.Add(new ValidationIssue(feature, ValidationStatus.Error, ValidationStatusCode.Error_FeatureSource_SecuredCredentialTokensWithoutSecuredCredentialData, Strings.FS_SecuredCredentialTokensWithoutSecuredCredentialData));
                    }
                }
            }

            //Note: Must be saved!
            string s = featSvc.TestConnection(feature.ResourceID);
            if (s.Trim().ToUpper() != true.ToString().ToUpper())
            {
                issues.Add(new ValidationIssue(feature, ValidationStatus.Error, ValidationStatusCode.Error_FeatureSource_ConnectionTestFailed, string.Format(Strings.FS_ConnectionTestFailed, s)));
                return issues.ToArray();
            }

            try
            {
                System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InvariantCulture;
                FdoSpatialContextList lst = context.GetSpatialContexts(feature.ResourceID);
                if (lst == null || lst.SpatialContext == null || lst.SpatialContext.Count == 0)
                {
                    issues.Add(new ValidationIssue(feature, ValidationStatus.Warning, ValidationStatusCode.Warning_FeatureSource_NoSpatialContext, Strings.FS_NoSpatialContextWarning));
                }
                else
                {
                    foreach (FdoSpatialContextListSpatialContext c in lst.SpatialContext)
                    {
                        if (c.Extent == null || c.Extent.LowerLeftCoordinate == null || c.Extent.UpperRightCoordinate == null)
                            issues.Add(new ValidationIssue(feature, ValidationStatus.Warning, ValidationStatusCode.Warning_FeatureSource_EmptySpatialContext, Strings.FS_EmptySpatialContextWarning));
                        else if (double.Parse(c.Extent.LowerLeftCoordinate.X, ci) <= -1000000 && double.Parse(c.Extent.LowerLeftCoordinate.Y, ci) <= -1000000 && double.Parse(c.Extent.UpperRightCoordinate.X, ci) >= 1000000 && double.Parse(c.Extent.UpperRightCoordinate.Y, ci) >= 1000000)
                            issues.Add(new ValidationIssue(feature, ValidationStatus.Warning, ValidationStatusCode.Warning_FeatureSource_DefaultSpatialContext, Strings.FS_DefaultSpatialContextWarning));
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = NestedExceptionMessageProcessor.GetFullMessage(ex);
                issues.Add(new ValidationIssue(feature, ValidationStatus.Error, ValidationStatusCode.Error_FeatureSource_SpatialContextReadError, string.Format(Strings.FS_SpatialContextReadError, msg)));
            }

            List<string> classes = new List<string>();
            try
            {
                var schemaNames = featSvc.GetSchemas(feature.ResourceID);
                if (schemaNames.Length == 0)
                    issues.Add(new ValidationIssue(feature, ValidationStatus.Warning, ValidationStatusCode.Warning_FeatureSource_NoSchemasFound, Strings.FS_SchemasMissingWarning));
            }
            catch (Exception ex)
            {
                var wex = ex as System.Net.WebException;
                if (wex != null) //Most likely timeout due to really large schema
                {
                    string msg = NestedExceptionMessageProcessor.GetFullMessage(ex);
                    issues.Add(new ValidationIssue(feature, ValidationStatus.Warning, ValidationStatusCode.Warning_FeatureSource_Validation_Timeout, string.Format(Strings.FS_ValidationTimeout, msg)));
                }
                else
                {
                    string msg = NestedExceptionMessageProcessor.GetFullMessage(ex);
                    issues.Add(new ValidationIssue(feature, ValidationStatus.Error, ValidationStatusCode.Error_FeatureSource_SchemaReadError, string.Format(Strings.FS_SchemaReadError, msg)));
                }
            }

            string configDocXml = feature.GetConfigurationContent(Connection);
            if (!string.IsNullOrEmpty(configDocXml))
            {
                var doc = ConfigurationDocument.LoadXml(configDocXml);
                var odbcDoc = doc as OdbcConfigurationDocument;
                if (odbcDoc != null)
                {
                    issues.AddRange(ValidateOdbcDoc(feature, odbcDoc));
                }
            }

            context.MarkValidated(resource.ResourceID);

            return issues.ToArray();
        }

        private IEnumerable<ValidationIssue> ValidateOdbcDoc(IFeatureSource fs, OdbcConfigurationDocument odbcDoc)
        {
            foreach (var schema in odbcDoc.Schemas)
            {
                var featureClasses = schema
                    .Classes
                    .Where(c => !string.IsNullOrEmpty(c.DefaultGeometryPropertyName) && c.Properties.Any(p => p.Type == Schema.PropertyDefinitionType.Geometry && p.Name == c.DefaultGeometryPropertyName));

                foreach (var fc in featureClasses)
                {
                    var geomProp = fc.Properties.OfType<GeometricPropertyDefinition>().FirstOrDefault(p => p.Name == fc.DefaultGeometryPropertyName);
                    if (geomProp != null)
                    {
                        //Must be point
                        if (geomProp.GeometricTypes != FeatureGeometricType.Point)
                        {
                            yield return new ValidationIssue(fs, ValidationStatus.Error, ValidationStatusCode.Error_OdbcConfig_InvalidLogicalGeometryProperty, string.Format(Strings.ODBC_InvalidGeometryProperty, fc.Name, geomProp.Name));
                        }

                        var ovTable = odbcDoc.GetOverride(schema.Name, fc.Name);
                        if (ovTable == null) //If it has geometry, it must have a table override
                        {
                            yield return new ValidationIssue(fs, ValidationStatus.Error, ValidationStatusCode.Error_OdbcConfig_NoTableOverrideForFeatureClass, string.Format(Strings.ODBC_NoSuchTableOverrideForFeatureClass, fc.Name));
                        }
                        else if (geomProp.GeometricTypes == FeatureGeometricType.Point)
                        {
                            if (string.IsNullOrEmpty(ovTable.XColumn) || string.IsNullOrEmpty(ovTable.YColumn))
                            {
                                yield return new ValidationIssue(fs, ValidationStatus.Error, ValidationStatusCode.Error_OdbcConfig_IncompleteXYZColumnMapping, string.Format(Strings.ODBC_IncompleteXYZColumnMapping, fc.Name));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the resource type and version this validator supports
        /// </summary>
        /// <value></value>
        public ResourceTypeDescriptor SupportedResourceAndVersion => new ResourceTypeDescriptor(ResourceTypes.FeatureSource.ToString(), _version);
    }
}