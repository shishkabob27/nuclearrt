using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class PickOneOfCondition : ConditionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = -34;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();

		result.AppendLine($"{GetSelector(eventBase.ObjectInfo)}->SelectRandom();");
		result.AppendLine($"if ({GetSelector(eventBase.ObjectInfo)}->Count() == 0) goto {nextLabel};");

		return result.ToString();
	}
}
