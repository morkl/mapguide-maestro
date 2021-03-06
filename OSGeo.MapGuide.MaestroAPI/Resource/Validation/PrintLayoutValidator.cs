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

namespace OSGeo.MapGuide.MaestroAPI.Resource.Validation
{
    /// <summary>
    /// Resource validator for Print Layouts
    /// </summary>
    public class PrintLayoutValidator : IResourceValidator
    {
        /// <summary>
        /// The server connection which validation will be performed against
        /// </summary>
        public IServerConnection Connection { get; set; }

        /// <summary>
        /// Gets the resource type and version this validator supports
        /// </summary>
        /// <value></value>
        public ResourceTypeDescriptor SupportedResourceAndVersion => ResourceTypeDescriptor.PrintLayout;

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

            if (resource.ResourceType != ResourceTypes.PrintLayout.ToString())
                return null;

            //TODO: What problems could there possibly be?

            context.MarkValidated(resource.ResourceID);

            return new ValidationIssue[0];
        }
    }
}