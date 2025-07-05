using System.Text;

public class EventProcessor
{
	private readonly Exporter _exporter;

	public EventProcessor(Exporter exporter)
	{
		_exporter = exporter;
	}

	public string BuildEventUpdateLoop(int frameIndex)
	{
		return "";
	}

	public string BuildEventFunctions(int frameIndex)
	{
		return "";
	}

	public string BuildEventIncludes(int frameIndex)
	{
		return "";
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
