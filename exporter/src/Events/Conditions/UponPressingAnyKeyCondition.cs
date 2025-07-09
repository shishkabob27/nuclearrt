using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class UponPressingAnyKeyCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -6;
	public override int Num { get; set; } = -9;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		return $"{ifStatement} (Application::Instance().GetInput()->IsAnyKeyPressed())) goto {nextLabel};";
	}
}
