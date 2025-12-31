using System.Text;
using Avalonia.Logging;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using CTFAK.Utils;
public class PlaySample : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 0;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();
		result.AppendLine($"Application::Instance().GetBackend()->LoadSample({CheckType.Check(eventBase)});");
		result.AppendLine($"Application::Instance().GetBackend()->PlaySample({CheckType.Check(eventBase)}, -1, 1, NULL, false);");
		
		return result.ToString();
	}
}
public class CheckType
{
	public static string Check(EventBase eventBase)
	{
		string type;
		if (eventBase.Items[0].Loader is Sample)
			type = $"{((Sample)eventBase.Items[0].Loader).Handle}";
		else
			type = $"{ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)}"; // Supposedly the String expression that uses the name of the sample.
		return type;
	}
}
public class PlaySampleChannel : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 11;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();

		result.AppendLine($"Application::Instance().GetBackend()->LoadSample({CheckType.Check(eventBase)});");
		result.AppendLine($"Application::Instance().GetBackend()->PlaySample({CheckType.Check(eventBase)}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)}, 1, NULL, false);");

		return result.ToString();
	}
}
public class PlayAndLoopSample : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = -2;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();
		result.AppendLine($"Application::Instance().GetBackend()->LoadSample({CheckType.Check(eventBase)};");
		result.AppendLine($"Application::Instance().GetBackend()->PlaySample({CheckType.Check(eventBase)}, -1, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)}, NULL, false);");

		return result.ToString();
	}
}
public class StopAnySample : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 1;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		return "Application::Instance().GetBackend()->StopSample(-1, false);\n";
	}
}
public class PlayAndLoopSampleAtChannel : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 12;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();
		result.AppendLine($"Application::Instance().GetBackend()->LoadSample({((Sample)eventBase.Items[0].Loader).Handle});");
		result.AppendLine($"Application::Instance().GetBackend()->PlaySample({((Sample)eventBase.Items[0].Loader).Handle}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[2].Loader, eventBase)}, NULL, false);");

		return result.ToString();
	}
}
public class SetMainVolume : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 20;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		return $"Application::Instance().GetBackend()->SetSampleVolume({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)}, -1, false);";
	}
}
public class SetChannelVolume : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 17;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		return $"Application::Instance().GetBackend()->SetSampleVolume({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)}, true);";
	}
}
public class SetSampleVolume : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 21;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		return $"Application::Instance().GetBackend()->SetSampleVolume({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)}, {CheckType.Check(eventBase)}, true);";
	}
}
public class PauseSpecificSample : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 7;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		return $"Application::Instance().GetBackend()->PauseSample({CheckType.Check(eventBase)}, false, {(eventBase.Num == 7 ? "true" : "false")});";
	}

}
public class ResumeSpecificSample : PauseSpecificSample
{
	public override int Num { get; set; } = 8;
}
public class PauseChannel : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 13;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		return $"Application::Instance().GetBackend()->PauseSample({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)}, true, {(eventBase.Num == 13 ? "true" : "false")});";
	}
}
public class ResumeChannel : PauseChannel
{
	public override int Num { get; set; } = 14;
}
public class PauseAllSamples : ActionBase
{
	public override int Num { get; set; } = 24;
	public override int ObjectType { get; set; } = -2;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		return $"Application::Instance().GetBackend()->PauseSample(-1, false, {(eventBase.Num == 24 ? "true" : "false")});";
	}
}
public class ResumeAllSamples : PauseAllSamples
{
	public override int Num { get; set; } = 25;
}
