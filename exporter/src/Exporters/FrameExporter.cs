using System.Text;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.Utils;

public class FrameExporter : BaseExporter
{
	private readonly ExpressionConverter _expressionConverter;
	private readonly EventProcessor _eventProcessor;

	public FrameExporter(Exporter exporter) : base(exporter)
	{
		_expressionConverter = new ExpressionConverter(exporter);
		_eventProcessor = new EventProcessor(exporter);
	}

	public override void Export()
	{
		var frameFactoryCppPath = Path.Combine(RuntimeBasePath.FullName, "source", "FrameFactory.template.cpp");

		// read templates
		var frameHeaderTemplate = File.ReadAllText(Path.Combine(RuntimeBasePath.FullName, "include", "Frame.template.h"));
		var frameCppTemplate = File.ReadAllText(Path.Combine(RuntimeBasePath.FullName, "source", "Frame.template.cpp"));
		var frameFactoryCppTemplate = File.ReadAllText(Path.Combine(RuntimeBasePath.FullName, "source", "FrameFactory.template.cpp"));

		string frameIncludes = "";
		string frameCases = "";

		for (int i = 0; i < GameData.Frames.Count; i++)
		{
			Logger.Log($"Exporting frame {i + 1}/{GameData.Frames.Count}...");
			_exporter.CurrentFrame = i;

			var frameHeader = frameHeaderTemplate.Replace("{{ FRAME_INDEX }}", i.ToString());
			var frameCpp = frameCppTemplate.Replace("{{ FRAME_INDEX }}", i.ToString());

			frameCpp = ProcessFrameTemplate(frameCpp, i);
			frameHeader = ProcessFrameHeader(frameHeader, i);

			// write frame files
			SaveFile(Path.Combine(OutputPath.FullName, "include", $"GeneratedFrame{i}.h"), frameHeader);
			SaveFile(Path.Combine(OutputPath.FullName, "source", $"GeneratedFrame{i}.cpp"), frameCpp);

			// add to factory
			frameIncludes += $"#include \"GeneratedFrame{i}.h\"\n";
			frameCases += $"        case {i}:\n            return std::make_unique<GeneratedFrame{i}>();\n";
		}

		// write factory implementation
		string frameFactoryCpp = frameFactoryCppTemplate.Replace("{{ FRAME_INCLUDES }}", frameIncludes);
		frameFactoryCpp = frameFactoryCpp.Replace("{{ FRAME_CASES }}", frameCases);

		SaveFile(Path.Combine(OutputPath.FullName, "source", "FrameFactory.cpp"), frameFactoryCpp);

		// delete the template files
		File.Delete(Path.Combine(OutputPath.FullName, "include", "Frame.template.h"));
		File.Delete(Path.Combine(OutputPath.FullName, "source", "Frame.template.cpp"));
		File.Delete(Path.Combine(OutputPath.FullName, "source", "FrameFactory.template.cpp"));
	}

	private string ProcessFrameTemplate(string frameCpp, int frameIndex)
	{
		var frame = GameData.Frames[frameIndex];

		frameCpp = frameCpp.Replace("{{ FRAME_INDEX }}", frameIndex.ToString());
		frameCpp = frameCpp.Replace("{{ FRAME_NAME }}", SanitizeString(frame.name));
		frameCpp = frameCpp.Replace("{{ FRAME_WIDTH }}", frame.width.ToString());
		frameCpp = frameCpp.Replace("{{ FRAME_HEIGHT }}", frame.height.ToString());
		frameCpp = frameCpp.Replace("{{ FRAME_BACKGROUND_COLOR }}", ColorToRGB(frame.background).ToString());

		frameCpp = frameCpp.Replace("{{ LAYER_INIT }}", BuildLayers(frame));
		frameCpp = frameCpp.Replace("{{ OBJECT_INSTANCES }}", BuildObjectInstances(frame));
		frameCpp = frameCpp.Replace("{{ OBJECT_SELECTORS_INIT }}", BuildObjectSelectorsInit(frame));
		frameCpp = frameCpp.Replace("{{ GROUP_ACTIVE }}", BuildGroupActive(frameIndex));

		_eventProcessor.PreProcessFrame(frameIndex);

		frameCpp = frameCpp.Replace("{{ EVENT_UPDATE_LOOP }}", _eventProcessor.BuildEventUpdateLoop(frameIndex));
		frameCpp = frameCpp.Replace("{{ EVENT_FUNCTIONS }}", _eventProcessor.BuildEventFunctions(frameIndex));

		return frameCpp;
	}

	private string ProcessFrameHeader(string frameHeader, int frameIndex)
	{
		var frame = GameData.Frames[frameIndex];

		frameHeader = frameHeader.Replace("{{ FRAME_INDEX }}", frameIndex.ToString());
		frameHeader = frameHeader.Replace("{{ OBJECT_SELECTORS }}", BuildObjectSelectors(frame));
		frameHeader = frameHeader.Replace("{{ EVENT_INCLUDES }}", _eventProcessor.BuildEventIncludes(frameIndex));
		frameHeader = frameHeader.Replace("{{ LOOP_INCLUDES }}", _eventProcessor.BuildLoopIncludes(frameIndex));
		frameHeader = frameHeader.Replace("{{ RUN_ONCE_CONDITION }}", _eventProcessor.BuildRunOnceCondition(frameIndex));
		frameHeader = frameHeader.Replace("{{ ONLY_ONE_ACTION_WHEN_LOOP_CONDITION }}", _eventProcessor.BuildOneActionLoop(frameIndex));

		return frameHeader;
	}

