using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class AnimationPlayingCondition : ConditionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = -3;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		result.AppendLine($"    if (!((Active*)instance)->animations.IsSequencePlaying({((Short)eventBase.Items[0].Loader).Value})) it.deselect();");
		result.AppendLine("}");

		result.AppendLine($"if ({GetSelector(eventBase.ObjectInfo)}->Count() == 0) goto {nextLabel};");

		return result.ToString();
	}
}
