using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class SetMovementDirectionAction : ActionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = 23;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		string direction = "";
		if (eventBase.Items[0].Loader is IntParam intParam) {
			direction = intParam.Value.ToString();
		}
		else if (eventBase.Items[0].Loader is ExpressionParameter expressionParameter) {
			direction = ExpressionConverter.ConvertExpression(expressionParameter, eventBase);
		}
		result.AppendLine($"    ((Active*)instance)->animations.SetForcedDirection({direction});");
		result.AppendLine($"    (({ExpressionConverter.GetObjectClassName(eventBase.ObjectInfo, IsGlobal)}*)instance)->movements.GetCurrentMovement()->SetMovementDirection({direction});");
		result.AppendLine("}");

		return result.ToString();
	}
}
