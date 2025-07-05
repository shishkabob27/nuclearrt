using System.Reflection;
using System.Text;
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

			result.Append($"{eventName}();\n");
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

		return result.ToString();
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

	public string BuildLoopIncludes(int frameIndex)
	{
		return "";
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
