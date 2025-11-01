using System.Text;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.MFA.MFAObjectLoaders;
using CTFAK.Utils;

public class ObjectInfoExporter : BaseExporter
{
	public ObjectInfoExporter(Exporter exporter) : base(exporter) { }

	public override void Export()
	{
		var objectInfoListPath = Path.Combine(RuntimeBasePath.FullName, "source", "ObjectFactory.template.cpp");
		var objectInfoList = File.ReadAllText(objectInfoListPath);

		var objectInfosCases = new StringBuilder();
		var objectInfoFunctionDefinitions = new StringBuilder();
		var objectInfoFunctions = new StringBuilder();
		foreach (var objectInfo in GameData.frameitems.Values)
		{
			objectInfoFunctionDefinitions.Append(BuildObjectInfoFunctionDefinition(objectInfo));
			objectInfosCases.Append(BuildObjectInfoCase(objectInfo));
			objectInfoFunctions.Append(BuildObjectInfoFunction(objectInfo));
		}

		objectInfoList = objectInfoList.Replace("{{ OBJECT_INFO_FUNCTIONS_DEFINITIONS }}", objectInfoFunctionDefinitions.ToString());
		objectInfoList = objectInfoList.Replace("{{ OBJECT_INFO_FUNCTIONS }}", objectInfoFunctions.ToString());
		objectInfoList = objectInfoList.Replace("{{ OBJECT_INFO_CASES }}", objectInfosCases.ToString());

		SaveFile(Path.Combine(OutputPath.FullName, "source", "ObjectFactory.cpp"), objectInfoList);
		File.Delete(Path.Combine(OutputPath.FullName, "source", "ObjectFactory.template.cpp"));

		//get all includes from used extensions
		var extensionIncludes = new StringBuilder();
		var extensionFolderExporter = new ExtensionFolderExporter(_exporter);
		var extensionClassNames = extensionFolderExporter.GetAllExtensionClassNames();

		foreach (var extension in extensionClassNames)
		{
			extensionIncludes.Append($"#include \"{extension}.h\"\n");
		}

		var objectFactoryIncludePath = Path.Combine(OutputPath.FullName, "include", "ObjectFactory.template.h");
		var objectFactoryInclude = File.ReadAllText(objectFactoryIncludePath);
		objectFactoryInclude = objectFactoryInclude.Replace("{{ EXTENSION_INCLUDES }}", extensionIncludes.ToString());
		objectFactoryInclude = objectFactoryInclude.Replace("{{ OBJECT_INFO_FUNCTIONS_DEFINITIONS }}", objectInfoFunctionDefinitions.ToString());
		SaveFile(Path.Combine(OutputPath.FullName, "include", "ObjectFactory.h"), objectFactoryInclude);
		File.Delete(objectFactoryIncludePath);
	}

	private string BuildObjectInfoCase(ObjectInfo objectInfo)
	{
		var result = new StringBuilder();
		result.Append($"case {objectInfo.handle}:\n");
		result.Append($"return CreateInstance_{SanitizeObjectName(objectInfo.name)}_{objectInfo.handle}();\n");
		return result.ToString();
	}

	private string BuildObjectInfoFunctionDefinition(ObjectInfo objectInfo)
	{
		var result = new StringBuilder();
		result.AppendLine($"ObjectInstance* CreateInstance_{SanitizeObjectName(objectInfo.name)}_{objectInfo.handle}();");
		return result.ToString();
	}

