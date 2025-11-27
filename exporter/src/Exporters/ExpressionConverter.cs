using CTFAK.CCN;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.MFA;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using CTFAK.Utils;
using System.Text;

public class ExpressionConverter
{
	private readonly Exporter _exporter;

	public ExpressionConverter(Exporter exporter)
	{
		_exporter = exporter;
	}

	private enum ObjectType
	{
		Player = -7,
		Keyboard = -6,
		Create = -5,
		Timer = -4,
		Game = -3,
		Speaker = -2,
		System = -1,
		Arithmetic = 0,
		Backdrop = 1,
		Active = 2,
		Text = 3,
		Question = 4,
		Score = 5,
		Lives = 6,
		Counter = 7,
		Rtf = 8,
		SubApplication = 9,
		Extension = 32,
	}

	private static readonly Dictionary<(ObjectType, int), Func<Expression, string>> expressionsLookup = new()
	{
        //Player
        { (ObjectType.Player, 1), e => $"Application::Instance().GetAppData()->GetPlayerLives({e.ObjectInfo})" }, // Player Lives

        //Keyboard / Mouse
        { (ObjectType.Keyboard, 0), _ => "Application::Instance().GetInput()->GetMouseX()" }, // XMouse
        { (ObjectType.Keyboard, 1), _ => "Application::Instance().GetInput()->GetMouseY()" }, // YMouse
        { (ObjectType.Keyboard, 2), _ => "Application::Instance().GetInput()->GetMouseWheelMove()" }, // WheelDelta

        //Create
        { (ObjectType.Create, 0), _ => $"ObjectInstances.size()" }, // Total Objects

        //Timer
        { (ObjectType.Timer, 0), _ => "GameTimer.GetTime()" }, // Timer
        { (ObjectType.Timer, 1), _ => "GameTimer.GetHundreds()" }, // Hundreds
        { (ObjectType.Timer, 2), _ => "GameTimer.GetSeconds()" }, // seconds
        { (ObjectType.Timer, 3), _ => "GameTimer.GetMinutes()" }, // Minutes
        { (ObjectType.Timer, 4), _ => "GameTimer.GetHours()" }, // Hours
        { (ObjectType.Timer, 5), _ => $"0" }, // Event Index // TODO

        //Game
        { (ObjectType.Game, 0),  _ => $"Index + 1" }, // Frame
        { (ObjectType.Game, 8),  _ => $"Index + 1" }, // Frame
        { (ObjectType.Game, 10), _ => "Application::Instance().GetAppData()->GetTargetFPS()" }, // FrameRate // TODO: Verify this
        { (ObjectType.Game, 14), _ => "0" }, // DisplayMode // TODO
        { (ObjectType.Game, 15), _ => "0" }, // PixelShaderVersion // TODO

        //Speaker
        { (ObjectType.Speaker, 12), _ => "std::to_string(" }, // ChannelSampleName$ // TODO

        // System
        { (ObjectType.System, -3), _ => ", " },
		{ (ObjectType.System, -2), _ => ")" },
		{ (ObjectType.System, -1), _ => "(" },
		{ (ObjectType.System, 1),  _ => "Application::Instance().Random(" }, // Random(
        { (ObjectType.System, 2),  _ => $"Application::Instance().GetAppData()->GetGlobalValue(" }, // Global Value
        { (ObjectType.System, 3),  e => $"std::string(\"{e.Loader.ToString()}\")" },
		{ (ObjectType.System, 4),  _ => $"std::to_string(" }, // Str$
        { (ObjectType.System, 6),  _ => "\"\"" }, // Appdrive$ // TODO
        { (ObjectType.System, 7),  _ => "\"\"" }, // Appdir$ // TODO
        { (ObjectType.System, 8),  _ => "\"\"" }, // Apppath$ // TODO
        { (ObjectType.System, 9),  _ => "\"\"" }, // Appname$ // TODO
        { (ObjectType.System, 19), _ => "StringLeft(" }, // String Left
        { (ObjectType.System, 20), _ => "StringRight(" }, // String Right
        { (ObjectType.System, 22), _ => "StringLength(" }, // String Length
		{ (ObjectType.System, 23), e => (e.Loader as DoubleExp).FloatValue.ToString() },
        { (ObjectType.System, 29), _ => "std::abs(" }, // Abs(
        { (ObjectType.System, 41), _ => "std::max(" }, // Max(
        { (ObjectType.System, 46), _ => "0" }, // LoopIndex // TODO // NOTE: original code had i += 2, might be important?
		{ (ObjectType.System, 50), e => $"Application::Instance().GetAppData()->GetGlobalStrings()[{(e.Loader as GlobalCommon).Value}]" },
        { (ObjectType.System, 56), _ => "\"\"" }, // AppTempPath$ // TODO
        { (ObjectType.System, 65), _ => "Application::Instance().RandomRange(" }, // RRandom
        { (ObjectType.System, 67), _ => "Application::Instance().GetBackend()->GetPlatformName()" }, // RuntimeName$

        // Arithmetic
        { (ObjectType.Arithmetic, 2), _ => " + " }, // Add
        { (ObjectType.Arithmetic, 4), _ => " - " }, // Sub
        { (ObjectType.Arithmetic, 6), _ => " * " }, // Multiply
        { (ObjectType.Arithmetic, 8), _ => " /MathHelper::GetSafeDivision()/ " }, // Division
    };

