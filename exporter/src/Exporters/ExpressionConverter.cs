using CTFAK.CCN;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.MFA;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using CTFAK.Utils;
using System.Text;
using static CTFAK.CCN.Constants;
using OT = CTFAK.CCN.Constants.ObjectType;
using System.Text;

public class ExpressionConverter
{
	private readonly Exporter _exporter;

	public ExpressionConverter(Exporter exporter)
	{
		_exporter = exporter;
	}
	// most function output are in the string builder itself, no need to return values when the argument is modified
	private static void HandlePlayerExpr(StringBuilder stringBuilder, Expression expressions)
	{
		switch (expressions.Num) {

		}
	}
	private static void HandleInputExpr(StringBuilder stringBuilder, Expression expressions)
	{
		switch (expressions.Num)
		{

		}
	}
	private static void HandleCreateExpr(StringBuilder stringBuilder, Expression expressions)
	{
		switch (expressions.Num)
		{

		}
	}
	private static void HandleTimerExpr(StringBuilder stringBuilder, Expression expressions)
	{
		switch (expressions.Num)
		{

		}
	}
	private static void HandleGameExpr(StringBuilder stringBuilder, Expression expressions)
	{
		switch (expressions.Num)
		{

		}
	}
	private static void HandleSoundExpr(StringBuilder stringBuilder, Expression expressions)
	{
		switch (expressions.Num)
		{

		}
	}

	private static void HandleArithmeticExpr(StringBuilder stringBuilder, Expression expressions)
	{
		switch (expressions.Num)
		{

		}
	}

	private static void HandleSystemExpr(StringBuilder stringBuilder, Expression expressions)
	{
		switch (expressions.Num)
		{

		}
	}
	private static void HandleRuntimeObjectExpr(StringBuilder stringBuilder, Expression expressions, EventBase eventBase = null)
	{
		switch (expressions.Num)
		{

		}
	}
	private static void HandleUnimplemented(StringBuilder result, Expression expression, EventBase eventBase = null)
	{
		switch (expression.Num) { }
	}

	public static string ConvertExpression(ExpressionParameter expressions, EventBase eventBase = null)
	{
		//TODO: refactor this
		// TODO: use switch cases, maybe seperate every speecial object switch to its own function
		StringBuilder result = new();
		for (int i = 0; i < expressions.Items.Count; i++)
		{
			Expression expression = expressions.Items[i];
			switch ((OT)expression.ObjectType)
			{
				case OT.Player:
					HandlePlayerExpr(result, expression); break;

				case OT.Keyboard:
					HandleInputExpr(result, expression); break;

				case OT.Create:
					HandleCreateExpr(result, expression); break;

				case OT.Timer:
					HandleTimerExpr(result, expression); break;

				case OT.Game:
					HandleGameExpr(result, expression); break;

				case OT.Speaker:
					HandleSoundExpr(result, expression); break;

				case OT.System:
					HandleSystemExpr(result, expression); break;

				case (OT)0: // 0 is Arithmetic not a quickdrop (+ - * /)
					HandleArithmeticExpr(result, expression); break;

				default:
					if (expression.ObjectType > 0)
					{
						// Runtime Objects (Active, Text, Counter, etc.)
						HandleRuntimeObjectExpr(result, expression, eventBase);
					}
					else
					{
						HandleUnimplemented(result, expression);
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
