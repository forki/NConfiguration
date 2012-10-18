using System;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace Configuration.Joining
{
	public class IncludeXmlElementEventArgs : EventArgs
	{
		public SettingsLoader Loader { get; private set; }
		public XElement IncludeElement { get; private set; }
		public IAppSettings BaseSettings { get; private set; }
		public bool Handled { get; set; }

		public IncludeXmlElementEventArgs(SettingsLoader loader, IAppSettings baseSettings, XElement includeElement)
		{
			Loader = loader;
			BaseSettings = baseSettings;
			IncludeElement = includeElement;
			Handled = false;
		}

		private List<IAppSettings> _addedSettings = new List<IAppSettings>();

		public IEnumerable<IAppSettings> AddedSettings
		{
			get
			{
				return _addedSettings;
			}
		}

		public void Add(IAppSettings settings)
		{
			_addedSettings.Add(settings);
		}
	}
}
