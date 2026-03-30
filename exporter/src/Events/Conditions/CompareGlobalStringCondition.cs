using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class CompareGlobalStringCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -1;
	public override int Num { get; set; } = -20;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		if (eventBase.Items[0].Loader is Short shortValue)
		{
			return $"{ifStatement} (Application::Instance().GetAppData()->GetGlobalString({shortValue.Value}) {ExpressionConverter.GetComparisonSymbol(((ExpressionParameter)eventBase.Items[1].Loader).Comparsion)} {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)})) goto {nextLabel};";
		}
		if (eventBase.Items[0].Loader is ExpressionParameter expressionParameter)
		{
			return $"{ifStatement} ((Application::Instance().GetAppData()->GetGlobalString({ExpressionConverter.ConvertExpression(expressionParameter, eventBase)} - 1) {ExpressionConverter.GetComparisonSymbol(((ExpressionParameter)eventBase.Items[1].Loader).Comparsion)} {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)} - 1))) goto {nextLabel};"; // -1 since GetGlobalString is 0-indexed
		}
		return "";
	}
}
