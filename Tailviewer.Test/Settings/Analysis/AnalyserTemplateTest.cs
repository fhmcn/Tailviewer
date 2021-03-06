﻿using FluentAssertions;
using Moq;
using NUnit.Framework;
using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;

namespace Tailviewer.Test.Settings.Analysis
{
	[TestFixture]
	public sealed class AnalyserTemplateTest
	{
		[Test]
		public void TestClone1()
		{
			var analysisConfiguration = new Mock<ILogAnalyserConfiguration>();
			analysisConfiguration.Setup(x => x.Clone()).Returns(new Mock<ILogAnalyserConfiguration>().Object);
			var widget = new AnalyserTemplate
			{
				Configuration = analysisConfiguration.Object
			};

			analysisConfiguration.Verify(x => x.Clone(), Times.Never);
			var clone = widget.Clone();
			clone.Should().NotBeNull();
			clone.Should().NotBeSameAs(widget);
			clone.Configuration.Should().NotBeNull();
			clone.Configuration.Should().NotBeSameAs(analysisConfiguration.Object);
			analysisConfiguration.Verify(x => x.Clone(), Times.Once);
		}

		sealed class TestConfiguration
			: ILogAnalyserConfiguration
		{
			public void Serialize(IWriter writer)
			{
				
			}

			public void Deserialize(IReader reader)
			{
				
			}

			public object Clone()
			{
				throw new System.NotImplementedException();
			}

			public bool IsEquivalent(ILogAnalyserConfiguration other)
			{
				throw new System.NotImplementedException();
			}
		}

		[Test]
		public void TestSerialize1()
		{
			var template = new AnalyserTemplate
			{
				Id = AnalyserId.CreateNew(),
				FactoryId = new LogAnalyserFactoryId("lkwdqjklowlkw"),
				Configuration = new TestConfiguration()
			};

			var actualTemplate = template.Roundtrip(typeof(TestConfiguration));
			actualTemplate.Should().NotBeNull();
			actualTemplate.Id.Should().Be(template.Id);
			actualTemplate.FactoryId.Should().Be(template.FactoryId);
			actualTemplate.Configuration.Should().NotBeNull();
			actualTemplate.Configuration.Should().BeOfType<TestConfiguration>();
		}

		[Test]
		[Description("Verifies that not being able to restore the configuration is NOT a problem")]
		public void TestSerialize2()
		{
			var template = new AnalyserTemplate
			{
				Configuration = new TestConfiguration()
			};

			// We don't pass the type of the expected sub-types so the configuration
			// cannot be restored. This can happen when opening a template / snapshot
			// on an older installation or one that doesn't have a particular plugin.
			var actualTemplate = template.Roundtrip();
			actualTemplate.Should().NotBeNull();
			actualTemplate.Configuration.Should().BeNull();
		}
	}
}
