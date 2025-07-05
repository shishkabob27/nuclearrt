using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class PlayerControlPressedCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -7;
	public override int Num { get; set; } = -4;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		return $"{ifStatement} (Application::Instance().GetInput()->IsControlsPressed({eventBase.ObjectInfo}, {((Short)eventBase.Items[0].Loader).Value}))) goto {nextLabel};";
	}
}
