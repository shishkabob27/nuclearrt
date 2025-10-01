using CTFAK.CCN.Chunks.Frame;
using CTFAK.Memory;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class UltimateFullscreenExporter : ExtensionExporter
{
	public override string ObjectIdentifier => "sFwU";

	public override string ExtensionName => "ultimatefullscreen";
	public override string CppClassName => "UltimateFullscreenExtension";
	public override string ExportExtension(byte[] extensionData)
	{
		ByteReader reader = new ByteReader(extensionData);


		return CreateExtension($"0");
	}

	public override string ExportCondition(EventBase eventBase, int conditionNum, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (", bool isGlobal = false)
	{
		StringBuilder result = new StringBuilder();
		switch (conditionNum)
		{
			case -81: // Is Fullscreen
				result.Append($"{ifStatement} ({GetExtensionInstance(eventBase.ObjectInfo)}->isFullscreen == true)) goto {nextLabel};");
				break;
			case -82: // Is Windowed
				result.Append($"{ifStatement} ({GetExtensionInstance(eventBase.ObjectInfo)}->isFullscreen == false)) goto {nextLabel};");
				break;
			default:
				result.Append($"// Ultimate Fullscreen condition {conditionNum} not implemented.\n");
				result.Append($"goto {nextLabel};");
				break;
		}
		return result.ToString();
	}
	public override string ExportAction(EventBase eventBase, int actionNum, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, bool isGlobal = false)
	{
		StringBuilder result = new StringBuilder();
		switch (actionNum)
		{
			case 80: // Go fullscreen
				result.Append($"{GetExtensionInstance(eventBase.ObjectInfo)}->GoFullscreen();");
				break;
			case 81: // Go Windowed
				result.Append($"{GetExtensionInstance(eventBase.ObjectInfo)}->GoWindowed();");
				break;
			default:
				result.Append($"// Ultimate Fullscreen action {actionNum} not implemented.");
				break;
		}
		return result.ToString();
	}
	public override string ExportExpression(Expression expression, EventBase eventBase = null)
	{
		string result;
		switch (expression.Num)
		{
			default:
				result = $"0 /* Ultimate Fullscreen expression {expression.Num} not implemeneted */";
				break;
		}
		return result;
	}
}
