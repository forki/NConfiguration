using System;
using System.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using NConfiguration.Monitoring;
using System.Collections.Generic;
using NConfiguration.Combination;
using NConfiguration.Joining;
using NConfiguration.Serialization;

namespace NConfiguration
{
	/// <summary>
	/// Application setting extensions.
	/// </summary>
	public static class AppSettingExtensions
	{
		internal static readonly string IdentitySectionName = "Identity";

		internal static readonly string WatchFileSectionName = "WatchFile";

		internal static string GetIdentitySource(this IConfigNodeProvider nodeProvider, string defaultIdentity)
		{
			foreach(var node in nodeProvider.ByName(IdentitySectionName))
				return node.Text;

			return defaultIdentity;
		}

		internal static FileMonitor GetMonitoring(this IConfigNodeProvider nodeProvider, string fileName, byte[] expectedContent)
		{
			foreach (var node in nodeProvider.ByName(IdentitySectionName))
			{
				var cfg = DefaultDeserializer.Instance.Deserialize<WatchFileConfig>(node);
				if (cfg.Mode == WatchMode.None)
					return null;

				return new FileMonitor(fileName, expectedContent, cfg.Mode, cfg.Delay);
			}

			return null;
		}

		public static IAppSettings AsSingleSettings(this IConfigNodeProvider nodeProvider)
		{
			return new SingleAppSettings(nodeProvider);
		}

		/// <summary>
		/// Gets the name of the section in DataContractAttribute or class name
		/// </summary>
		/// <returns>The section name.</returns>
		/// <typeparam name='T'>type of configuration</typeparam>
		public static string GetSectionName<T>()
		{
			return typeof(T).GetSectionName();
		}

		public static T TryGet<T>(this IAppSettings settings)
		{
			return settings.TryGet<T>(GetSectionName<T>());
		}

		public static T TryGet<T>(this IAppSettings settings, string sectionName)
		{
			var cfgs = settings.LoadSections<T>(sectionName).GetEnumerator();

			if (!cfgs.MoveNext())
				return default(T);

			T sum = cfgs.Current;

			while (cfgs.MoveNext())
				sum = settings.Combiner.Combine<T>(sum, cfgs.Current);

			return sum;
		}

		public static IEnumerable<T> LoadSections<T>(this IAppSettings settings, string sectionName)
		{
			return settings.Nodes.ByName(sectionName).Select(_ => settings.Deserializer.Deserialize<T>(_));
		}

		public static IEnumerable<T> LoadSections<T>(this IAppSettings settings)
		{
			return settings.LoadSections<T>(GetSectionName<T>());
		}

		/// <summary>
		/// Gets the name of the section in DataContractAttribute or class name
		/// </summary>
		/// <returns>The section name.</returns>
		/// <typeparam name='T'>type of configuration</typeparam>
		public static string GetSectionName(this Type type)
		{
			var dataAttrName = type.GetCustomAttributes(typeof(DataContractAttribute), false)
				.Select(a => (a as DataContractAttribute).Name)
				.FirstOrDefault();

			if(string.IsNullOrWhiteSpace(dataAttrName))
				return type.Name;
			else
				return dataAttrName;
		}

		public static T TryFirst<T>(this IAppSettings settings)
		{
			return settings.TryFirst<T>(GetSectionName<T>());
		}

		public static T TryFirst<T>(this IAppSettings settings, string sectionName)
		{
			foreach (var result in settings.LoadSections<T>(sectionName))
				return result;

			return default(T);
		}

		public static T First<T>(this IAppSettings settings)
		{
			return settings.First<T>(GetSectionName<T>());
		}

		public static T First<T>(this IAppSettings settings, string sectionName)
		{
			foreach(var result in settings.LoadSections<T>(sectionName))
				return result;

			throw new SectionNotFoundException(sectionName, typeof(T));
		}

		public static T TryFirst<T>(this IConfigNodeProvider nodeProvider, bool createDefaultInstance) where T : class
		{
			foreach (var node in nodeProvider.ByName(GetSectionName<T>()))
				return DefaultDeserializer.Instance.Deserialize<T>(node);

			return createDefaultInstance ? Activator.CreateInstance<T>() : null;
		}

		public static T TryFirst<T>(this IAppSettings settings, bool createDefaultInstance) where T : class
		{
			return settings.TryFirst<T>(GetSectionName<T>(), createDefaultInstance);
		}

		public static T TryFirst<T>(this IAppSettings settings, string sectionName, bool createDefaultInstance) where T : class
		{
			foreach (var result in settings.LoadSections<T>(sectionName))
				return result;

			return createDefaultInstance ? Activator.CreateInstance<T>() : null;
		}

		public static T Get<T>(this IAppSettings settings)
		{
			return settings.Get<T>(GetSectionName<T>());
		}

		public static T Get<T>(this IAppSettings settings, string sectionName)
		{
			var cfgs = settings.LoadSections<T>(sectionName).GetEnumerator();

			if (!cfgs.MoveNext())
				throw new SectionNotFoundException(sectionName, typeof(T));

			T sum = cfgs.Current;

			while (cfgs.MoveNext())
				sum = settings.Combiner.Combine<T>(sum, cfgs.Current);

			return sum;
		}
	
	
	}
}

