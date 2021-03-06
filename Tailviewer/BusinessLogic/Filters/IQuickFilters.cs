﻿using System.Collections.Generic;
using Tailviewer.Core;

namespace Tailviewer.BusinessLogic.Filters
{
	public interface IQuickFilters
	{
		IEnumerable<QuickFilter> Filters { get; }

		/// <summary>
		///     Adds a new quickfilter.
		/// </summary>
		/// <returns></returns>
		QuickFilter Add();

		void Remove(QuickFilterId id);
	}
}