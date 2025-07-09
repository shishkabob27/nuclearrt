using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class UponClickingOnObjectCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -6;
	public override int Num { get; set; } = -7;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();
		Click click = (Click)eventBase.Items[0].Loader;
		int button = click.Button;
		if (button == 0)
			button = 1;
		else if (button == 1)
			button = 4;
		else if (button == 4)
			button = 1;

		result.AppendLine($"{ifStatement} (Application::Instance().GetInput()->IsMouseButtonPressed({button}, {(click.IsDouble == 0 ? false : true).ToString().ToLower()}))) goto {nextLabel};"); //check if the user is clicking

		//Check mouse is over object
		ParamObject paramObj = (ParamObject)eventBase.Items[1].Loader;
		result.AppendLine($"for (ObjectIterator it(*{GetSelector(paramObj.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    {ifStatement} (IsColliding(&(**it), GetMouseX(), GetMouseY()))) it.deselect();");
		result.AppendLine("}");
		result.AppendLine($"if ({GetSelector(paramObj.ObjectInfo)}->Count() == 0) goto {nextLabel};");

		return result.ToString();
	}
}