	private string BuildObjectInfoFunction(ObjectInfo objectInfo)
	{
		var result = new StringBuilder();

		result.AppendLine($"ObjectInstance* ObjectFactory::CreateInstance_{SanitizeObjectName(objectInfo.name)}_{objectInfo.handle}() {{");

		string objectTypeClass = ExpressionConverter.GetObjectClassName(objectInfo.handle, convertToCCN: false);
		string additionalParameters = "";
		if (objectInfo.ObjectType >= 32)
		{
			objectTypeClass = "Extension";
			if (objectInfo.properties is ObjectCommon common)
			{
				ExtensionExporter extensionExporter = ExtensionExporterRegistry.GetExporter(common.Identifier);
				if (extensionExporter != null)
				{
					objectTypeClass = extensionExporter.CppClassName;
					if (common.ExtensionOffset > 0 && common.ExtensionData != null)
					{
						string extensionParameters = extensionExporter.ExportExtension(common.ExtensionData);
						additionalParameters = $", {extensionParameters}";
					}
				}
			}
		}

		result.AppendLine($"\tObjectInstance* instance = new {objectTypeClass}({objectInfo.handle}, {objectInfo.ObjectType}, \"{SanitizeString(objectInfo.name)}\"{additionalParameters});");

		result.AppendLine($"instance->RGBCoefficient = {ColorToArgb(objectInfo.rgbCoeff)};");
		result.AppendLine($"instance->BlendCoefficient = {objectInfo.blend};");
		result.AppendLine($"instance->Effect = {objectInfo.InkEffect};");
		result.AppendLine($"instance->EffectParameter = {objectInfo.InkEffectValue};");

		{
			if (objectInfo.properties is ObjectCommon common)
			{
				result.AppendLine($"instance->Qualifiers = {BuildQualifiers(common)};");
			}
		}


		if (objectInfo.ObjectType == 0)
		{
			result.AppendLine(BuildQuickBackdropProperties((Quickbackdrop)objectInfo.properties));
		}
		else if (objectInfo.ObjectType == 1)
		{
			result.AppendLine(BuildBackdropProperties((Backdrop)objectInfo.properties));
		}
		else if (objectInfo.ObjectType == 2)
		{
			var common = (ObjectCommon)objectInfo.properties;
			result.AppendLine($"((Active*)instance)->Visible = {common.NewFlags.GetFlag("VisibleAtStart").ToString().ToLower()};");
			result.AppendLine($"((Active*)instance)->FollowFrame = {(!common.Flags.GetFlag("ScrollingIndependant")).ToString().ToLower()};");
			result.AppendLine($"((Active*)instance)->AutomaticRotation = {common.NewFlags.GetFlag("AutomaticRotation").ToString().ToLower()};");
			result.AppendLine($"((Active*)instance)->FineDetection = {(!common.NewFlags.GetFlag("CollisionBox")).ToString().ToLower()};");
			result.AppendLine($"((Active*)instance)->Animations = {BuildAnimations(common)};");
			result.AppendLine($"((Active*)instance)->Values = {BuildAlterableValues(common)};");
			result.AppendLine($"((Active*)instance)->Strings = {BuildAlterableStrings(common)};");
			result.AppendLine($"((Active*)instance)->Flags = {BuildAlterableFlags(common)};");
			result.AppendLine($"((Active*)instance)->Movements = {BuildMovements(common)};");
		}
		else if (objectInfo.ObjectType == 3)
		{
			var common = (ObjectCommon)objectInfo.properties;
			result.AppendLine($"((StringObject*)instance)->Visible = {common.NewFlags.GetFlag("VisibleAtStart").ToString().ToLower()};");
			result.AppendLine(BuildParagraphs((ObjectCommon)objectInfo.properties));
		}
		else if (objectInfo.ObjectType == 5 || objectInfo.ObjectType == 6 || objectInfo.ObjectType == 7)
		{
			var common = (ObjectCommon)objectInfo.properties;
			result.AppendLine($"((CounterBase*)instance)->Visible = {common.NewFlags.GetFlag("VisibleAtStart").ToString().ToLower()};");
			result.AppendLine($"((CounterBase*)instance)->FollowFrame = {common.Preferences.GetFlag("ScrollingIndependant").ToString().ToLower()};");
			result.AppendLine(BuildCounter((ObjectCommon)objectInfo.properties));

			if (objectInfo.ObjectType == 7) // only counter has alterable values, strings, and flags
			{
				result.AppendLine($"((Counter*)instance)->Values = {BuildAlterableValues((ObjectCommon)objectInfo.properties)};");
				result.AppendLine($"((Counter*)instance)->Strings = {BuildAlterableStrings((ObjectCommon)objectInfo.properties)};");
				result.AppendLine($"((Counter*)instance)->Flags = {BuildAlterableFlags((ObjectCommon)objectInfo.properties)};");
			}
		}


		result.AppendLine($"\treturn instance;");
		result.AppendLine("}");

		return result.ToString();
	}

	private string BuildBackdropProperties(Backdrop backdrop)
	{
		var result = new StringBuilder();
		result.AppendLine($"((Backdrop*)instance)->Image = {backdrop.Image};");
		result.AppendLine($"((Backdrop*)instance)->ObstacleType = {(int)backdrop.ObstacleType};");
		result.AppendLine($"((Backdrop*)instance)->CollisionType = {(int)backdrop.CollisionType};");
		return result.ToString();
	}

