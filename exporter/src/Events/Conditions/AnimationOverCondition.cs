using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class AnimationOverCondition : ConditionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = -2;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		StringBuilder result = new();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		result.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
		result.AppendLine($"    auto animations = std::dynamic_pointer_cast<Animations>(commonProperties->oAnimations);");
		result.AppendLine($"    if (!animations->IsSequenceOver({((Short)eventBase.Items[0].Loader).Value})) it.deselect();");
		result.AppendLine("}");

		result.AppendLine($"if ({GetSelector(eventBase.ObjectInfo)}->Count() == 0) goto {nextLabel};");

		return result.ToString();
	}
}
