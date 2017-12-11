﻿using System;
using Tailviewer.BusinessLogic;
using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.Core.LogFiles
{
	/// <summary>
	///     Provides access to well-known columns which are provided by all log files:
	///     Every column has a well-defined meaning which will never change.
	/// </summary>
	public static class LogFileColumns
	{
		/// <summary>
		///     The index of the log entry, from 0 to the number of log entries.
		/// </summary>
		/// <remarks>
		///     Change property to <see cref="LogEntryIndex" /> once <see cref="LogLineIndex" />
		///     has been removed.
		/// </remarks>
		public static readonly ILogFileColumn<LogLineIndex> Index;

		/// <summary>
		///     The index of the log entry another one was created from.
		///     Only differs from <see cref="Index" /> when the log entry has been created
		///     through operations such as filtering, merging, etc...
		/// </summary>
		/// <remarks>
		///     Change property to <see cref="LogEntryIndex" /> once <see cref="LogLineIndex" />
		///     has been removed.
		/// </remarks>
		public static readonly ILogFileColumn<LogLineIndex> OriginalIndex;

		/// <summary>
		/// </summary>
		public static readonly ILogFileColumn<string> RawContent;

		/// <summary>
		///     The amount of time between this and the last log entry.
		/// </summary>
		public static readonly ILogFileColumn<TimeSpan?> DeltaTime;

		/// <summary>
		///     The amount of time elapsed between the first and this log entry.
		/// </summary>
		public static readonly ILogFileColumn<TimeSpan?> ElapsedTime;

		/// <summary>
		///     The absolute timestamp of when the log entry was produced.
		/// </summary>
		public static readonly ILogFileColumn<DateTime?> TimeStamp;

		static LogFileColumns()
		{
			Index = new WellKnownLogFileColumn<LogLineIndex>("index", "Index");
			OriginalIndex = new WellKnownLogFileColumn<LogLineIndex>("original_index", "Original Index");
			RawContent = new WellKnownLogFileColumn<string>("raw_content", "Raw Content");
			DeltaTime = new WellKnownLogFileColumn<TimeSpan?>("delta_time", "Delta Time");
			ElapsedTime = new WellKnownLogFileColumn<TimeSpan?>("elapsed_time", "Elapsed Time");
			TimeStamp = new WellKnownLogFileColumn<DateTime?>("timestamp", "Timestamp");
		}
	}
}