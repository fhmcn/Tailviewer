using System.Xml;

namespace Tailviewer.Events.BusinessLogic
{
	/// <summary>
	/// Contains the settings for a single event of a <see cref="EventsLogAnalyser"/>.
	/// </summary>
	public sealed class EventConfiguration
	{
		public string Name;
		public string FilterExpression;

		public override string ToString()
		{
			return string.Format("{0}, {1}", Name, FilterExpression);
		}

		public void Restore(XmlReader reader)
		{
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "name":
						Name = reader.ReadContentAsString();
						break;
					case "filterexpression":
						FilterExpression = reader.ReadContentAsString();
						break;
				}
			}
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteAttributeString("name", Name);
			writer.WriteAttributeString("filterexpression", FilterExpression);
		}
	}
}