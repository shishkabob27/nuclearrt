using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class TimerSetAction : ActionBase
{
	public override int ObjectType { get; set; } = -4;
	public override int Num { get; set; } = 0;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		if (eventBase.Items[0].Loader is Time time)
		{
			return $"GameTimer.SetTime({time.Timer});";
		}
		else if (eventBase.Items[0].Loader is ExpressionParameter expressionParameter)
		{
			return $"GameTimer.SetTime({ExpressionConverter.ConvertExpression(expressionParameter, eventBase)});";
		}

		return $"//Unsupported timer type: {eventBase.Items[0].Loader.GetType()}";
	}
}
