using CTFAK.CCN.Chunks.Frame;
using CTFAK.Memory;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class WindowControlExporter : ExtensionExporter
{
	public override string ObjectIdentifier => "LTCW";
	public override string ExtensionName => "kcwctrl";
	public override string CppClassName => "WindowControlExtension";
	public override string ExportExtension(byte[] extensionData)
	{
		ByteReader reader = new ByteReader(extensionData);

		short flags = reader.ReadInt16();

		return CreateExtension($"{flags}");
	}
	public override string ExportCondition(EventBase eventBase, int conditionNum, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (", bool isGlobal = false)
	{
		StringBuilder result = new StringBuilder();
		switch (conditionNum)
		{
			default:
				result.AppendLine($"// Window Control condition {conditionNum} not implemented");
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
			case 98: // Set Title Action
				result.AppendLine($"Application::Instance().GetBackend()->SetTitle({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)});");
				break;
			case 80: // Set Window Horizontal Position
				result.AppendLine($"Application::Instance().GetBackend()->ChangeWindowPosX({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)});");
				break;
			case 81: // Set Window Vertical Position
				result.AppendLine($"Application::Instance().GetBackend()->ChangeWindowPosY({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)});");
				break;
			default:
				result.AppendLine($"// Window Control action {actionNum} not implemented");
				break;
		}
		return result.ToString();
	}
	public override string ExportExpression(Expression expression, EventBase eventBase = null)
	{
		string result;
		switch (expression)
		{
			default:
				result = $"0 /* Window Control expression {expression.Num} not implemented */";
				break;
		}
		return result;
	}
}
