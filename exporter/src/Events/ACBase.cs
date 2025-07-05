using CTFAK.CCN.Chunks.Frame;

public class ACBase
{
	public virtual int ObjectType { get; set; }
	public virtual int Num { get; set; }

	//if statement and next label are only used for conditions
	public virtual string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		return $"//{GetType().Name}";
	}

	public string GetSelector(int objectInfo)
	{
		return ExpressionConverter.GetSelector(objectInfo);
	}
}
