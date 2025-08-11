using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class SetBackgroundColorAction : ActionBase
{
	public override int ObjectType { get; set; } = -3;
	public override int Num { get; set; } = 23;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		if (eventBase.Items[0].Loader is Colour colorParameter)
		{
			return $"BackgroundColor = {colorParameter.Value.ToArgb()};";
		}
		else if (eventBase.Items[0].Loader is ExpressionParameter expressionParameter)
		{
			return $"BackgroundColor = {ExpressionConverter.ConvertExpression(expressionParameter, eventBase)};";
		}
		
		return "";
	}
}
