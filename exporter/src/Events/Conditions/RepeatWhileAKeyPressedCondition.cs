using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class RepeatWhileAKeyPressedCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -6;
	public override int Num { get; set; } = -2;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		return $"{ifStatement} (Application::Instance().GetInput()->IsKeyDown({((KeyParameter)eventBase.Items[0].Loader).Key}))) goto {nextLabel};";
	}
}
