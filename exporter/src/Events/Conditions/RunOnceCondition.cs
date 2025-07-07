using System.Text;
using CTFAK.CCN.Chunks.Frame;

public class RunOnceCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -1;
	public override int Num { get; set; } = -6;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		StringBuilder result = new();

		result.AppendLine($"if (event_{parameters["eventIndex"]}_run_once) goto {nextLabel};");
		result.AppendLine($"event_{parameters["eventIndex"]}_run_once = true;");

		return result.ToString();
	}
}
