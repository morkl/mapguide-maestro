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

using ICSharpCode.Core;
using Maestro.Base.Services;
using Maestro.Editors.Diagnostics;
using Maestro.Shared.UI;
using OSGeo.MapGuide.MaestroAPI.Services;
using System;

namespace Maestro.Base.Commands
{
    internal class ServerMonitorCommand : AbstractMenuCommand
    {
        public override void Run()
        {
            try
            {
                var wb = Workbench.Instance;
                var exp = wb.ActiveSiteExplorer;
                var connMgr = ServiceRegistry.GetService<ServerConnectionManager>();
                var conn = connMgr.GetConnection(exp.ConnectionName);

                ISiteService siteSvc = null;
                var svcTypes = conn.Capabilities.SupportedServices;
                if (Array.IndexOf(svcTypes, (int)ServiceType.Site) >= 0)
                {
                    siteSvc = (ISiteService)conn.GetService((int)ServiceType.Site);
                }

                if (siteSvc != null)
                    ServerStatusMonitor.Init(siteSvc);
                ServerStatusMonitor.ShowWindow();
            }
            catch (Exception ex)
            {
                ErrorDialog.Show(ex);
            }
        }
    }
}