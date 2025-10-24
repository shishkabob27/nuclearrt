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
		result.Append($"return CreateObjectInfo_{objectInfo.handle}();\n");
		return result.ToString();
	}

	private string BuildObjectInfoFunctionDefinition(ObjectInfo objectInfo)
	{
		var result = new StringBuilder();
		result.AppendLine($"std::shared_ptr<ObjectInfo> CreateObjectInfo_{objectInfo.handle}();");
		return result.ToString();
	}

	private string BuildObjectInfoFunction(ObjectInfo objectInfo)
	{
		var result = new StringBuilder();

		result.AppendLine($"std::shared_ptr<ObjectInfo> ObjectFactory::CreateObjectInfo_{objectInfo.handle}() {{");

		result.Append($"\treturn std::make_shared<ObjectInfo>({objectInfo.handle}, ");
		result.Append($"{objectInfo.ObjectType}, ");
		result.Append($"std::string(\"{SanitizeString(objectInfo.name)}\"), ");
		result.Append($"{ColorToRGB(objectInfo.rgbCoeff)}, ");
		result.Append($"{objectInfo.InkEffect}, ");
		result.Append($"{objectInfo.blend}, ");
		result.Append($"{objectInfo.InkEffectValue}, ");
		result.Append($"std::make_shared<");

		if (objectInfo.properties is ObjectCommon common)
		{
			result.Append(BuildCommonProperties(common));
		}
		else if (objectInfo.properties is Backdrop backdrop)
		{
			result.Append(BuildBackdropProperties(backdrop));
		}
		else if (objectInfo.properties is Quickbackdrop quickBackdrop)
		{
			result.Append(BuildQuickBackdropProperties(quickBackdrop));
		}

		result.Append(")");
		result.AppendLine(");");
		result.AppendLine("}");

		return result.ToString();
	}

	private string BuildCommonProperties(ObjectCommon common)
	{
		var result = new StringBuilder();
		result.Append("CommonProperties>(");

		// Basic properties
		result.Append($"\"{SanitizeString(common.Identifier)}\", ");
		result.Append($"{common.NewFlags.GetFlag("VisibleAtStart").ToString().ToLower()}, ");
		result.Append($"{(!common.Flags.GetFlag("ScrollingIndependant")).ToString().ToLower()}, ");
		result.Append($"{(!common.NewFlags.GetFlag("CollisionBox")).ToString().ToLower()}, ");
		result.Append($"{common.NewFlags.GetFlag("AutomaticRotation").ToString().ToLower()}");

		result.Append(BuildQualifiers(common));
		result.Append(BuildAlterableValues(common));
		result.Append(BuildAlterableStrings(common));
		result.Append(BuildAlterableFlags(common));
		result.Append(BuildAnimations(common));
		result.Append(BuildValue(common));
		result.Append(BuildCounter(common));
		result.Append(BuildParagraphs(common));
		result.Append(BuildMovements(common));
		result.Append(BuildExtension(common));

		return result.ToString();
	}

	private string BuildBackdropProperties(Backdrop backdrop)
	{
		var result = new StringBuilder();
		result.Append("BackdropProperties>(");
		result.Append($"{(int)backdrop.ObstacleType}, ");
		result.Append($"{(int)backdrop.CollisionType}, ");
		result.Append($"{backdrop.Width}, ");
		result.Append($"{backdrop.Height}, ");
		result.Append($"{backdrop.Image}");
		return result.ToString();
	}

	private string BuildQuickBackdropProperties(Quickbackdrop quickBackdrop)
	{
		var result = new StringBuilder();
		result.Append("QuickBackdropProperties>(");
		result.Append($"{(int)quickBackdrop.ObstacleType}, ");
		result.Append($"{(int)quickBackdrop.CollisionType}, ");
		result.Append($"{quickBackdrop.Width}, ");
		result.Append($"{quickBackdrop.Height}, ");
		result.Append(BuildShape(quickBackdrop.Shape));
		return result.ToString();
	}

	private string BuildQualifiers(ObjectCommon common)
	{
		var result = new StringBuilder();

		result.Append(", std::vector<short>{");
		foreach (var qualifier in common._qualifiers)
		{
			result.Append($"{qualifier}, ");
		}
		result.Append("}");

		return result.ToString();
	}

	private string BuildAlterableValues(ObjectCommon common)
	{
		if (common.Flags.GetFlag("Values"))
		{
			var result = new StringBuilder();

			result.Append(", std::make_shared<AlterableValues>(");
			result.Append("std::vector<int>{");
			if (common.Values != null)
			{
				foreach (var value in common.Values.Items)
				{
					result.Append($"{value}, ");
				}
			}
			result.Append("})");

			return result.ToString();
		}
		else
		{
			return ", nullptr";
		}
	}

	private string BuildAlterableStrings(ObjectCommon common)
	{
		if (common.Strings != null)
		{
			var result = new StringBuilder();

			result.Append(", std::make_shared<AlterableStrings>(");
			result.Append("std::vector<std::string>{");
			foreach (var str in common.Strings.Items)
			{
				result.Append($"\"{SanitizeString(str)}\", ");
			}
			result.Append("})");

			return result.ToString();
		}
		else
		{
			return ", nullptr";
		}
	}

	private string BuildAlterableFlags(ObjectCommon common)
	{
		var result = new StringBuilder();

		result.Append(", std::make_shared<AlterableFlags>(");
		result.Append("std::vector<bool>{");
		if (common.Values != null)
		{
			for (int i = 0; i < 32; i++)
			{
				result.Append($"{common.Values.Flags.GetFlag(i).ToString().ToLower()}, ");
			}
		}
		result.Append("})");

		return result.ToString();
	}

	private string BuildAnimations(ObjectCommon common)
	{
		if (common.AnimationsOffset > 0)
		{
			var result = new StringBuilder();

			result.Append(", ");
			result.Append($"std::make_shared<Animations>(");
			result.Append($"std::unordered_map<int, std::shared_ptr<Sequence>>{{");

			var sequences = new List<string>();
			foreach (var sequence in common.Animations.AnimationDict)
			{
				var sequenceStr = new StringBuilder();
				sequenceStr.Append($"std::pair<int, std::shared_ptr<Sequence>>({sequence.Key}, ");
				sequenceStr.Append($"std::make_shared<Sequence>(");
				sequenceStr.Append($"std::vector<std::shared_ptr<Direction>>{{");

				var directions = new List<string>();
				foreach (var direction in sequence.Value.DirectionDict.Values)
				{
					int index = sequence.Value.DirectionDict.Values.ToList().IndexOf(direction);
					directions.Add($"std::make_shared<Direction>({index}, {direction.MinSpeed}, " +
							$"{direction.MaxSpeed}, {(direction.Repeat == 0).ToString().ToLower()}, {direction.BackTo}, " +
							$"std::vector<unsigned int>{{{string.Join(",", direction.Frames)}}})"
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
		else
		{
			return ", nullptr";
		}
	}

	private string BuildValue(ObjectCommon common)
	{
		if (common.Counter != null)
		{
			return $", std::make_shared<Value>({common.Counter.Initial}, {common.Counter.Minimum}, {common.Counter.Maximum})";
		}
		else
		{
			return ", nullptr";
		}
	}

	private string BuildCounter(ObjectCommon common)
	{
		if (common.Counters != null)
		{
			var result = new StringBuilder();

			result.Append(", ");
			result.Append($"std::make_shared<Counter>");
			result.Append($"({common.Counters.Width}, {common.Counters.Height}, {common.Counters.Player - 1}, {common.Counters.DisplayType}, {common.Counters.IntegerDigits.ToString().ToLower()}, {common.Counters.FloatDigits.ToString().ToLower()}, {common.Counters.Decimals.ToString().ToLower()}, {common.Counters.FormatFloat.ToString().ToLower()}, {common.Counters.AddNulls.ToString().ToLower()}, {common.Counters.IntegerDigits}, {common.Counters.FloatDigits}, {common.Counters.FloatDigits}, {common.Counters.Font}, ");

			result.Append("std::vector<unsigned int>{");
			foreach (var frame in common.Counters.Frames)
			{
				result.Append($"{frame}, ");
			}
			result.Append("}, ");


			result.Append("std::make_shared<Shape>(");
			result.Append($"{((common.Counters.Shape.LineFlags & 1) == 0).ToString().ToLower()}, {((common.Counters.Shape.LineFlags & 2) == 0).ToString().ToLower()}, {common.Counters.Shape.BorderSize}, {ColorToArgb(common.Counters.Shape.BorderColor)}, {common.Counters.Shape.ShapeType}, {common.Counters.Shape.FillType}, {ColorToArgb(common.Counters.Shape.Color1)}, {ColorToArgb(common.Counters.Shape.Color2)}, {((common.Counters.Shape.GradFlags & 1) == 0).ToString().ToLower()}, {common.Counters.Shape.Image})");
			result.Append(")");

			return result.ToString();
		}
		else
		{
			return ", nullptr";
		}
	}

	private string BuildParagraphs(ObjectCommon common)
	{
		if (common.Text != null)
		{
			var result = new StringBuilder();

			result.Append(", std::make_shared<ObjectParagraphs>(");
			result.Append($"{common.Text.Width}, ");
			result.Append($"{common.Text.Height}, ");
			result.Append("std::vector<std::shared_ptr<Paragraph>>{");
			foreach (var paragraph in common.Text.Items)
			{
				result.Append($"std::make_shared<Paragraph>(");
				result.Append($"{paragraph.FontHandle}, ");
				result.Append($"{ColorToRGB(paragraph.Color)}, ");
				result.Append($"\"{SanitizeString(paragraph.Value)}\"");
				result.Append(")");
				if (paragraph != common.Text.Items.Last())
				{
					result.Append(", ");
				}
			}
			result.Append("}");
			result.Append(")");

			return result.ToString();
		}
		else
		{
			return ", nullptr";
		}
	}

	private string BuildMovements(ObjectCommon common)
	{
		if (common.Movements != null)
		{
			var result = new StringBuilder();
			result.Append(", std::make_shared<Movements>(");
			result.Append("std::vector<std::shared_ptr<Movement>>{");
			for (int i = 0; i < common.Movements.Items.Count; i++)
			{
				var movement = common.Movements.Items[i];

				string movementClassName = null;
				switch (movement.Type)
				{
					case 1:
						movementClassName = "MouseMovement";
						break;
					case 3:
						movementClassName = "EightDirectionsMovement";
						break;
				}

				if (movementClassName != null)
				{
					result.Append($"std::make_shared<{movementClassName}>({movement.Player - 1}, {movement.MovingAtStart}, {movement.DirectionAtStart}, ");

					if (movement.Loader is EightDirections eightDirections)
					{
						result.Append($"{eightDirections.Speed}, {eightDirections.Acceleration}, {eightDirections.Deceleration}, {eightDirections.BounceFactor}, {eightDirections.Directions}");
					}
					else if (movement.Loader is Mouse mouse)
					{
						result.Append($"{mouse.X1}, {mouse.X2}, {mouse.Y1}, {mouse.Y2}");
					}

					result.Append(")");
				}
				else
				{
					result.Append("nullptr");
				}

				if (i != common.Movements.Items.Count - 1)
					result.Append(", ");
			}
			result.Append("})");
			return result.ToString();
		}
		else
		{
			return ", nullptr";
		}
	}

	private string BuildShape(Shape shape)
	{
		var result = new StringBuilder();

		result.Append("std::make_shared<Shape>(");
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

	private string BuildExtension(ObjectCommon common)
	{
		if (common.ExtensionOffset > 0 && common.ExtensionData != null)
		{
			string extensionName = common.Identifier;

			ExtensionExporter extension = ExtensionExporterRegistry.GetExporter(extensionName);
			if (extension == null)
			{
				Logger.Log($"Extension {extensionName} not found");
				return ", nullptr";
			}

			return $", {extension.ExportExtension(common.ExtensionData)}";
		}
		else
		{
			return ", nullptr";
		}
	}
}
