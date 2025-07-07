using System.Reflection;
using System.Text;
using Avalonia.Animation.Easings;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class EventProcessor
{
	private readonly Exporter _exporter;

	public EventProcessor(Exporter exporter)
	{
		_exporter = exporter;
	}

	public string BuildEventUpdateLoop(int frameIndex)
	{
		var result = new StringBuilder();

		for (int j = 0; j < _exporter.MfaData.Frames[frameIndex].Events.Items.Count; j++)
		{
			var evt = _exporter.MfaData.Frames[frameIndex].Events.Items[j];
			string eventName = $"Event_{j}";

			for (int k = 0; k < evt.RestrictCpt; k++) //TODO: check if this is correct
			{
				result.Append("\t");
			}

			if (evt.Conditions[0].IsOfType(new GroupStartCondition())) //if this event is a group start, don't include it in the main event update loop
			{
				result.Append($"if (IsGroupActive({(evt.Conditions[0].Items[0].Loader as Group).Id})) {{\n");
				continue;
			}
			else if (evt.Conditions[0].IsOfType(new GroupEndCondition())) //if this event is a group end, don't include it in the main event update loop, just close the current group
			{
				result.Remove(result.Length - 1, 1); //Remove the last tab
				result.Append("}\n");
				continue;
			}

			//Check if there is an on loop condition, if yes, don't include it in the main event update loop
			foreach (var cond in evt.Conditions)
			{
				if (cond.ObjectType == -1 && cond.Num == -16)
				{
					continue;
				}
			}

			//only add event to normal event loop if it doesn't have a loop condition
			if (DoesEventHaveLoop(evt) == null) result.Append($"{eventName}();\n");
		}
		return result.ToString();
	}

	public string BuildEventFunctions(int frameIndex)
	{
		var result = new StringBuilder();

		for (int j = 0; j < _exporter.MfaData.Frames[frameIndex].Events.Items.Count; j++)
		{
			var evt = _exporter.MfaData.Frames[frameIndex].Events.Items[j];

			if (evt.Conditions[0].IsOfType(new GroupStartCondition()) || evt.Conditions[0].IsOfType(new GroupEndCondition())) continue; //if this event is a group start or end, don't include it in the event includes

			result.AppendLine($"void GeneratedFrame{frameIndex}::Event_{j}()");
			result.AppendLine("{");

			//Get all relevant object infos for this event
			//TODO: instead of reseting all selectors at the beginning of the event, reset them only if they are used in this event
			List<Tuple<int, string>> relevantObjectInfos = new List<Tuple<int, string>>();
			foreach (var cond in evt.Conditions) relevantObjectInfos.AddRange(GetRelevantObjectInfos(cond, frameIndex));
			foreach (var act in evt.Actions) relevantObjectInfos.AddRange(GetRelevantObjectInfos(act, frameIndex));
			relevantObjectInfos = relevantObjectInfos.Distinct().ToList();

			foreach (var obj in relevantObjectInfos)
			{
				result.AppendLine($"{StringUtils.SanitizeObjectName(obj.Item2)}_{obj.Item1}_selector->Reset();");
			}

			string nextLabel = $"event_{j}_end";
			foreach (var condition in evt.Conditions)
			{
				var acBaseTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(ConditionBase))).ToList();
				var acBaseType = acBaseTypes.FirstOrDefault(t =>
				{
					var instance = Activator.CreateInstance(t) as ConditionBase;
					return instance?.ObjectType == condition.ObjectType && instance?.Num == condition.Num;
				});

				if (acBaseType == null)
				{
					result.AppendLine($"//Condition ({condition.ObjectType}, {condition.Num}) not found");
					result.AppendLine($"goto {nextLabel};");
					continue;
				}

				Dictionary<string, string> parameters = new Dictionary<string, string>()
				{
					{ "eventIndex", j.ToString() },
					{ "frameIndex", frameIndex.ToString() }
				};

				var instance = Activator.CreateInstance(acBaseType) as ConditionBase;
				string ifStatement = (condition.OtherFlags & 1) == 0 ? "if (!" : "if (";
				result.AppendLine(instance?.Build(condition, parameters, ifStatement, nextLabel));
			}

			result.AppendLine($"event_{j}_actions:;");

			foreach (var action in evt.Actions)
			{
				var acBaseTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(ActionBase))).ToList();
				var acBaseType = acBaseTypes.FirstOrDefault(t =>
				{
					var instance = Activator.CreateInstance(t) as ActionBase;
					return instance?.ObjectType == action.ObjectType && instance?.Num == action.Num;
				});

				if (acBaseType == null)
				{
					result.AppendLine($"//Action ({action.ObjectType}, {action.Num}) not found");
					continue;
				}

				Dictionary<string, string> parameters = new Dictionary<string, string>()
				{
					{ "eventIndex", j.ToString() },
					{ "frameIndex", frameIndex.ToString() }
				};

				var instance = Activator.CreateInstance(acBaseType) as ActionBase;
				result.AppendLine(instance?.Build(action, parameters, "", ""));
			}

			result.AppendLine($"event_{j}_end:;");

			result.AppendLine("}");
			result.AppendLine("");
		}

		//Create any loop functions
		foreach (var loopName in GetAllLoopNames(frameIndex))
		{
			result.AppendLine($"void GeneratedFrame{frameIndex}::{loopName}_loop()");
			result.AppendLine("{");

			//go through all events and find events that should be called in this loop
			for (int j = 0; j < _exporter.MfaData.Frames[frameIndex].Events.Items.Count; j++)
			{
				var evt = _exporter.MfaData.Frames[frameIndex].Events.Items[j];

				foreach (var condition in evt.Conditions)
				{
					if (!condition.IsOfType(new LoopCondition())) continue;

					string loopNameSanitized = StringUtils.SanitizeObjectName((((condition.Items[0].Loader as ExpressionParameter).Items[0].Loader as StringExp).Value.ToString()));
					if (loopNameSanitized == loopName)
					{
						//TODO: Check if group is active?
						result.AppendLine($"\tEvent_{j}();");
					}
				}
			}

			result.AppendLine("}");
		}

		return result.ToString();
	}

	//returns a list of all OIs used in this event condition or action
	List<Tuple<int, string>> GetRelevantObjectInfos(EventBase eventBase, int frameIndex)
	{
		List<Tuple<int, string>> relevantObjectInfos = new List<Tuple<int, string>>();

		int objectInfo = -1;
		string objectName = "";

		if (eventBase.ObjectType > 0)
		{
			objectInfo = eventBase.ObjectInfo;
		}

		foreach (var expression in eventBase.Items)
		{
			if (expression.Loader is ExpressionParameter)
			{
				foreach (var exp in (expression.Loader as ExpressionParameter).Items)
				{
					if (exp.ObjectType > 0)
					{
						objectInfo = exp.ObjectInfo;
					}
				}
			}
			else if (expression.Loader is Position)
			{
				if ((expression.Loader as Position).ObjectInfoParent != 65535)
				{
					objectInfo = (int)(expression.Loader as Position).ObjectInfoParent;
				}
			}
			else if (expression.Loader is ParamObject)
			{
				objectInfo = (expression.Loader as ParamObject).ObjectInfo;
			}
		}

		int systemQualifier = 0;
		int objectType = 0;

		foreach (var evtObj in _exporter.MfaData.Frames[frameIndex].Events.Objects)
		{
			if (evtObj.Handle == objectInfo)
			{
				objectName = evtObj.Name;
				objectType = evtObj.ObjectType;
				systemQualifier = evtObj.SystemQualifier;

				//Find object name in ccn frame
				foreach (var ccnObj in _exporter.GameData.Frames[frameIndex].objects)
				{
					if (objectName == _exporter.GameData.frameitems[(int)ccnObj.objectInfo].name)
					{
						objectInfo = ccnObj.objectInfo;
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

		if (objectInfo == -1) return new List<Tuple<int, string>>();

		if (!relevantObjectInfos.Any(x => x.Item1 == objectInfo)) relevantObjectInfos.Add(new Tuple<int, string>(objectInfo, objectName));

		return relevantObjectInfos;
	}


	public string BuildEventIncludes(int frameIndex)
	{
		var result = new StringBuilder();

		for (int j = 0; j < _exporter.MfaData.Frames[frameIndex].Events.Items.Count; j++)
		{
			var evt = _exporter.MfaData.Frames[frameIndex].Events.Items[j];

			if (evt.Conditions[0].IsOfType(new GroupStartCondition()) || evt.Conditions[0].IsOfType(new GroupEndCondition())) continue; //if this event is a group start or end, don't include it in the event includes

			result.AppendLine($"void Event_{j}();");
		}

		return result.ToString();
	}

	public List<string> GetAllLoopNames(int frameIndex)
	{
		List<string> loopNames = new();

		foreach (var evt in _exporter.MfaData.Frames[frameIndex].Events.Items)
		{
			string? loopName = DoesEventHaveLoop(evt);
			if (loopName != null)
			{
				loopNames.Add(StringUtils.SanitizeObjectName(loopName));
			}
		}

		return loopNames.Distinct().ToList();
	}

	//returns the loop name if the event has a loop condition, otherwise returns null
	string? DoesEventHaveLoop(EventGroup evtGroup)
	{
		foreach (var condition in evtGroup.Conditions)
		{
			if (condition.IsOfType(new LoopCondition())) return ((condition.Items[0]?.Loader as ExpressionParameter)?.Items[0]?.Loader as StringExp)?.Value.ToString() ?? "";
		}

		return null;
	}

	public string BuildLoopIncludes(int frameIndex)
	{
		StringBuilder result = new();

		List<string> loopNames = GetAllLoopNames(frameIndex);

		foreach (var loopName in loopNames)
		{
			result.AppendLine($"bool loop_{loopName}_running = false;");
			result.AppendLine($"int loop_{loopName}_index = 0;");
			result.AppendLine($"void {loopName}_loop();");
		}

		return result.ToString();
	}

	public string BuildRunOnceCondition(int frameIndex)
	{
		return "";
	}

	public string BuildOneActionLoop(int frameIndex)
	{
		return "";
	}
}
