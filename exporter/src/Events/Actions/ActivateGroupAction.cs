using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class ActivateGroupAction : ActionBase
{
	public override int ObjectType { get; set; } = -1;
	public override int Num { get; set; } = 6;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();

		result.AppendLine($"SetGroupActive({((GroupPointer)eventBase.Items[0].Loader).Id}, true);");

		return result.ToString();
	}
}
