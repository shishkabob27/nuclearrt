using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class JumpFrameAction : ActionBase
{
	public override int ObjectType { get; set; } = -3;
	public override int Num { get; set; } = 2;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		string frame;
		if (eventBase.Items[0].Loader is ExpressionParameter)
			frame = $"(({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)}) - 1)";
		else
			frame = Exporter.Instance.GameData.frameHandles.Items[((Short)eventBase.Items[0].Loader).Value].ToString();

		return $"Application::Instance().QueueStateChange(GameState::JumpToFrame, {frame});";
	}
}
