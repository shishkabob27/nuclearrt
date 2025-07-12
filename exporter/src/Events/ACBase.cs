using CTFAK.CCN.Chunks.Frame;

public class ACBase
{
	public virtual int ObjectType { get; set; }
	public virtual int Num { get; set; }

	public virtual bool IsGlobal { get; set; } = false;

	//if statement and next label are only used for conditions
	public virtual string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		return $"//{GetType().Name}";
	}

	public string GetSelector(int objectInfo)
	{
		return ExpressionConverter.GetSelector(objectInfo, IsGlobal);
	}
}
