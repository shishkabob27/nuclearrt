using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using CTFAK.Utils;

public class MovementStartAction : ActionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = 5;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		result.AppendLine($"    (({ExpressionConverter.GetObjectClassName(eventBase.ObjectInfo, IsGlobal)}*)instance)->movements.GetCurrentMovement()->Start();");
		result.AppendLine("}");

		result.AppendLine($"if ({GetSelector(eventBase.ObjectInfo)}->Count() == 0) goto {nextLabel};");

		return result.ToString();
	}
}

public class CounterMovementStartAction : MovementStartAction
{
	public override int ObjectType { get; set; } = 7;
}