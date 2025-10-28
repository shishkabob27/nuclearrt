using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using CTFAK.Utils;

public class DestroyAction : ActionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = 24;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		result.AppendLine($"    MarkForDeletion(instance);");
		result.AppendLine($"    it.deselect();");
		result.AppendLine($"	{GetSelector(eventBase.ObjectInfo)}->RemoveInstance(instance->Handle);");
		//remove from qualifier selectors
		var obj = ExpressionConverter.GetObject(eventBase.ObjectType, true);
		if (Exporter.Instance.GameData.frameitems[(int)obj.Item1].properties is ObjectCommon common)
		{
			foreach (var qualifier in common._qualifiers)
			{
				if (qualifier != -1 && qualifier != (short)eventBase.ObjectInfo)
				{
					string qualifierSelector = StringUtils.SanitizeObjectName(Utilities.GetQualifierName(qualifier & 0x7FFF, eventBase.ObjectType)) + "_" + (32768 + qualifier) + "_selector";
					if (qualifierSelector != GetSelector(eventBase.ObjectInfo))
					{
						result.AppendLine($"	{qualifierSelector}->RemoveInstance(instance->Handle);");
					}
				}
			}
		}
		result.AppendLine("}");


		return result.ToString();
	}
}

public class DestroyStringAction : DestroyAction
{
	public override int ObjectType { get; set; } = 3;
}
