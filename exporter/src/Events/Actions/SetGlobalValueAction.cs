using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class SetGlobalValueAction : ActionBase
{
	public override int ObjectType { get; set; } = -1;
	public override int Num { get; set; } = 3;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		if (eventBase.Items[0].Loader is GlobalValue globalValue)
		{
			return $"Application::Instance().GetAppData()->SetGlobalValue({globalValue.Value + 1}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)});"; // +1 because SetGlobalValue is 1-indexed
		}
		if (eventBase.Items[0].Loader is ExpressionParameter expressionParameter)
		{
			return $"Application::Instance().GetAppData()->SetGlobalValue({ExpressionConverter.ConvertExpression(expressionParameter, eventBase)}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)});";
		}
		return "";
	}
}
