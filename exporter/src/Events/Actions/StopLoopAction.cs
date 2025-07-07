using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class StopLoopAction : ActionBase
{
	public override int ObjectType { get; set; } = -1;
	public override int Num { get; set; } = 15;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		StringBuilder result = new();

		string loopName = StringUtils.SanitizeObjectName(ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase).ToString());
		loopName = loopName.Substring(2, loopName.Length - 4); // remove first 2 letters and last 2 letters as they are quotes

		result.AppendLine($"loop_{loopName}_running = false;");

		return result.ToString();
	}
}
