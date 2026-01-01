using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using CTFAK.Utils;
public class EmptyAction : ActionBase
{
	public override int ObjectType { get; set; } = -1;
	public override int Num { get; set; } = 0;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		return ""; // Truth be told this isn't needed but why not.
	}
}
