#region Disclaimer / License
// Copyright (C) 2008, Kenneth Skovhede
// http://www.hexad.dk, opensource@hexad.dk
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Xml;
using System.Collections.Generic;

namespace Globalizator
{
	/// <summary>
	/// Summary description for ResourceManager.
	/// </summary>
	public class ResourceManager
	{
		private Hashtable m_localities = new Hashtable();

		public ResourceManager()
		{
			XmlDocument doc = new XmlDocument();
			string locpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Localization");
			if (!Directory.Exists(locpath))
				return;

            //Look for "es" or "es-ES" like folders 
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("([A-z][A-z])(\\-[A-z][A-z])?");

            foreach (string folder in Directory.GetDirectories(locpath))
            {
                if (regex.Match(Path.GetFileName(folder)).Value == Path.GetFileName(folder))
                    foreach (string s in Directory.GetFiles(folder, "*.resx"))
                    {
                        try
                        {
                            string classname = Path.GetFileNameWithoutExtension(s);
                            CultureInfo ci = CultureInfo.InvariantCulture;
                            //TODO: Will break on filenames like: Namespace.12345.resx
                            if (Path.GetExtension(classname) != null && Path.GetExtension(classname).Length == 6)
                                try
                                {
                                    ci = CultureInfo.CreateSpecificCulture(Path.GetExtension(classname).Substring(1));
                                    classname = Path.GetFileNameWithoutExtension(classname);
                                }
                                catch
                                {
                                    continue;
                                }

                            if (!m_localities.ContainsKey(ci))
                                m_localities.Add(ci, new Hashtable());

                            doc.Load(s);
                            Hashtable translations = (Hashtable)m_localities[ci];

                            foreach (XmlNode n in doc.SelectNodes("root/data"))
                                if (n["value"] != null)
                                    translations[n.Attributes["name"].Value] = n["value"].InnerText;

                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format("Failed while reading file: {0}\nMessage: {1}", s, ex));
                        }
                    }
            }
		}

		public CultureInfo[] AvalibleCultures
		{
			get 
			{
				CultureInfo[] cil = new CultureInfo[m_localities.Count];
				int i = 0;
				foreach(CultureInfo ci in m_localities.Keys)
					cil[i++] = ci;

				return cil;
			}
		}


		public string GetString(string val, CultureInfo ci)
		{
			Hashtable files;
			if (m_localities.ContainsKey(ci))
				files = (Hashtable)m_localities[ci];
			else if (m_localities.ContainsKey(CultureInfo.InvariantCulture))
				files = (Hashtable)m_localities[CultureInfo.InvariantCulture];
			else
				return null;

			if (files.ContainsKey(val))
				return (string)files[val];
			else
				return null;
		}
	}
}