	private string BuildQuickBackdropProperties(Quickbackdrop quickBackdrop)
	{
		var result = new StringBuilder();
		result.AppendLine($"((QuickBackdrop*)instance)->ObstacleType = {(int)quickBackdrop.ObstacleType};");
		result.AppendLine($"((QuickBackdrop*)instance)->CollisionType = {(int)quickBackdrop.CollisionType};");
		result.AppendLine($"((QuickBackdrop*)instance)->Width = {quickBackdrop.Width};");
		result.AppendLine($"((QuickBackdrop*)instance)->Height = {quickBackdrop.Height};");
		result.AppendLine($"((QuickBackdrop*)instance)->Shape = {BuildShape(quickBackdrop.Shape)};");
		return result.ToString();
	}

	private string BuildQualifiers(ObjectCommon common)
	{
		var result = new StringBuilder();

		result.Append("std::vector<short>{");
		foreach (var qualifier in common._qualifiers)
		{
			result.Append($"{qualifier}, ");
		}
		result.Append("}");

		return result.ToString();
	}

	private string BuildAlterableValues(ObjectCommon common)
	{
		return "AlterableValues(std::vector<int>{ " + (common.Values == null ? string.Empty : string.Join(",", common.Values.Items)) + " })";
	}

	private string BuildAlterableStrings(ObjectCommon common)
	{
		return "AlterableStrings(std::vector<std::string>{ " + (common.Strings == null ? string.Empty : string.Join(",", common.Strings.Items.Select(str => $"\"{SanitizeString(str)}\""))) + " })";
	}

	private string BuildAlterableFlags(ObjectCommon common)
	{
		var result = new StringBuilder();

		result.Append("AlterableFlags(std::vector<bool>{");
		for (int i = 0; i < 32; i++)
		{
			if (common.Values == null) break;
			result.Append($"{common.Values.Flags.GetFlag(i).ToString().ToLower()}");
			if (i != 31) result.Append(", ");
		}
		result.Append("})");
		return result.ToString();
	}

	private string BuildAnimations(ObjectCommon common)
	{
		var result = new StringBuilder();

		result.Append($"Animations(");
		result.Append("std::unordered_map<int, Sequence*>{");

		var sequences = new List<string>();
		foreach (var sequence in common.Animations.AnimationDict)
		{
			var sequenceStr = new StringBuilder();
			sequenceStr.Append($"std::pair<int, Sequence*>({sequence.Key}, ");
			sequenceStr.Append($"new Sequence(");
			sequenceStr.Append("std::vector<Direction*>{");

			var directions = new List<string>();
			foreach (var direction in sequence.Value.DirectionDict.Values)
			{
				int index = sequence.Value.DirectionDict.Values.ToList().IndexOf(direction);
					directions.Add(
						"new Direction(" + index + ", " + direction.MinSpeed + ", " +
						direction.MaxSpeed + ", " + (direction.Repeat == 0).ToString().ToLower() + ", " + direction.BackTo + ", " +
						"std::vector<unsigned int>{" + string.Join(",", direction.Frames) + "})"
					);
			}

			sequenceStr.Append(string.Join(", ", directions));
			sequenceStr.Append("}))");
			sequences.Add(sequenceStr.ToString());
		}

		result.Append(string.Join(", ", sequences));
		result.Append("})");

		return result.ToString();
	}

	private string BuildCounter(ObjectCommon common)
	{
		var result = new StringBuilder();

		if (common.Counters != null)
		{
			result.AppendLine($"((CounterBase*)instance)->Width = {common.Counters.Width};");
			result.AppendLine($"((CounterBase*)instance)->Height = {common.Counters.Height};");
			result.AppendLine($"((CounterBase*)instance)->Player = {common.Counters.Player - 1};");
			result.AppendLine($"((CounterBase*)instance)->DisplayType = {common.Counters.DisplayType};");
			result.AppendLine($"((CounterBase*)instance)->IntDigitPadding = {common.Counters.IntegerDigits.ToString().ToLower()};");
			result.AppendLine($"((CounterBase*)instance)->FloatWholePadding = {common.Counters.FloatDigits.ToString().ToLower()};");
			result.AppendLine($"((CounterBase*)instance)->FloatDecimalPadding = {common.Counters.UseDecimals.ToString().ToLower()};");
			result.AppendLine($"((CounterBase*)instance)->FloatPadding = {common.Counters.AddNulls.ToString().ToLower()};");
			result.AppendLine($"((CounterBase*)instance)->BarDirection = {common.Counters.Inverse.ToString().ToLower()};");
			result.AppendLine($"((CounterBase*)instance)->IntDigitCount = {common.Counters.IntegerDigits};");
			result.AppendLine($"((CounterBase*)instance)->FloatWholeCount = {common.Counters.FloatDigits};");
			result.AppendLine($"((CounterBase*)instance)->FloatDecimalCount = {common.Counters.Decimals};");
			result.AppendLine($"((CounterBase*)instance)->Font = {common.Counters.Font};");
			result.AppendLine($"((CounterBase*)instance)->oShape = {BuildShape(common.Counters.Shape)};");
			result.AppendLine($"((CounterBase*)instance)->Frames = std::vector<unsigned int>{{{string.Join(",", common.Counters.Frames)}}};");
		}

		if (common.Counter != null)
		{
			result.AppendLine($"((Counter*)instance)->DefaultValue = {common.Counter.Initial};");
			result.AppendLine($"((Counter*)instance)->MinValue = {common.Counter.Minimum};");
			result.AppendLine($"((Counter*)instance)->MaxValue = {common.Counter.Maximum};");
			result.AppendLine($"((Counter*)instance)->SetValue({common.Counter.Initial});");
		}

		return result.ToString();
	}

