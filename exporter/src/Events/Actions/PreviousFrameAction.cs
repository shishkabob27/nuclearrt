using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class PreviousFrameAction : ActionBase
{
	public override int ObjectType { get; set; } = -3;
	public override int Num { get; set; } = 1;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		return $"Application::Instance().QueueStateChange(GameState::PreviousFrame);";
	}
}
