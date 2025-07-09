using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class RepeatWhileMousePressedCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -6;
	public override int Num { get; set; } = -8;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		return $"{ifStatement} (Application::Instance().GetInput()->IsMouseButtonDown({((KeyParameter)eventBase.Items[0].Loader).Key}))) goto {nextLabel};";
	}
}
