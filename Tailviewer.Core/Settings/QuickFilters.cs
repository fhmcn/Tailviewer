﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Tailviewer.Core.Settings
{
	/// <summary>
	///     The list of all application-wide quick filters.
	/// </summary>
	public sealed class QuickFilters
		: List<QuickFilter>
			, ICloneable
	{
		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		///     Restores the values of this object from the given xml document.
		/// </summary>
		/// <param name="reader"></param>
		public void Restore(XmlReader reader)
		{
			var quickfilters = new List<QuickFilter>();
			var subtree = reader.ReadSubtree();
			while (subtree.Read())
				switch (subtree.Name)
				{
					case "quickfilter":
						var quickfilter = new QuickFilter();
						if (quickfilter.Restore(subtree))
							quickfilters.Add(quickfilter);
						break;
				}

			Clear();
			AddRange(quickfilters);
		}

		/// <summary>
		///     Stores the values of this object in the given xml document.
		/// </summary>
		/// <param name="writer"></param>
		public void Save(XmlWriter writer)
		{
			foreach (var dataSource in this)
			{
				writer.WriteStartElement("quickfilter");
				dataSource.Save(writer);
				writer.WriteEndElement();
			}
		}

		/// <summary>
		///     Returns a deep clone of this object.
		/// </summary>
		/// <returns></returns>
		public QuickFilters Clone()
		{
			var filters = new QuickFilters();
			filters.AddRange(this.Select(x => x.Clone()));
			return filters;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, objB: null))
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			var other = obj as QuickFilters;
			if (ReferenceEquals(other, objB: null))
				return false;

			if (Count != other.Count)
				return false;

			for (var i = 0; i < Count; ++i)
			{
			}

			return true;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return 42;
		}

		/// <summary>
		///     Tests if this and the given object are equivalent (i.e. would behave identical, or not).
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool IsEquivalent(QuickFilters other)
		{
			if (ReferenceEquals(other, objB: null))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			if (Count != other.Count)
				return false;

			for (var i = 0; i < Count; ++i)
			{
				var filter = this[i];
				var otherFilter = other[i];

				if (!IsEquivalent(filter, otherFilter))
					return false;
			}

			return true;
		}

		private static bool IsEquivalent(QuickFilter lhs, QuickFilter rhs)
		{
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (ReferenceEquals(lhs, objB: null))
				return false;

			return lhs.IsEquivalent(rhs);
		}
	}
}