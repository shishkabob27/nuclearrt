using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class MoveBehindOfAction : ActionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = 59;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		result.AppendLine($"    MoveObjectBehindOf(instance, ({ExpressionConverter.GetObject(((ParamObject)eventBase.Items[0].Loader).ObjectInfo, IsGlobal).Item1}));");
		result.AppendLine("}");

		return result.ToString();
	}
}
