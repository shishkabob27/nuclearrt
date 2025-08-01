using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MFA;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.Utils;

public class ExpressionConverter
{
	private readonly Exporter _exporter;

	public ExpressionConverter(Exporter exporter)
	{
		_exporter = exporter;
	}

	public static string ConvertExpression(ExpressionParameter expressions, EventBase eventBase = null)
	{
		//TODO: refactor this

		string result = string.Empty;
		for (int i = 0; i < expressions.Items.Count; i++)
		{
			Expression expression = expressions.Items[i];
			if (expression.ObjectType == -6 && expression.Num == 0) // XMouse
			{
				result += "Application::Instance().GetInput()->GetMouseX()";
			}
			else if (expression.ObjectType == -6 && expression.Num == 1) // YMouse
			{
				result += "Application::Instance().GetInput()->GetMouseY()";
			}
			else if (expression.ObjectType == -6 && expression.Num == 2) // WheelDelta
			{
				result += "Application::Instance().GetInput()->GetMouseWheelMove()";
			}
			else if (expression.ObjectType == -5 && expression.Num == 0) // Total Objects
			{
				result += $"ObjectInstances.size()";
			}
			else if (expression.ObjectType == -4 && expression.Num == 0) // Timer
			{
				result += "GameTimer.GetTime()";
			}
			else if (expression.ObjectType == -4 && expression.Num == 1) // Hundreds
			{
				result += "GameTimer.GetHundreds()";
			}
			else if (expression.ObjectType == -4 && expression.Num == 2) // seconds
			{
				result += "GameTimer.GetSeconds()";
			}
			else if (expression.ObjectType == -4 && expression.Num == 3) // Minutes
			{
				result += "GameTimer.GetMinutes()";
			}
			else if (expression.ObjectType == -4 && expression.Num == 4) // Hours
			{
				result += "GameTimer.GetHours()";
			}
			else if (expression.ObjectType == -4 && expression.Num == 5) // Event Index
			{
				result += $"0"; // TODO
			}
			else if (expression.ObjectType == -3 && (expression.Num == 0 || expression.Num == 8)) // Frame
			{
				result += $"Index + 1";
			}
			else if (expression.ObjectType == -3 && expression.Num == 10) // FrameRate
			{
				result += "Application::Instance().GetAppData()->GetTargetFPS()"; // TODO: Verify this
			}
			else if (expression.ObjectType == -3 && expression.Num == 14) // DisplayMode
			{
				result += "0"; // TODO
			}
			else if (expression.ObjectType == -3 && expression.Num == 15) // PixelShaderVersion
			{
				result += "0"; // TODO
			}
			else if (expression.ObjectType == -2 && expression.Num == 12) // ChannelSampleName$
			{
				result += "std::to_string("; // TODO
			}
			else if (expression.ObjectType == -1 && expression.Num == -3)
			{
				result += ", ";
			}
			else if (expression.ObjectType == -1 && expression.Num == -2)
			{
				result += ")";
			}
			else if (expression.ObjectType == -1 && expression.Num == -1)
			{
				result += "(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 0)
			{
				ExpressionLoader loader = (ExpressionLoader)expression.Loader;

				if (loader is StringExp) result += $"\"{(loader as StringExp).Value}\"";
				else if (loader is DoubleExp) result += (loader as DoubleExp).FloatValue;
				else result += loader.Value.ToString();
			}
			else if (expression.ObjectType == -1 && expression.Num == 1) // Random(
			{
				result += "Application::Instance().Random(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 2) // Global Value
			{
				result += $"Application::Instance().GetAppData()->GetGlobalValue(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 3)
			{
				result += $"\"{expression.Loader.ToString()}\"";
			}
			else if (expression.ObjectType == -1 && expression.Num == 4) // Str$
			{
				result += $"std::to_string(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 6) // Appdrive$
			{
				result += "\"\""; // TODO
			}
			else if (expression.ObjectType == -1 && expression.Num == 7) // Appdir$
			{
				result += "\"\""; // TODO
			}
			else if (expression.ObjectType == -1 && expression.Num == 8) // Apppath$
			{
				result += "\"\""; // TODO
			}
			else if (expression.ObjectType == -1 && expression.Num == 9) // Appname$
			{
				result += "\"\""; // TODO
			}
			else if (expression.ObjectType == -1 && expression.Num == 19) // String Left
			{
				result += "StringLeft(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 20) // String Right
			{
				result += "StringRight(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 22) // String Length
			{
				result += "StringLength(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 23)
			{
				result += (expression.Loader as DoubleExp).FloatValue;
			}
			else if (expression.ObjectType == -1 && expression.Num == 29) // Abs(
			{
				result += "std::abs(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 41) // Max(
			{
				result += "std::max(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 46) // LoopIndex
			{
				result += "0"; // TODO
							   //skip
				i += 2;
			}
			else if (expression.ObjectType == -1 && expression.Num == 50) // Global String
			{
				result += $"Application::Instance().GetAppData()->GetGlobalStrings()[{(expression.Loader as GlobalCommon).Value}]";
			}
			else if (expression.ObjectType == -1 && expression.Num == 56) // AppTempPath$
			{
				result += "\"\""; // TODO
			}
			else if (expression.ObjectType == -1 && expression.Num == 65) // RRandom
			{
				result += "Application::Instance().RandomRange(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 67) // RuntimeName$
			{
				result += "Application::Instance().GetBackend()->GetPlatformName()";
			}
			else if (expression.ObjectType == 0 && expression.Num == 2) // Add
			{
				result += " + ";
			}
			else if (expression.ObjectType == 0 && expression.Num == 4) // Sub
			{
				result += " - ";
			}
			else if (expression.ObjectType == 0 && expression.Num == 6) // Multiply
			{
				result += " * ";
			}
			else if (expression.ObjectType == 0 && expression.Num == 8) // Division
			{
				result += " /MathHelper::safeDivision/ ";
			}
			else if (expression.ObjectType > 0 && expression.Num == 1) // Y Position
			{
				if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
					result += "instance->Y";
				else
					result += $"(*{GetSelector(expression.ObjectInfo)}->begin())->Y";
			}
			else if (expression.ObjectType > 0 && expression.Num == 2) // Image
			{
				if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
					result += "std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties)->oAnimations->GetCurrentFrameIndex()";
				else
					result += $"std::dynamic_pointer_cast<CommonProperties>((*{GetSelector(expression.ObjectInfo)}->begin())->OI->Properties)->oAnimations->GetCurrentFrameIndex()";
			}
			else if (expression.ObjectType > 0 && expression.Num == 11) // X Position
			{
				if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
					result += "instance->X";
				else
					result += $"(*{GetSelector(expression.ObjectInfo)}->begin())->X";
			}
			else if (expression.ObjectType > 0 && expression.Num == 12) // Fixed Value
			{
				result += "0"; // TODO
			}
			else if (expression.ObjectType > 0 && expression.Num == 14) // Animation Number
			{
				if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
					result += "std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties)->oAnimations->GetCurrentSequenceIndex()";
				else
					result += $"std::dynamic_pointer_cast<CommonProperties>((*{GetSelector(expression.ObjectInfo)}->begin())->OI->Properties)->oAnimations->GetCurrentSequenceIndex()";
			}
			else if (expression.ObjectType > 0 && expression.Num == 16) // Alterable Value
			{
				if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
					result += "std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties)->oAlterableValues->GetValue(" + ((ShortExp)expression.Loader).Value + ")";
				else
					result += $"std::dynamic_pointer_cast<CommonProperties>((*{GetSelector(expression.ObjectInfo)}->begin())->OI->Properties)->oAlterableValues->GetValue({((ShortExp)expression.Loader).Value})";
			}
			else if (expression.ObjectType > 0 && expression.Num == 22) // Font Color
			{
				result += "0"; // TODO
			}
			else if (expression.ObjectType > 0 && expression.Num == 27) // Alpha Coefficient
			{
				if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
					result += "instance->OI->BlendCoefficient";
				else
					result += $"(*{GetSelector(expression.ObjectInfo)}->begin())->OI->BlendCoefficient";
			}
			else if (expression.ObjectType > 0 && expression.Num == 46)
			{
				result += "instance->InstanceValue";
			}
			else if (expression.ObjectType > 0 && expression.Num == 80)
			{
				if (expression.ObjectType == 7) // Counter
				{
					string selector = GetSelector(expression.ObjectInfo);
					result += $"std::dynamic_pointer_cast<CommonProperties>((*({selector}->begin()))->OI->Properties)->oValue->GetValue()";
				}
			}
			else if (expression.ObjectType > 0 && expression.Num == 81)
			{
				if (expression.ObjectType == 3) // String
				{
					if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
						result += "std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties)->oParagraphs->GetText()";
					else
						result += $"std::dynamic_pointer_cast<CommonProperties>((*{GetSelector(expression.ObjectInfo)}->begin())->OI->Properties)->oParagraphs->GetText()";
				}
			}
			else if (expression.ObjectType >= 32)
			{
				var exporter = ExtensionExporterRegistry.GetExporterByObjectInfo(expression.ObjectInfo, Exporter.Instance.CurrentFrame);

				if (exporter == null)
				{
					Logger.Log($"Extension exporter not found for ObjectInfo {expression.ObjectInfo}");
					return "0";
				}

				result += exporter.ExportExpression(expression, eventBase);
			}
			else if (expression.ObjectType > 0 && expression.Num == 83)
			{
				if (expression.ObjectType == 2) // Angle
				{
					if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
						result += "instance->GetAngle()";
					else
						result += $"(*{GetSelector(expression.ObjectInfo)}->begin())->GetAngle()";
				}
			}
			else
			{
				Logger.Log($"No expresion match, ObjectType: {expression.ObjectType}, Num: {expression.Num}");
			}
		}
		return result;
	}

	public static string GetComparisonSymbol(short comparison)
	{
		switch (comparison)
		{
			case 0: return "==";
			case 1: return "!=";
			case 2: return "<=";
			case 3: return "<";
			case 4: return ">=";
			case 5: return ">";
			default: return "==";
		}
	}

	public static string GetOppositeComparison(short comparison)
	{
		switch (comparison)
		{
			case 0: return "!=";
			case 1: return "==";
			case 2: return ">";
			case 3: return ">=";
			case 4: return "<";
			case 5: return "<=";
			default: return "!=";
		}
	}

	public static string GetSelector(int objectInfo, bool isGlobal = false)
	{
		var obj = GetObject(objectInfo, isGlobal);
		return $"{StringUtils.SanitizeObjectName(obj.Item2)}_{obj.Item1}_selector";
	}

	public static Tuple<int, string, ObjectInstance> GetObject(int objectInfo, bool isGlobal = false, int frame = -1)
	{
		int FrameIndex = Exporter.Instance.CurrentFrame;
		if (frame != -1) FrameIndex = frame;

		string objectName = "";
		int objectType = 0;
		int systemQualifier = 0;
		ObjectInstance objectInstance = null;

		List<EventObject> eventObjects;
		if (isGlobal)
		{
			eventObjects = Exporter.Instance.MfaData.GlobalEvents.Objects;
		}
		else
		{
			eventObjects = Exporter.Instance.MfaData.Frames[FrameIndex].Events.Objects;
		}

		foreach (var evtObj in eventObjects)
		{
			if (evtObj.Handle == objectInfo)
			{
				objectName = evtObj.Name;
				objectType = evtObj.ObjectType;
				systemQualifier = evtObj.SystemQualifier;

				//Find object name in ccn frame
				foreach (var ccnObj in Exporter.Instance.GameData.Frames[FrameIndex].objects)
				{
					if (objectName == Exporter.Instance.GameData.frameitems[(int)ccnObj.objectInfo].name)
					{
						objectInfo = ccnObj.objectInfo;
						objectInstance = ccnObj;
						break;
					}
				}
				break;
			}
		}

		if (systemQualifier != 0)
		{
			objectName = Utilities.GetQualifierName(systemQualifier, objectType - 1);
			objectInfo = short.MaxValue + systemQualifier + 1;
		}

		return new Tuple<int, string, ObjectInstance>(objectInfo, objectName, objectInstance);
	}
}
