using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class CenterDisplayXAction : ActionBase
{
	public override int ObjectType { get; set; } = -3;
	public override int Num { get; set; } = 8;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		result.AppendLine($"    SetScrollX(instance->" + (eventBase.Num == 8 ? "X" : "Y") + ");");
		result.AppendLine("}");

		return result.ToString();
	}
}

public class CenterDisplayYAction : CenterDisplayXAction
{
	public override int Num { get; set; } = 9;
}
