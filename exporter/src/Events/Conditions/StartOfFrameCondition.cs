using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class StartOfFrameCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -3;
	public override int Num { get; set; } = -1;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		return $"{ifStatement} (Application::Instance().GetCurrentState() == GameState::StartOfFrame)) goto {nextLabel};";
	}
}
