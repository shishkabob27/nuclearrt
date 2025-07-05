using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class CenterDisplayAtAction : ActionBase
{
	public override int ObjectType { get; set; } = -3;
	public override int Num { get; set; } = 7;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		StringBuilder result = new StringBuilder();

		Position position = (Position)eventBase.Items[0].Loader;
		if (position.ObjectInfoParent == ushort.MaxValue) // Absolute position
		{
			result.AppendLine($"SetScroll({position.X}, {position.Y});");
		}
		else // Relative position from object
		{
			result.AppendLine($"for (ObjectIterator it(*{GetSelector((int)position.ObjectInfoParent)}); !it.end(); ++it) {{");
			result.AppendLine($"    auto parent = *it;");
			result.AppendLine($"    SetScroll({position.X} + parent->X, {position.Y} + parent->Y, parent->Layer);");
			result.AppendLine("}");
		}

		return result.ToString();
	}
}
