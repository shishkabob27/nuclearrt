using CTFAK.CCN.Chunks.Frame;

public class LoopCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -1;
	public override int Num { get; set; } = -16;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		return "";
	}
}
