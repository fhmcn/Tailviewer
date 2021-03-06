﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Metrolib;
using Tailviewer.BusinessLogic.LogFiles;

namespace LogFile2Playground
{
	public sealed class TextLogFile2
		: ILogFile2, ILogLineAccessor
	{
		private readonly LogLineCache _cache;

		public TextLogFile2(ISerialTaskScheduler scheduler)
		{
			_cache = new LogLineCache(scheduler, this, Size.FromMegabytes(10));
		}

		public Task<LogLineResponse> RequestAsync(LogFileSection section)
		{
			throw new NotImplementedException();
		}

		public Task<LogLineResponse> RequestAsync(LogFileSection section, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public LogLineResponse Request(LogFileSection section)
		{
			throw new NotImplementedException();
		}
	}
}