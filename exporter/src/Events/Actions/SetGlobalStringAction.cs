using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class SetGlobalStringAction : ActionBase
{
	public override int ObjectType { get; set; } = -1;
	public override int Num { get; set; } = 19;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();
		result.AppendLine($"Application::Instance().GetAppData()->SetGlobalString({((Short)eventBase.Items[0].Loader).Value},{ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)});");
		return result.ToString();
	}
}
