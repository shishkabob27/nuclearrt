using System.Text;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class PlayMusic : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 2;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();
		result.AppendLine($"Application::Instance().GetBackend()->LoadMusic({((Sample)eventBase.Items[0].Loader).Handle};");
		return result.ToString();
	}
}