	private string BuildParagraphs(ObjectCommon common)
	{
		if (common.Text == null) return string.Empty;

		var result = new StringBuilder();

		result.AppendLine($"((StringObject*)instance)->Width = {common.Text.Width};");
		result.AppendLine($"((StringObject*)instance)->Height = {common.Text.Height};");
		result.Append("((StringObject*)instance)->Paragraphs = std::vector<Paragraph>{");
		foreach (var paragraph in common.Text.Items)
		{
			result.Append($"Paragraph({paragraph.FontHandle}, {ColorToRGB(paragraph.Color)}, \"{SanitizeString(paragraph.Value)}\")");
			if (paragraph != common.Text.Items.Last()) result.Append(", ");
		}
		result.AppendLine("};");

		return result.ToString();
	}

	private string BuildMovements(ObjectCommon common)
	{
		if (common.Movements == null) return string.Empty;

		var result = new StringBuilder();
		result.Append("Movements(std::unordered_map<int, Movement*>{");
		for (int i = 0; i < common.Movements.Items.Count; i++)
		{
			var movement = common.Movements.Items[i];

			string? movementClassName = null;
			switch (movement.Type)
			{
				case 0:
					movementClassName = "StaticMovement";
					break;
				case 1:
					movementClassName = "MouseMovement";
					break;
				case 3:
					movementClassName = "EightDirectionsMovement";
					break;
				case 5:
					movementClassName = "PathMovement";
					break;
			}

			if (movementClassName != null)
			{
				result.Append($"std::pair<int, Movement*>({i}, new {movementClassName}({movement.Player - 1}, {movement.MovingAtStart}, {movement.DirectionAtStart}");

				if (movement.Loader is EightDirections eightDirections)
				{
					result.Append($", {eightDirections.Speed}, {eightDirections.Acceleration}, {eightDirections.Deceleration}, {eightDirections.BounceFactor}, {eightDirections.Directions}");
				}
				else if (movement.Loader is Mouse mouse)
				{
					result.Append($", {mouse.X1}, {mouse.X2}, {mouse.Y1}, {mouse.Y2}");
				}
				else if (movement.Loader is MovementPath pathMovement)
				{
					result.Append($", {pathMovement.MinimumSpeed}, {pathMovement.MaximumSpeed}, {pathMovement.Loop}, {pathMovement.RepositionAtEnd}, {pathMovement.ReverseAtEnd}, ");
					result.Append($"std::vector<PathNode>{{{string.Join(",", pathMovement.Steps.Select(node => $"PathNode({node.Speed}, {node.Direction}, {node.DestinationX}, {node.DestinationY})"))}}}");
				}

				result.Append("))");

				if (i != common.Movements.Items.Count - 1)
					result.Append(", ");
			}

		}
		result.Append("})");
		return result.ToString();
	}

	private string BuildShape(Shape shape)
	{
		var result = new StringBuilder();

		result.Append("Shape(");
		result.Append($"{((shape.LineFlags & 1) == 0).ToString().ToLower()}, ");
		result.Append($"{((shape.LineFlags & 2) == 0).ToString().ToLower()}, ");
		result.Append($"{shape.BorderSize}, ");
		result.Append($"{ColorToArgb(shape.BorderColor)}, ");
		result.Append($"{shape.ShapeType}, ");
		result.Append($"{shape.FillType}, ");
		result.Append($"{ColorToArgb(shape.Color1)}, ");
		result.Append($"{ColorToArgb(shape.Color2)}, ");
		result.Append($"{((shape.GradFlags & 1) == 0).ToString().ToLower()}, ");
		result.Append($"{shape.Image}");
		result.Append(")");

		return result.ToString();
	}
}
