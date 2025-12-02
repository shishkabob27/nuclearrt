using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class AnimationSequenceAction : ActionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = 17;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();

		string sequenceValue = "";
		if (eventBase.Items[0].Loader is Short shortLoader) {
			sequenceValue = shortLoader.Value.ToString();
		}
		else if (eventBase.Items[0].Loader is ExpressionParameter expressionParameter) {
			sequenceValue = ExpressionConverter.ConvertExpression(expressionParameter, eventBase);
		}

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		result.AppendLine($"    ((Active*)instance)->animations.SetCurrentSequenceIndex({sequenceValue});");
		result.AppendLine("}");

		return result.ToString();
	}
}
