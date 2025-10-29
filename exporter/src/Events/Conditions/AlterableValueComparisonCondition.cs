using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class AlterableValueComparisonCondition : ConditionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = -27;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		result.AppendLine($"    if ((({ExpressionConverter.GetObjectClassName(eventBase.ObjectInfo)}*)instance)->Values.GetValue({((Short)eventBase.Items[0].Loader).Value}) {ExpressionConverter.GetOppositeComparison(((ExpressionParameter)eventBase.Items[1].Loader).Comparsion)} {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)}) it.deselect();");
		result.AppendLine("}");

		//If no instances are selected, we go to the end label
		result.AppendLine($"if ({GetSelector(eventBase.ObjectInfo)}->Count() == 0) goto {nextLabel};");

		return result.ToString();
	}
}

public class CounterAlterableValueComparisonCondition : AlterableValueComparisonCondition
{
	public override int ObjectType { get; set; } = 7;
}
