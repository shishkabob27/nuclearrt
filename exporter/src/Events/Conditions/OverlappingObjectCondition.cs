using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class OverlappingObjectCondition : ConditionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = -4;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		result.AppendLine($"    bool hasCollision = false;");
		result.AppendLine($"    for (ObjectIterator other(*{GetSelector(((ParamObject)eventBase.Items[0].Loader).ObjectInfo)}); !other.end(); ++other) {{");
		result.AppendLine($"        if (IsColliding(&(**it), &(**other))) {{");
		result.AppendLine($"            hasCollision = true;");
		result.AppendLine($"        }}");
		result.AppendLine($"        else {{");
		result.AppendLine($"            {ifStatement}false) other.deselect();");
		result.AppendLine($"        }}");
		result.AppendLine($"    }}");
		result.AppendLine($"    {ifStatement} hasCollision) it.deselect();");
		result.AppendLine("}");

		//If no instances are selected, we go to the end label
		result.AppendLine($"if ({GetSelector(eventBase.ObjectInfo)}->Count() == 0) goto {nextLabel};");

		return result.ToString();
	}
}
