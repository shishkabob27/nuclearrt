using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class SetGlobalStringAction : ActionBase
{
	public override int ObjectType { get; set; } = -1;
	public override int Num { get; set; } = 19;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		if (eventBase.Items[0].Loader is Short shortValue)
		{
			return $"Application::Instance().GetAppData()->SetGlobalString({shortValue.Value}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)});";
		}
		if (eventBase.Items[0].Loader is ExpressionParameter expressionParameter)
		{
			return $"Application::Instance().GetAppData()->SetGlobalString({ExpressionConverter.ConvertExpression(expressionParameter, eventBase)} - 1, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)});"; // -1 since SetGlobalString is 0-indexed
		}
		return "";
	}
}
