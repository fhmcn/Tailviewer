﻿using Tailviewer.BusinessLogic.LogFiles;

namespace Tailviewer.BusinessLogic.LogTables
{
	/// <summary>
	/// This <see cref="ILogTable"/> implementation offers a tabular view onto a <see cref="ILogFile"/>
	/// by interpreting each log entry using a given pattern, such as the ones used by log4net.
	/// </summary>
	public sealed class LogFileTable
		: ILogTable
	{
		public int RowCount
		{
			get { throw new System.NotImplementedException(); }
		}

		public int ColumnCount
		{
			get { throw new System.NotImplementedException(); }
		}

		public LogTableRow this[int index]
		{
			get { throw new System.NotImplementedException(); }
		}
	}
}