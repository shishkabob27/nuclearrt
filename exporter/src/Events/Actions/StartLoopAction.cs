using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class StartLoopAction : ActionBase
{
	public override int ObjectType { get; set; } = -1;
	public override int Num { get; set; } = 14;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();

		eventBase.ObjectInfoList = -1; // NOTE: im doing this because or else object expressions will be writen as "instance->???" rather than "player_selector->begin()->???"

		string loopName = ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase);
		string count = ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase);

		result.AppendLine($"StartLoop({loopName}, {count});");

		return result.ToString();
	}
}
