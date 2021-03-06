﻿using System;
using System.Linq;
using System.Threading;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.Analysis;
using Tailviewer.Core.Filters;
using Tailviewer.Core.LogFiles;
using Tailviewer.Core.Settings;

namespace Tailviewer.Count.BusinessLogic
{
	public sealed class LogEntryCountAnalyser
		: LogAnalyser
	{
		private const int MaximumLineCount = 10000;

		private LogEntryCountResult _result;
		private readonly ILogFile _logFile;
		private readonly bool _ownsLogFile;

		public LogEntryCountAnalyser(ITaskScheduler scheduler, ILogFile logFile, TimeSpan maximumWaitTime, LogEntryCountAnalyserConfiguration configuration)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));
			if (logFile == null)
				throw new ArgumentNullException(nameof(logFile));
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			var filter = CreateFilter(configuration.QuickFilters);
			if (filter != null)
			{
				_logFile = new FilteredLogFile(scheduler, maximumWaitTime, logFile,
					null,
					filter);
				_ownsLogFile = true;
			}
			else
			{
				_logFile = logFile;
			}

			// EVERYTHING MUST BE INITIALIZED AFTER THIS LINE AS OnLogFileModifiedInternal
			// WILL BE CALLED NEXT
			_logFile.AddListener(this, maximumWaitTime, MaximumLineCount);
		}

		private ILogEntryFilter CreateFilter(QuickFilters configurationQuickFilters)
		{
			return Filter.Create(configurationQuickFilters.Select(x => x.CreateFilter()));
		}

		public override ILogAnalysisResult Result => _result;

		public override Percentage Progress => Percentage.FromPercent((float)_logFile.Progress * 100);

		protected override void OnLogFileModifiedInternal(ILogFile logFile, LogFileSection section)
		{
			_result = new LogEntryCountResult
			{
				Count = _logFile.Count
			};
		}

		protected override void DisposeInternal()
		{
			_logFile.RemoveListener(this);
			if (_ownsLogFile)
			{
				_logFile.Dispose();
			}
		}
	}
}