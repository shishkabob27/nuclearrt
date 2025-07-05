using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class TimerComparisonEveryCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -4;
	public override int Num { get; set; } = -8;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		return $"{ifStatement} (GameTimer.CheckEvent({parameters["eventIndex"]}, {((Time)eventBase.Items[0].Loader).Timer}, TimerEventType::Every))) goto {nextLabel};";
	}
}

public class TimerComparisonEveryCondition2 : TimerComparisonEveryCondition
{
	public override int Num { get; set; } = -4;
}

public class TimerComparisonEqualToCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -4;
	public override int Num { get; set; } = -7;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		return $"{ifStatement} (GameTimer.CheckEvent({parameters["eventIndex"]}, {((Time)eventBase.Items[0].Loader).Timer}, TimerEventType::Equals))) goto {nextLabel};";
	}
}

public class TimerComparisonLessThanCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -4;
	public override int Num { get; set; } = -2;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		return $"{ifStatement} (GameTimer.CheckEvent({parameters["eventIndex"]}, {((Time)eventBase.Items[0].Loader).Timer}, TimerEventType::LessThan))) goto {nextLabel};";
	}
}

public class TimerComparisonGreaterThanCondition : ConditionBase
{
	public override int ObjectType { get; set; } = -4;
	public override int Num { get; set; } = -1;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		return $"{ifStatement} (GameTimer.CheckEvent({parameters["eventIndex"]}, {((Time)eventBase.Items[0].Loader).Timer}, TimerEventType::GreaterThan))) goto {nextLabel};";
	}
}
