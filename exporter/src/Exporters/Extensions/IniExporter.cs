using CTFAK.Memory;
using CTFAK.CCN.Chunks.Frame;
using System.Text;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class IniExporter : ExtensionExporter
{
	public override string ObjectIdentifier => "0INI";
	public override string ExtensionName => "kcini";
	public override string CppClassName => "IniExtension";

	public override string ExportExtension(byte[] extensionData)
	{
		ByteReader reader = new ByteReader(extensionData);

		short flags = reader.ReadInt16();
		string name = reader.ReadWideString();
		if (string.IsNullOrWhiteSpace(name))
		{
			name = "default.ini";
		}

		return CreateExtension($"{flags}, \"{name}\"");
	}

	public override string ExportCondition(EventBase eventBase, int conditionNum, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (", bool isGlobal = false)
	{
		StringBuilder result = new();

		switch (conditionNum)
		{
			default:
				result.AppendLine($"// Ini condition {conditionNum} not implemented");
				result.AppendLine($"goto {nextLabel};");
				break;
		}

		return result.ToString();
	}

	public override string ExportAction(EventBase eventBase, int actionNum, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, bool isGlobal = false)
	{
		StringBuilder result = new();

		switch (actionNum)
		{
			case 80: // Set Current Group
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->SetCurrentGroup({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)});");
				break;
			case 81: // Set Current Item
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->SetCurrentItem({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)});");
				break;
			case 82: // Set Value
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->SetValue({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)});");
				break;
			case 83: // Save Position
				ParamObject paramObject = (ParamObject)eventBase.Items[0].Loader;
				string objectSelector = ExpressionConverter.GetSelector(paramObject.ObjectInfo);
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->SavePosition(&(**{objectSelector}->begin()));");
				break;
			case 84: // Load Position
				ParamObject paramObject2 = (ParamObject)eventBase.Items[0].Loader;
				string objectSelector2 = ExpressionConverter.GetSelector(paramObject2.ObjectInfo);
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->LoadPosition(&(**{objectSelector2}->begin()));");
				break;
			case 86: // Set File Name
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->SetFileName({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)});");
				break;
			case 87: // Set Value (Item)
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->SetValue({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)});");
				break;
			case 88: // Set Value (Group - Item)
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->SetValue({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[2].Loader, eventBase)});");
				break;
			case 85: // Set String
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->SetString({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)});");
				break;
			case 89: // Set String (Item)
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->SetString({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)});");
				break;
			case 90: // Set String (Group - Item)
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->SetString({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[2].Loader, eventBase)});");
				break;
			case 91: // Delete Item
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->DeleteItem({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)});");
				break;
			case 92: // Delete Item (Group - Item)
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->DeleteItem({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)});");
				break;
			case 93: // Delete Group
				result.AppendLine($"{GetExtensionInstance(eventBase.ObjectInfo)}->DeleteGroup({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)});");
				break;
			default:
				result.AppendLine($"// Ini action {actionNum} not implemented");
				break;
		}

		return result.ToString();
	}

	public override string ExportExpression(Expression expression, EventBase eventBase = null)
	{
		string result;

		switch (expression.Num)
		{
			case 80: // Get Value
			case 82: // Get Value (Item)
			case 83: // Get Value (Group - Item)
				result = $"{GetExtensionInstance(expression.ObjectInfo)}->GetValue(";
				break;
			case 81: // Get String
			case 84: // Get String (Item)
			case 85: // Get String (Group - Item)
				result = $"{GetExtensionInstance(expression.ObjectInfo)}->GetString(";
				break;
			default:
				result = $"0 /* Ini expression {expression.Num} not implemented */";
				break;
		}

		return result;
	}
}
