﻿using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Core.LogFiles;

namespace Tailviewer.Test.BusinessLogic.LogFiles
{
	[TestFixture]
	public sealed class LogFileColumnTest
	{
		[Test]
		public void TestToString1()
		{
			var column = new WellKnownLogFileColumn<string>("foobar");
			column.ToString().Should().Be("foobar: String");
		}
	}
}
