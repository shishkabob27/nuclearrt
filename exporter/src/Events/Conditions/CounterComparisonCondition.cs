using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class CounterComparisonCondition : ConditionBase
{
	public override int ObjectType { get; set; } = 7;
	public override int Num { get; set; } = -81;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		result.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
		result.AppendLine($"    auto value = std::dynamic_pointer_cast<Value>(commonProperties->oValue);");
		result.AppendLine($"    if (value->GetValue() {ExpressionConverter.GetOppositeComparison(((ExpressionParameter)eventBase.Items[0].Loader).Comparsion)} {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)}) it.deselect();");
		result.AppendLine("}");

		return result.ToString();
	}
}
