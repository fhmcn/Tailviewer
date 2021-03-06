﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Tailviewer.BusinessLogic.LogFiles;
using Tailviewer.Core.LogTables;

namespace Tailviewer.Core.Filters
{
	/// <summary>
	/// The interface for a filter that is responsible for deciding whether or not an entire <see cref="LogEntry"/> shall be visible
	/// or not.
	/// </summary>
	public interface ILogEntryFilter
		: ILogLineFilter
	{
		/// <summary>
		///     Tests if the given entry passes the filter.
		/// </summary>
		/// <param name="logEntry"></param>
		/// <returns></returns>
		[Pure]
		bool PassesFilter(IEnumerable<LogLine> logEntry);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		/// <param name="matches"></param>
		void Match(LogLine line, List<LogLineMatch> matches);
	}
}