	private string BuildLayers(CTFAK.CCN.Chunks.Frame.Frame frame)
	{
		var layers = new StringBuilder();
		foreach (var layer in frame.layers.Items)
		{
			layers.AppendLine($"Layers.push_back(Layer(\"{SanitizeString(layer.Name)}\", {layer.XCoeff}, {layer.YCoeff}));");
		}
		return layers.ToString();
	}

	private string BuildObjectInstances(CTFAK.CCN.Chunks.Frame.Frame frame)
	{
		var objectInstances = new StringBuilder();
		foreach (var obj in frame.objects)
		{
			// skip instances not created on start
			if (obj.parentType != 0) continue;
			if (GameData.frameitems[(int)obj.objectInfo].properties is ObjectCommon common && common.Flags.GetFlag("DoNotCreateAtStart")) continue;

			objectInstances.Append($"ObjectInstances.push_back(factory.CreateInstance({obj.handle}, {obj.objectInfo}, {obj.x}, {obj.y}, {obj.layer}, {obj.instance})); // {SanitizeString(GameData.frameitems[(int)obj.objectInfo].name)}\n");
		}
		return objectInstances.ToString();
	}

	private string BuildObjectSelectors(CTFAK.CCN.Chunks.Frame.Frame frame)
	{
		var eventObjects = new StringBuilder();
		var uniqueHandles = new List<uint>();

		foreach (var obj in frame.objects)
		{
			if (!uniqueHandles.Contains(obj.objectInfo))
			{
				uniqueHandles.Add(obj.objectInfo);
				string objectName = GameData.frameitems[(int)obj.objectInfo].name;
				eventObjects.AppendLine($"std::shared_ptr<ObjectSelector> {SanitizeObjectName(objectName)}_{obj.objectInfo}_selector;");
			}
		}

		// qualifiers
		foreach (var qualifier in frame.events.QualifiersList)
		{
			if (!uniqueHandles.Contains((uint)qualifier.ObjectInfo))
			{
				uniqueHandles.Add((uint)qualifier.ObjectInfo);
				string objectName = Utilities.GetQualifierName(qualifier.ObjectInfo & 0x7FFF, qualifier.Type);
				eventObjects.AppendLine($"std::shared_ptr<ObjectSelector> {SanitizeObjectName(objectName)}_{qualifier.ObjectInfo}_selector;");
			}
		}

		return eventObjects.ToString();
	}

	private string BuildObjectSelectorsInit(CTFAK.CCN.Chunks.Frame.Frame frame)
	{
		var objectSelectorsInit = new StringBuilder();
		var uniqueHandles = new List<uint>();

		foreach (var obj in frame.objects)
		{
			if (!uniqueHandles.Contains(obj.objectInfo))
			{
				uniqueHandles.Add(obj.objectInfo);
				string objectName = GameData.frameitems[(int)obj.objectInfo].name;
				objectSelectorsInit.AppendLine($"{SanitizeObjectName(objectName)}_{obj.objectInfo}_selector = std::make_shared<ObjectSelector>(ObjectInstances, {obj.objectInfo}, false);");
			}
		}

		// qualifiers
		foreach (var qualifier in frame.events.QualifiersList)
		{
			if (!uniqueHandles.Contains((uint)qualifier.ObjectInfo))
			{
				uniqueHandles.Add((uint)qualifier.ObjectInfo);
				string objectName = Utilities.GetQualifierName(qualifier.ObjectInfo & 0x7FFF, qualifier.Type);
				objectSelectorsInit.AppendLine($"{SanitizeObjectName(objectName)}_{qualifier.ObjectInfo}_selector = std::make_shared<ObjectSelector>(ObjectInstances, {qualifier.ObjectInfo - 32768}, true);");
			}
		}

		return objectSelectorsInit.ToString();
	}

	private string BuildGroupActive(int frameIndex)
	{
		var groupActive = new StringBuilder();
		for (int j = 0; j < MfaData.Frames[frameIndex].Events.Items.Count; j++)
		{
			var evt = MfaData.Frames[frameIndex].Events.Items[j];
			if (evt.Conditions[0].ObjectType == -1 && evt.Conditions[0].Num == -10)
			{
				int groupId = (evt.Conditions[0].Items[0].Loader as Group).Id;
				bool isActiveOnStart = !(evt.Conditions[0].Items[0].Loader as Group).Flags.GetFlag("InactiveOnStart");
				groupActive.Append($"SetGroupActive({groupId}, {isActiveOnStart.ToString().ToLower()});\n");
			}
		}
		return groupActive.ToString();
	}
}
