using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.Memory;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
public class PlaySample : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 0;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();
		result.AppendLine($"Application::Instance().GetBackend()->LoadSample({((Sample)eventBase.Items[0].Loader).Handle});");
		result.AppendLine($"Application::Instance().GetBackend()->PlaySample({((Sample)eventBase.Items[0].Loader).Handle}, -1, 1, NULL, false);");
		
		return result.ToString();
	}
}

public class PlaySampleChannel : ActionBase
{
	public override int ObjectType { get; set; } = -2;
	public override int Num { get; set; } = 11;
	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();
		result.AppendLine($"Application::Instance().GetBackend()->LoadSample({((Sample)eventBase.Items[0].Loader).Handle});");
		result.AppendLine($"Application::Instance().GetBackend()->PlaySample({((Sample)eventBase.Items[0].Loader).Handle}, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)}, 1, NULL, false);");

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
		result.AppendLine($"Application::Instance().GetBackend()->LoadSample({((Sample)eventBase.Items[0].Loader).Handle});");
		result.AppendLine($"Application::Instance().GetBackend()->PlaySample({((Sample)eventBase.Items[0].Loader).Handle}, -1, {ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)}, NULL, false);");

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
		return $"Application::Instance().GetBackend()->SetSampleVolume({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase)}, {((Sample)eventBase.Items[0].Loader).Handle}, true);";
	}
}