	private static void HandleSystemExpr(StringBuilder stringBuilder, Expression expression)
	{
		switch (expression.Num)
		{
			case 0: //
				ExpressionLoader loader = (ExpressionLoader)expression.Loader;

				if (loader is StringExp) stringBuilder.Append($"std::string(\"{(loader as StringExp).Value}\")");
				else if (loader is DoubleExp) stringBuilder.Append((loader as DoubleExp).FloatValue);
				else stringBuilder.Append(loader.Value.ToString());
				break;
			default:
				HandleUnimplemented(stringBuilder, expression);
				break;
		}
	}
	private static void HandleRuntimeObjectExpr(StringBuilder stringBuilder, Expression expression, EventBase eventBase = null)
	{
		switch (expression.Num)
		{

		}
	}
	private static void HandleUnimplemented(StringBuilder result, Expression expression, EventBase eventBase = null)
	{

		result.Append($"/* Expression not found: ({expression.ObjectType}, {expression.Num}) */");
		Logger.Log($"No expresion match, ObjectType: {expression.ObjectType}, Num: {expression.Num}");
	}

	public static string ConvertExpression(ExpressionParameter expressions, EventBase eventBase = null)
	{
		//TODO: refactor this
		// TODO: use switch cases, maybe seperate every speecial object switch to its own function
		StringBuilder result = new();
		for (int i = 0; i < expressions.Items.Count; i++)
		{
			Expression expression = expressions.Items[i];

			if (expressionsLookup.TryGetValue(((ObjectType)expression.ObjectType, expression.Num), out var generator)) {
				result.Append(generator(expression));
				continue;
			}

			switch ((ObjectType)expression.ObjectType)
			{
				case ObjectType.System:
					HandleSystemExpr(result, expression); break; // for readablity sake, it is a seperate function

				default:
					if (expression.ObjectType > 0)
					{
						// Runtime Objects (Active, Text, Counter, etc.)
						HandleRuntimeObjectExpr(result, expression, eventBase);
					}
					else
					{
						HandleUnimplemented(result, expression, eventBase);
					}
					break;
			}
		}
		return result.ToString();
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

	public static string GetObjectClassName(int objectInfo, bool isGlobal = false, bool convertToCCN = true)
	{
		int ObjectType = 0;
		int ObjectInfo = 0;
		if (convertToCCN)
		{
			var obj = GetObject(objectInfo, isGlobal);
			if (obj.Item1 > short.MaxValue)
			{
				ObjectType = GetQualifierType(obj.Item1);
			}
			else
			{
				ObjectType = Exporter.Instance.GameData.frameitems[obj.Item1].ObjectType;
				ObjectInfo = obj.Item1;
			}
		}
		else
		{
			ObjectInfo = objectInfo;
			ObjectType = Exporter.Instance.GameData.frameitems[objectInfo].ObjectType;
		}

		switch (ObjectType)
		{
			case 0: return "QuickBackdrop";
			case 1: return "Backdrop";
			case 2: return "Active";
			case 3: return "StringObject";
			case 5: return "Score";
			case 6: return "Lives";
			case 7: return "Counter";
			case >= 32:
			ObjectCommon common = Exporter.Instance.GameData.frameitems[ObjectInfo].properties as ObjectCommon;
			ExtensionExporter exporter = ExtensionExporterRegistry.GetExporter(common.Identifier);
			return exporter?.CppClassName ?? "Extension";
			default: return "ObjectInstance";
		}
	}

	static int GetQualifierType(int qualifier)
	{
		return (qualifier & 0x7FFF) + 1;
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
