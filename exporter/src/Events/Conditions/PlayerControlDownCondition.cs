using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class PlayerControlDownCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -7;
	public override int Num { get; set; } = -6;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		return $"{ifStatement} (Application::Instance().GetInput()->IsControlsDown({eventBase.ObjectInfo}, {((Short)eventBase.Items[0].Loader).Value}))) goto {nextLabel};";
	}
}
