using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class SpreadAlterableValueAction : ActionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = 34;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();

		result.AppendLine($"{{");
		result.AppendLine($"	int currentValue = {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)};");
		result.AppendLine($"	for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"		auto instance = *it;");
		result.AppendLine($"		auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
		result.AppendLine($"		commonProperties->oAlterableValues->SetValue({((AlterableValue)eventBase.Items[0].Loader).Value}, currentValue);");
		result.AppendLine($"		currentValue++;");
		result.AppendLine($"	}}");
		result.AppendLine($"}}");

		return result.ToString();
	}
}
