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

		string loopName = StringUtils.SanitizeObjectName(ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase).ToString());

		eventBase.ObjectInfoList = -1; // TODO: im doing this because or else it will write it as an "instance->???" rather than "player_selector->begin()->???"

		result.AppendLine("{");

		result.AppendLine($"loop_{loopName}_running = true;");
		result.AppendLine($"loop_{loopName}_index = 0;");
		result.AppendLine($"int loopTimes = {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)};");
		result.AppendLine($"while (loop_{loopName}_running && loop_{loopName}_index < loopTimes) {{");
		result.AppendLine($"    {loopName}_loop();");
		result.AppendLine($"    if (!loop_{loopName}_running) break;");
		result.AppendLine($"    loop_{loopName}_index++;");
		result.AppendLine("}");

		result.AppendLine("}");

		return result.ToString();
	}
}
