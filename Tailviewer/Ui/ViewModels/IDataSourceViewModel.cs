﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Metrolib;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.Core.Filters;

namespace Tailviewer.Ui.ViewModels
{
	public interface IDataSourceViewModel
		: INotifyPropertyChanged
	{
		ICommand OpenInExplorerCommand { get; }

		string DisplayName { get; }

		/// <summary>
		/// A user-readable description of the origin of the data source.
		/// Could be a simpel filesystem path, a URL, etc...
		/// </summary>
		string DataSourceOrigin { get; }

		int TotalCount { get; }

		int OtherCount { get; }

		int DebugCount { get; }

		int InfoCount { get; }

		int WarningCount { get; }

		int NoTimestampCount { get; }

		int ErrorCount { get; }

		int FatalCount { get; }

		Size FileSize { get; }
		bool IsVisible { get; set; }
		LogLineIndex VisibleLogLine { get; set; }
		double HorizontalOffset { get; set; }

		HashSet<LogLineIndex> SelectedLogLines { get; set; }

		TimeSpan LastWrittenAge { get; }

		ICommand RemoveCommand { get; }

		bool FollowTail { get; set; }

		bool ShowLineNumbers { get; set; }

		bool ColorByLevel { get; set; }

		bool HideEmptyLines { get; set; }

		bool IsSingleLine { get; set; }

		double Progress { get; }

		#region Search

		string SearchTerm { get; set; }
		int SearchResultCount { get; }
		int CurrentSearchResultIndex { get; set; }

		#endregion

		DateTime LastViewed { get; }

		IDataSource DataSource { get; }

		LevelFlags LevelsFilter { get; set; }
		IDataSourceViewModel Parent { get; set; }
		IEnumerable<ILogEntryFilter> QuickFilterChain { get; set; }

		void RequestBringIntoView(LogLineIndex index);

		event Action<IDataSourceViewModel, LogLineIndex> OnRequestBringIntoView;
		event Action<IDataSourceViewModel> Remove;

		void Update();
	}
}