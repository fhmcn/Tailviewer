﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Metrolib;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.BusinessLogic.Plugins;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Archiver.Plugins
{
	/// <summary>
	///     Responsible for finding and loading plugins in a directory tree.
	///     Is only used in development mode (Tailviewer.exe -d)
	/// </summary>
	public sealed class PluginAssemblyLoader
		: AbstractPluginLoader
		, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly IReadOnlyList<Type> PluginInterfaces;

		static PluginAssemblyLoader()
		{
			PluginInterfaces = new[] {typeof(IFileFormatPlugin)};
		}

		/// <summary>
		///     Loads all plugins in the given directory path (recursive).
		///     Plugins are .NET assemblies compiled against AnyCPU that implement at least
		///     one of the available <see cref="IPlugin" /> interfaces.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public PluginAssemblyLoader(string path = null)
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

			try
			{
				if (path != null)
				{
					Log.InfoFormat("Looking for plugins in '{0}'...", path);
					var assemblies = Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories);
					foreach (var plugin in assemblies)
					{
						IPluginDescription description;
						TryLoad(plugin, out description);
						Add(description);
					}
				}
			}
			catch (DirectoryNotFoundException e)
			{
				Log.WarnFormat("Unable to find plugins in '{0}': {1}", path, e);
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
		}

		/// <inheritdoc />
		public override T Load<T>(IPluginDescription description)
		{
			if (description == null)
				throw new ArgumentNullException(nameof(description));

			var assembly = Assembly.LoadFrom(description.FilePath);
			var fullTypeName = description.Plugins[typeof(T)];
			var implementation = assembly.GetType(fullTypeName);
			if (implementation == null)
				throw new ArgumentException(string.Format("Plugin '{0}' does not define a type named '{1}'",
					description.FilePath,
					fullTypeName));

			var plugin = (T) Activator.CreateInstance(implementation);
			return plugin;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pluginPath"></param>
		/// <returns></returns>
		public IPluginDescription ReflectPlugin(string pluginPath)
		{
			Log.InfoFormat("Loading plugin '{0}'...", pluginPath);

			var assembly = Assembly.LoadFrom(pluginPath);
			return ReflectPlugin(assembly, pluginPath);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="pluginPath"></param>
		/// <returns></returns>
		public IPluginDescription ReflectPlugin(Assembly assembly, string pluginPath = null)
		{
			var idAttribute = assembly.GetCustomAttribute<PluginIdAttribute>();
			var authorAttribute = assembly.GetCustomAttribute<PluginAuthorAttribute>();
			var websiteAttribute = assembly.GetCustomAttribute<PluginWebsiteAttribute>();
			var descriptionAttribute = assembly.GetCustomAttribute<PluginDescriptionAttribute>();
			var versionAttribute = assembly.GetCustomAttribute<PluginVersionAttribute>();

			if (idAttribute == null)
				throw new PackException(string.Format("Plugin '{0}' is missing the reqired PluginId attribute, please add it", pluginPath));

			if (string.IsNullOrWhiteSpace(idAttribute.Id))
				throw new PackException(string.Format("The id of plugin '{0}' is required to be non-null and to consists of at least one non-whitespace character", pluginPath));

			if (authorAttribute == null)
				Log.WarnFormat("Plugin '{0}' is missing the PluginAuthor attribute, please consider adding it",
					pluginPath);

			if (websiteAttribute == null)
				Log.WarnFormat("Plugin '{0}' is missing the PluginWebsite attribute, please consider adding it",
					pluginPath);

			if (descriptionAttribute == null)
				Log.WarnFormat("Plugin '{0}' is missing the PluginDescription attribute, please consider adding it",
					pluginPath);

			Version pluginVersion;
			if (versionAttribute == null)
			{
				pluginVersion = new Version(0, 0, 0);
				Log.WarnFormat("Plugin '{0}' is missing the PluginVersion attribute, please consider adding it. Defaulting to {1}",
					pluginPath,
					pluginVersion);
			}
			else
			{
				pluginVersion = versionAttribute.Version;
			}

			var plugins = FindPluginImplementations(assembly);
			if (plugins.Count == 0)
				Log.WarnFormat("Plugin '{0}' doesn't implement any of the available plugin interfaces: {1}",
					pluginPath,
					string.Join(", ", PluginInterfaces.Select(x => x.Name)));

			return new PluginDescription
			{
				Id = idAttribute.Id,
				Name = assembly.GetName().Name,
				Author = authorAttribute?.Author,
				Website = websiteAttribute?.Website,
				Description = descriptionAttribute?.Description,
				Version = pluginVersion,
				FilePath = pluginPath,
				Plugins = plugins
			};
		}

		private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			// We will allow the user to load assemblies that are not an exact match to the installed assemblies:
			// This list consists of the Api assembly and all of its dependencies:
			var assemblies = new Dictionary<string, Assembly>
			{
				{"log4net,", typeof(ILog).Assembly},
				{"Metrolib,", typeof(AbstractBootstrapper).Assembly},
				{"System.Threading.Extensions,", typeof(ITaskScheduler).Assembly},
				{"Tailviewer.Api,", typeof(ILogFile).Assembly},
				{"Tailviewer.Core,", typeof(TextLogFile).Assembly}
			};

			// TODO: This feature most certainly needs proper tests. Go write them!

			foreach (var assembly in assemblies)
				if (args.Name.StartsWith(assembly.Key))
				{
					Log.InfoFormat("Assembly '{0}' requests to load '{1}', resolving to '{2}'",
						args.RequestingAssembly,
						args.Name,
						assembly.Value
					);
					return assembly.Value;
				}

			return null;
		}

		private bool TryLoad(string filePath, out IPluginDescription description)
		{
			try
			{
				description = ReflectPlugin(filePath);
				return true;
			}
			catch (FileLoadException e)
			{
				var error = string.Format("Unable to load plugin '{0}': {1}", filePath, e);
				Log.Error(error);
				description = new PluginDescription
				{
					FilePath = filePath,
					Error = error
				};
				return false;
			}
			catch (BadImageFormatException e)
			{
				var error = string.Format("Unable to load plugin '{0}' (plugins must be compiled against AnyCPU): {1}", filePath,
					e);
				Log.Error(error);
				description = new PluginDescription
				{
					FilePath = filePath,
					Error = error
				};
				return false;
			}
			catch (Exception e)
			{
				var error = string.Format("Caught unexpected exception while trying to load plugin '{0}': {1}",
					filePath, e);
				Log.Error(error);
				description = new PluginDescription
				{
					FilePath = filePath,
					Error = error
				};
				return false;
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="assembly"></param>
		/// <returns></returns>
		private IReadOnlyDictionary<Type, string> FindPluginImplementations(Assembly assembly)
		{
			var plugins = new Dictionary<Type, string>();
			foreach (var type in assembly.ExportedTypes)
			foreach (var @interface in PluginInterfaces)
				if (type.GetInterface(@interface.FullName) != null)
					plugins.Add(@interface, type.FullName);

			// TODO: Inspect non-public types and log a warning if one implements the IPlugin interface
			return plugins;
		}
	}
}