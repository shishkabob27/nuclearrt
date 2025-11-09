using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using CTFAK.Utils;

public class PreviousMovementAction : ActionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = 12;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		result.AppendLine($"    (({ExpressionConverter.GetObjectClassName(eventBase.ObjectInfo, IsGlobal)}*)instance)->movements.PreviousMovement();");
		result.AppendLine("}");

		return result.ToString();
	}
}

public class CounterPreviousMovementAction : PreviousMovementAction
{
	public override int ObjectType { get; set; } = 7;
}