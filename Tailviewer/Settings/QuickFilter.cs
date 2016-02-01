﻿using System;
using System.Xml;

namespace Tailviewer.Settings
{
	internal sealed class QuickFilter
	{
		private Guid _id;
		public Guid Id { get { return _id; } }
		public QuickFilterType Type;
		public string Value;
		public bool IgnoreCase;

		public QuickFilter()
		{
			_id = Guid.NewGuid();
			IgnoreCase = true;
		}

		public bool Restore(XmlReader reader)
		{
			int count = reader.AttributeCount;
			for (int i = 0; i < count; ++i)
			{
				reader.MoveToAttribute(i);

				switch (reader.Name)
				{
					case "id":
						_id = reader.ReadContentAsGuid();
						break;

					case "type":
						Type = reader.ReadContentAsEnum<QuickFilterType>();
						break;

					case "value":
						Value = reader.Value;
						break;

					case "ignorecase":
						IgnoreCase = reader.ReadContentAsBool();
						break;
				}
			}

			if (Id == Guid.Empty)
				return false;

			return true;
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeGuid("id", Id);
			writer.WriteAttributeEnum("type", Type);
			writer.WriteAttributeString("value", Value);
			writer.WriteAttributeBool("ignorecase", IgnoreCase);
		}
	}
}