using CTFAK.CCN.Chunks.Frame;

public class AlwaysCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -1;
	public override int Num { get; set; } = -1;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		//always event doesn't need to write anything
		return $"";
	}
}
