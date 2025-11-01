using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class ToggleFlagAction : ActionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = 37;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		result.AppendLine($"    (({ExpressionConverter.GetObjectClassName(eventBase.ObjectInfo, IsGlobal)}*)instance)->Flags.ToggleFlag({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)});");
		result.AppendLine("}");

		return result.ToString();
	}
}
