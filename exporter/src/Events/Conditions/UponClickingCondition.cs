using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class UponClickingCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -6;
	public override int Num { get; set; } = -5;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		int button = ((Click)eventBase.Items[0].Loader).Button;
		if (button == 0)
			button = 1;
		else if (button == 1)
			button = 4;
		else if (button == 4)
			button = 1;

		return $"{ifStatement} (Application::Instance().GetInput()->IsMouseButtonPressed({button}, {(((Click)eventBase.Items[0].Loader).IsDouble == 0 ? false : true).ToString().ToLower()}))) goto {nextLabel};";
	}
}
