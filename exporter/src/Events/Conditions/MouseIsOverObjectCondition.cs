using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class MouseIsOverObjectCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -6;
	public override int Num { get; set; } = -4;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();

		ParamObject obj = (ParamObject)eventBase.Items[0].Loader;
		result.AppendLine($"for (ObjectIterator it(*{GetSelector(obj.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    {ifStatement} (IsColliding(&(**it), GetMouseX(), GetMouseY()))) it.deselect();");
		result.AppendLine("}");
		result.AppendLine($"if ({GetSelector(obj.ObjectInfo)}->Count() == 0) goto {nextLabel};");

		return result.ToString();
	}
}
