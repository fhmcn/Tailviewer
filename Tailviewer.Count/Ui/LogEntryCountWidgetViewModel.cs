﻿using Tailviewer.BusinessLogic.Analysis;
using Tailviewer.Core.Analysis;
using Tailviewer.Count.BusinessLogic;
using Tailviewer.Templates.Analysis;

namespace Tailviewer.Count.Ui
{
	public sealed class EntryCountWidgetViewModel
		: AbstractWidgetViewModel
	{
		private readonly LogEntryCountAnalyserConfiguration _configuration;
		private readonly FiltersViewModel _quickFilters;
		private long? _count;
		private string _caption;

		public EntryCountWidgetViewModel(IWidgetTemplate template, IDataSourceAnalyser dataSourceAnalyser)
			: base(template, dataSourceAnalyser)
		{
			_configuration = AnalyserConfiguration as LogEntryCountAnalyserConfiguration;
			Title = "Line Count";
			Caption = "Line(s)";
			_quickFilters = new FiltersViewModel(_configuration?.QuickFilters);
		}

		public long? Count
		{
			get { return _count; }
			private set
			{
				if (value == _count)
					return;

				_count = value;
				EmitPropertyChanged();
			}
		}

		public string Caption
		{
			get { return _caption; }
			set
			{
				if (value == _caption)
					return;

				_caption = value;
				EmitPropertyChanged();
			}
		}

		public FiltersViewModel Filters => _quickFilters;

		public override void Update()
		{
			LogEntryCountResult result;
			if (TryGetResult(out result))
			{
				Count = result.Count;
			}
			else
			{
				Count = null;
			}

			base.Update();
		}
	}
}