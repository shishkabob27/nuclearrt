using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
public class PlaySample : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 0;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();
		result.AppendLine($"Application::Instance().GetBackend()->LoadSample({((Sample)eventBase.Items[0].Loader).Handle});");
		result.AppendLine($"Application::Instance().GetBackend()->PlaySample({((Sample)eventBase.Items[0].Loader).Handle}, -1, 2, NULL, false);");
		
		return result.ToString();
	}
}

public class PlaySampleChannel : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 11;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();
		result.AppendLine($"Application::Instance().GetBackend()->LoadSample({((Sample)eventBase.Items[0].Loader).Handle});");
		result.AppendLine($"Application::Instance().GetBackend()->PlaySample({((Sample)eventBase.Items[0].Loader).Handle}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)}, 2, NULL, false);");

		return result.ToString();
	}
}
/*public class PlayAndLoopSample : PlaySample
{
	public override int Num { get; set; } = -2;
}*/
