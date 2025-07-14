using CTFAK.Memory;
using CTFAK.CCN.Chunks.Frame;
using System.Text;

public class ButtonObjectExporter : ExtensionExporter
{
	public override string ObjectIdentifier => "0ntB";
	public override string ExtensionName => "kcbutton";
	public override string CppClassName => "ButtonObjectExtension";

	public override string ExportExtension(byte[] extensionData)
	{
		ByteReader reader = new ByteReader(extensionData);

		short Width = reader.ReadInt16();
		short Height = reader.ReadInt16();
		//TODO: all the other stuff

		return CreateExtension($"{Width}, {Height}");
	}

	public override string ExportCondition(EventBase eventBase, int conditionNum, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (", bool isGlobal = false)
	{
		StringBuilder result = new StringBuilder();

		switch (conditionNum)
		{
			case -82: // "Button is clicked" condition
				result.AppendLine($"for (ObjectIterator it(*{ExpressionConverter.GetSelector(eventBase.ObjectInfo, isGlobal)}); !it.end(); ++it) {{");
				result.AppendLine($"    auto instance = *it;");
				result.AppendLine($"    auto buttonExt = {GetExtensionInstanceLoop()};");
				result.AppendLine($"    {ifStatement} buttonExt->IsClicked()) it.deselect();");
				result.AppendLine("}");
				result.AppendLine($"if ({ExpressionConverter.GetSelector(eventBase.ObjectInfo, isGlobal)}->Count() == 0) goto {nextLabel};");
				break;

			default:
				result.AppendLine($"// Button condition {conditionNum} not implemented");
				result.AppendLine($"goto {nextLabel};");
				break;
		}

		return result.ToString();
	}

	public override string ExportAction(EventBase eventBase, int actionNum, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, bool isGlobal = false)
	{
		StringBuilder result = new StringBuilder();

		switch (actionNum)
		{
			case 83: // Enable button
				result.AppendLine($"for (ObjectIterator it(*{ExpressionConverter.GetSelector(eventBase.ObjectInfo, isGlobal)}); !it.end(); ++it) {{");
				result.AppendLine($"    auto instance = *it;");
				result.AppendLine($"    auto buttonExt = {GetExtensionInstanceLoop()};");
				result.AppendLine($"    buttonExt->SetEnabled(true);");
				result.AppendLine("}");
				break;
			case 84: // Disable button
				result.AppendLine($"for (ObjectIterator it(*{ExpressionConverter.GetSelector(eventBase.ObjectInfo, isGlobal)}); !it.end(); ++it) {{");
				result.AppendLine($"    auto instance = *it;");
				result.AppendLine($"    auto buttonExt = {GetExtensionInstanceLoop()};");
				result.AppendLine($"    buttonExt->SetEnabled(false);");
				result.AppendLine("}");
				break;
			default:
				result.AppendLine($"// Button action {actionNum} not implemented");
				break;
		}

		return result.ToString();
	}
}
