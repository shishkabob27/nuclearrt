using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class StopLoopAction : ActionBase
{
	public override int ObjectType { get; set; } = -1;
	public override int Num { get; set; } = 15;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();

		string loopName = StringUtils.SanitizeObjectName(ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase).ToString());

		result.AppendLine($"loop_{loopName}_running = false;");

		return result.ToString();
	}
}
