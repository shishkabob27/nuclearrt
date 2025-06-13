using System.Text;
using System.Drawing;
using CTFAK.FileReaders;
using CTFAK.CCN;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CTFAK.CCN.Chunks;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Expressions;
using CTFAK.Utils;
using CTFAK.MFA;
using CTFAK.EXE;

public class Exporter
{
	private readonly IFileReader _ccnReader;
	private readonly IFileReader _mfaReader;

	private readonly DirectoryInfo _runtimeBasePath;
	private readonly DirectoryInfo _outputPath;

	public GameData GameData => _ccnReader.getGameData();
	public MFAData MfaData => (_mfaReader as MFAFileReader).mfa;

	public DirectoryInfo RuntimeBasePath => _runtimeBasePath;
	public DirectoryInfo OutputPath => _outputPath;

	public int CurrentFrame { get; set; } = -1;

	public Exporter(IFileReader ccnReader, IFileReader mfaReader, DirectoryInfo runtimeBasePath, DirectoryInfo outputPath)
	{
		_ccnReader = ccnReader;
		_mfaReader = mfaReader;
		_runtimeBasePath = runtimeBasePath;
		_outputPath = outputPath;
	}

	public void Export()
	{
		// Copy contents of the C++ runtime base path to the output path
		CopyFilesRecursively(RuntimeBasePath.FullName, OutputPath.FullName);

		ExportProjectFiles();
		ExportAppData();
		ExportObjectInfoList();
		ExportImageBank();
		ExportFontBank();
		ExportFrames();
	}

	void ExportProjectFiles()
	{
		var cmakelistsPath = Path.Combine(RuntimeBasePath.FullName, "CMakeLists.txt");

		var cmakelists = File.ReadAllText(cmakelistsPath);
		cmakelists = cmakelists.Replace("nuclearrt-runtime", SanitizeObjectName(GameData.name));
		SaveFile(Path.Combine(OutputPath.FullName, "CMakeLists.txt"), cmakelists);
	}

	void ExportAppData()
	{
		var appDataTemplatePath = Path.Combine(RuntimeBasePath.FullName, "source", "AppData.template.cpp");
		var appData = File.ReadAllText(appDataTemplatePath);
		appData = appData.Replace("{{ app_name }}", SanitizeString(GameData.name));
		appData = appData.Replace("{{ about_box }}", SanitizeString(GameData.aboutText));
		appData = appData.Replace("{{ window_width }}", GameData.header.WindowWidth.ToString());
		appData = appData.Replace("{{ window_height }}", GameData.header.WindowHeight.ToString());
		appData = appData.Replace("{{ target_fps }}", GameData.header.FrameRate.ToString());
		appData = appData.Replace("{{ border_color }}", ColorToRGB(GameData.header.BorderColor).ToString());
		appData = appData.Replace("{{ fit_inside }}", GameData.header.Flags.GetFlag("FitInside") ? "true" : "false");
		appData = appData.Replace("{{ resize_display }}", GameData.header.Flags.GetFlag("ResizeDisplay") ? "true" : "false");
		appData = appData.Replace("{{ dont_center_frame }}", GameData.header.Flags.GetFlag("DontCenterFrame") ? "true" : "false");

		// Global Values
		var globalValues = new StringBuilder();
		globalValues.Append("{ ");
		foreach (var value in GameData.globalValues.Items)
		{
			globalValues.Append(value);
			if (value != GameData.globalValues.Items.Last())
			{
				globalValues.Append(", ");
			}
		}
		globalValues.Append(" }");
		appData = appData.Replace("{{ global_values }}", globalValues.ToString());

		// Global Strings
		var globalStrings = new StringBuilder();
		globalStrings.Append("{ ");
		foreach (var str in GameData.globalStrings.Items)
		{
			globalStrings.Append($"\"{SanitizeString(str)}\"");
			if (str != GameData.globalStrings.Items.Last())
			{
				globalStrings.Append(", ");
			}
		}
		globalStrings.Append(" }");
		appData = appData.Replace("{{ global_strings }}", globalStrings.ToString());

		//Controls
		var controlsTypes = new StringBuilder();
		controlsTypes.Append("{ ");
		foreach (var control in GameData.header.Controls.Items)
		{
			controlsTypes.Append($"{control.ControlType}, ");
		}
		controlsTypes.Append("}");

		var controlsKeys = new StringBuilder();
		controlsKeys.Append("{ ");
		foreach (var control in GameData.header.Controls.Items)
		{
			controlsKeys.Append($"{{ ");
			controlsKeys.Append($"{control.Keys.Up}, ");
			controlsKeys.Append($"{control.Keys.Down}, ");
			controlsKeys.Append($"{control.Keys.Left}, ");
			controlsKeys.Append($"{control.Keys.Right}, ");
			controlsKeys.Append($"{control.Keys.Button1}, ");
			controlsKeys.Append($"{control.Keys.Button2}, ");
			controlsKeys.Append($"{control.Keys.Button3}, ");
			controlsKeys.Append($"{control.Keys.Button4}");
			controlsKeys.Append("}, ");
		}
		controlsKeys.Append("}");

		appData = appData.Replace("{{ control_types }}", controlsTypes.ToString());
		appData = appData.Replace("{{ control_keys }}", controlsKeys.ToString());

		//Scores
		var scores = new StringBuilder();
		scores.Append("{ ");
		for (int i = 0; i < 4; i++)
		{
			scores.Append($"{GameData.header.InitialScore}, ");
		}
		scores.Append("}");

		appData = appData.Replace("{{ player_scores }}", scores.ToString());

		//Lives
		var lives = new StringBuilder();
		lives.Append("{ ");
		for (int i = 0; i < 4; i++)
		{
			lives.Append($"{GameData.header.InitialLives}, ");
		}
		lives.Append("}");

		appData = appData.Replace("{{ player_lives }}", lives.ToString());

		SaveFile(Path.Combine(OutputPath.FullName, "source", "AppData.cpp"), appData);
		File.Delete(Path.Combine(OutputPath.FullName, "source", "AppData.template.cpp"));
	}

	void ExportObjectInfoList()
	{
		var objectInfoListPath = Path.Combine(RuntimeBasePath.FullName, "source", "ObjectFactory.template.cpp");
		var objectInfoList = File.ReadAllText(objectInfoListPath);

		var objectInfos = new StringBuilder();
		foreach (var objectInfo in GameData.frameitems.Values)
		{
			objectInfos.Append($"case {objectInfo.handle}:\n");
			objectInfos.Append($"return std::make_shared<ObjectInfo>({objectInfo.handle}, ");
			objectInfos.Append($"{objectInfo.ObjectType}, ");
			objectInfos.Append($"std::string(\"{SanitizeString(objectInfo.name)}\"), ");
			objectInfos.Append($"{ColorToRGB(objectInfo.rgbCoeff)}, ");
			objectInfos.Append($"{objectInfo.InkEffect}, ");
			objectInfos.Append($"{objectInfo.blend}, ");
			objectInfos.Append($"{objectInfo.InkEffectValue}, ");
			objectInfos.Append($"std::make_shared<");
			if (objectInfo.properties is ObjectCommon)
			{
				ObjectCommon common = (ObjectCommon)objectInfo.properties;

				objectInfos.Append("CommonProperties>(");

				objectInfos.Append($"\"{SanitizeString(common.Identifier)}\", ");
				objectInfos.Append($"{common.NewFlags.GetFlag("VisibleAtStart").ToString().ToLower()}, ");
				objectInfos.Append($"{(!common.Flags.GetFlag("ScrollingIndependant")).ToString().ToLower()}, ");
				objectInfos.Append($"{(!common.NewFlags.GetFlag("CollisionBox")).ToString().ToLower()}");

				//Qualifiers
				objectInfos.Append(", std::vector<short>{");
				foreach (var qualifier in common._qualifiers)
				{
					objectInfos.Append($"{qualifier}, ");
				}
				objectInfos.Append("}");

				//Alterable Values
				if (common.Flags.GetFlag("Values"))
				{
					objectInfos.Append(", std::make_shared<AlterableValues>(");
					objectInfos.Append("std::vector<float>{");
					if (common.Values != null)
					{
						foreach (var value in common.Values.Items)
						{
							objectInfos.Append($"{value}, ");
						}
					}
					objectInfos.Append("})");
				}
				else
				{
					objectInfos.Append(", nullptr");
				}

				//Alterable Strings
				if (common.Strings != null)
				{
					objectInfos.Append(", std::make_shared<AlterableStrings>(");
					objectInfos.Append("std::vector<std::string>{");
					foreach (var str in common.Strings.Items)
					{
						objectInfos.Append($"\"{SanitizeString(str)}\", ");
					}
					objectInfos.Append("})");
				}
				else
				{
					objectInfos.Append(", nullptr");
				}

				//Alterable Flags
				if (common.Values != null)
				{
					objectInfos.Append(", std::make_shared<AlterableFlags>(");
					objectInfos.Append("std::vector<bool>{");
					objectInfos.Append("0");
					//TODO: Add flags
					//foreach (var flag in common.ObjectAlterableValues.AlterableFlags.Keys)
					//{
					//	objectInfos.Append($"{common.ObjectAlterableValues.AlterableFlags.GetFlag(flag).ToString().ToLower()}, ");
					//}
					objectInfos.Append("})");
				}
				else
				{
					objectInfos.Append(", nullptr");
				}

				//Animation
				if (common.AnimationsOffset > 0)
				{
					objectInfos.Append(", ");
					objectInfos.Append($"std::make_shared<Animations>(");
					objectInfos.Append($"std::unordered_map<int, std::shared_ptr<Sequence>>{{");

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

					objectInfos.Append(string.Join(", ", sequences));
					objectInfos.Append("})");
				}
				else
				{
					objectInfos.Append(", nullptr");
				}

				//Value
				if (common.Counter != null)
				{
					objectInfos.Append(", ");
					objectInfos.Append($"std::make_shared<Value>({common.Counter.Initial}, {common.Counter.Minimum}, {common.Counter.Maximum})");
				}
				else
				{
					objectInfos.Append(", nullptr");
				}

				//Counter
				if (common.Counters != null)
				{
					objectInfos.Append(", ");
					objectInfos.Append($"std::make_shared<Counter>");
					objectInfos.Append($"({common.Counters.Width}, {common.Counters.Height}, {common.Counters.Player - 1}, {common.Counters.DisplayType}, {common.Counters.IntegerDigits.ToString().ToLower()}, {common.Counters.FloatDigits.ToString().ToLower()}, {common.Counters.Decimals.ToString().ToLower()}, {common.Counters.FormatFloat.ToString().ToLower()}, {common.Counters.AddNulls.ToString().ToLower()}, {common.Counters.IntegerDigits}, {common.Counters.FloatDigits}, {common.Counters.FloatDigits}, {common.Counters.Font}, ");

					objectInfos.Append("std::vector<unsigned int>{");
					foreach (var frame in common.Counters.Frames)
					{
						objectInfos.Append($"{frame}, ");
					}
					objectInfos.Append("}, ");


					objectInfos.Append("std::make_shared<Shape>(");
					objectInfos.Append($"{((common.Counters.Shape.LineFlags & 1) == 0).ToString().ToLower()}, {((common.Counters.Shape.LineFlags & 2) == 0).ToString().ToLower()}, {common.Counters.Shape.BorderSize}, {ColorToArgb(common.Counters.Shape.BorderColor)}, {common.Counters.Shape.ShapeType}, {common.Counters.Shape.FillType}, {ColorToArgb(common.Counters.Shape.Color1)}, {ColorToArgb(common.Counters.Shape.Color2)}, {((common.Counters.Shape.GradFlags & 1) == 0).ToString().ToLower()}, {common.Counters.Shape.Image})");
					objectInfos.Append(")");
				}
				else
				{
					objectInfos.Append(", nullptr");
				}

				//Paragraphs
				if (common.Text != null)
				{
					objectInfos.Append(", std::make_shared<ObjectParagraphs>(");
					objectInfos.Append($"{common.Text.Width}, ");
					objectInfos.Append($"{common.Text.Height}, ");
					objectInfos.Append("std::vector<std::shared_ptr<Paragraph>>{");
					foreach (var paragraph in common.Text.Items)
					{
						objectInfos.Append($"std::make_shared<Paragraph>(");
						objectInfos.Append($"{paragraph.FontHandle}, ");
						objectInfos.Append($"{ColorToRGB(paragraph.Color)}, ");
						objectInfos.Append($"\"{SanitizeString(paragraph.Value)}\"");
						objectInfos.Append(")");
						if (paragraph != common.Text.Items.Last())
						{
							objectInfos.Append(", ");
						}
					}
					objectInfos.Append("}");
					objectInfos.Append(")");
				}
				else
				{
					objectInfos.Append(", nullptr");
				}
			}
			else if (objectInfo.properties is Backdrop)
			{
				Backdrop backdrop = (Backdrop)objectInfo.properties;

				objectInfos.Append("BackdropProperties>(");
				objectInfos.Append($"{(int)backdrop.ObstacleType}, ");
				objectInfos.Append($"{(int)backdrop.CollisionType}, ");
				objectInfos.Append($"{backdrop.Width}, ");
				objectInfos.Append($"{backdrop.Height}, ");
				objectInfos.Append($"{backdrop.Image}");
			}
			else if (objectInfo.properties is Quickbackdrop)
			{
				Quickbackdrop quickBackdrop = (Quickbackdrop)objectInfo.properties;

				objectInfos.Append("QuickBackdropProperties>(");
				objectInfos.Append($"{(int)quickBackdrop.ObstacleType}, ");
				objectInfos.Append($"{(int)quickBackdrop.CollisionType}, ");
				objectInfos.Append($"{quickBackdrop.Width}, ");
				objectInfos.Append($"{quickBackdrop.Height}, ");

				//shape
				objectInfos.Append("std::make_shared<Shape>(");
				objectInfos.Append($"{((quickBackdrop.Shape.LineFlags & 1) == 0).ToString().ToLower()}, ");
				objectInfos.Append($"{((quickBackdrop.Shape.LineFlags & 2) == 0).ToString().ToLower()}, ");
				objectInfos.Append($"{quickBackdrop.Shape.BorderSize}, ");
				objectInfos.Append($"{ColorToArgb(quickBackdrop.Shape.BorderColor)}, ");
				objectInfos.Append($"{quickBackdrop.Shape.ShapeType}, ");
				objectInfos.Append($"{quickBackdrop.Shape.FillType}, ");
				objectInfos.Append($"{ColorToArgb(quickBackdrop.Shape.Color1)}, ");
				objectInfos.Append($"{ColorToArgb(quickBackdrop.Shape.Color2)}, ");
				objectInfos.Append($"{((quickBackdrop.Shape.GradFlags & 1) == 0).ToString().ToLower()}, ");
				objectInfos.Append($"{quickBackdrop.Shape.Image}");
				objectInfos.Append(")");
			}

			objectInfos.Append(")");
			objectInfos.Append(");\n");
			objectInfos.Append("break;\n");
		}

		objectInfoList = objectInfoList.Replace("{{ OBJECT_INFO_CASES }}", objectInfos.ToString());

		SaveFile(Path.Combine(OutputPath.FullName, "source", "ObjectFactory.cpp"), objectInfoList);
		File.Delete(Path.Combine(OutputPath.FullName, "source", "ObjectFactory.template.cpp"));
	}

	void ExportImageBank()
	{
		var imageBankPath = Path.Combine(RuntimeBasePath.FullName, "source", "ImageBank.template.cpp");
		var imageBank = File.ReadAllText(imageBankPath);

		var imageBankData = new StringBuilder();
		foreach (var image in GameData.Images.Items.Values)
		{
			imageBankData.Append($"Images[{image.Handle}] = std::make_shared<ImageInfo>({image.Handle}, {image.Width}, {image.Height}, {image.HotspotX}, {image.HotspotY}, {image.ActionX}, {image.ActionY}, {ColorToRGB(image.Transparent)});\n");
		}

		imageBank = imageBank.Replace("{{ IMAGES }}", imageBankData.ToString());

		SaveFile(Path.Combine(OutputPath.FullName, "source", "ImageBank.cpp"), imageBank.ToString());
		File.Delete(Path.Combine(OutputPath.FullName, "source", "ImageBank.template.cpp"));
	}

	void ExportFontBank()
	{
		var fontBankPath = Path.Combine(RuntimeBasePath.FullName, "source", "FontBank.template.cpp");
		var fontBank = File.ReadAllText(fontBankPath);

		var fontBankData = new StringBuilder();
		foreach (var font in GameData.Fonts.Items)
		{
			fontBankData.Append($"Fonts[{font.Handle}] = std::make_shared<FontInfo>({font.Handle}, \"{SanitizeString(font.Value.FaceName.Replace("\0", ""))}\", {font.Value.Width}, {font.Value.Height}, {font.Value.Escapement}, {font.Value.Orientation}, {font.Value.Weight}, {(font.Value.Italic == 1 ? true : false).ToString().ToLower()}, {(font.Value.Underline == 1 ? true : false).ToString().ToLower()}, {(font.Value.StrikeOut == 1 ? true : false).ToString().ToLower()});\n");
		}

		fontBank = fontBank.Replace("{{ FONTS }}", fontBankData.ToString());

		SaveFile(Path.Combine(OutputPath.FullName, "source", "FontBank.cpp"), fontBank.ToString());
		File.Delete(Path.Combine(OutputPath.FullName, "source", "FontBank.template.cpp"));
	}

	void ExportFrames()
	{
		// Implementation for C++ Frame export
		var frameFactoryCppPath = Path.Combine(RuntimeBasePath.FullName, "source", "FrameFactory.template.cpp");

		// Read templates
		var frameHeaderTemplate = File.ReadAllText(Path.Combine(RuntimeBasePath.FullName, "include", "Frame.template.h"));
		var frameCppTemplate = File.ReadAllText(Path.Combine(RuntimeBasePath.FullName, "source", "Frame.template.cpp"));
		var frameFactoryCppTemplate = File.ReadAllText(Path.Combine(RuntimeBasePath.FullName, "source", "FrameFactory.template.cpp"));

		string frameIncludes = "";
		string frameCases = "";

		for (int i = 0; i < GameData.Frames.Count; i++)
		{
			Logger.Log($"Exporting frame {i + 1}/{GameData.Frames.Count}...");
			CurrentFrame = i;

			var frameHeader = frameHeaderTemplate.Replace("{{ FRAME_INDEX }}", i.ToString());
			var frameCpp = frameCppTemplate.Replace("{{ FRAME_INDEX }}", i.ToString());

			//Initialization
			frameCpp = frameCpp.Replace("{{ FRAME_INDEX }}", i.ToString());
			frameCpp = frameCpp.Replace("{{ FRAME_NAME }}", SanitizeString(GameData.Frames[i].name));

			frameCpp = frameCpp.Replace("{{ FRAME_WIDTH }}", GameData.Frames[i].width.ToString());
			frameCpp = frameCpp.Replace("{{ FRAME_HEIGHT }}", GameData.Frames[i].height.ToString());
			frameCpp = frameCpp.Replace("{{ FRAME_BACKGROUND_COLOR }}", ColorToRGB(GameData.Frames[i].background).ToString());

			//Layers
			StringBuilder layers = new StringBuilder();
			foreach (var layer in GameData.Frames[i].layers.Items)
			{
				layers.AppendLine($"Layers.push_back(Layer(\"{SanitizeString(layer.Name)}\", {layer.XCoeff}, {layer.YCoeff}));");
			}
			frameCpp = frameCpp.Replace("{{ LAYER_INIT }}", layers.ToString());

			//Object Instances
			StringBuilder objectInstances = new StringBuilder();
			foreach (var obj in GameData.Frames[i].objects)
			{
				//TODO
				//if (obj.InstanceFlags.GetFlag("CreateOnly")) continue; // Skip instances not created on start
				objectInstances.Append($"ObjectInstances.push_back(factory.CreateInstance({obj.handle}, {obj.objectInfo}, {obj.x}, {obj.y}, {obj.layer}, {obj.instance})); // {GameData.frameitems[(int)obj.objectInfo].name}\n");
			}
			frameCpp = frameCpp.Replace("{{ OBJECT_INSTANCES }}", objectInstances.ToString());

			//Event Objects
			//list of each oi in the frame so that we can access them and iterate through them in the events
			StringBuilder eventObjects = new StringBuilder();
			StringBuilder objectSelectorsInit = new StringBuilder();
			List<uint> uniqueHandles = new List<uint>();
			foreach (var obj in GameData.Frames[i].objects)
			{
				//it needs to be an easy to iterate through list, it will also be reset in each function
				if (!uniqueHandles.Contains(obj.objectInfo))
				{
					uniqueHandles.Add(obj.objectInfo);
					string objectName = GameData.frameitems[(int)obj.objectInfo].name;
					eventObjects.AppendLine($"std::shared_ptr<ObjectSelector> {SanitizeObjectName(objectName)}_{obj.objectInfo}_selector;");
					objectSelectorsInit.AppendLine($"{SanitizeObjectName(objectName)}_{obj.objectInfo}_selector = std::make_shared<ObjectSelector>(ObjectInstances, {obj.objectInfo}, false);");
				}
			}
			//qualifiers
			foreach (var qualifier in GameData.Frames[i].events.QualifiersList)
			{
				if (!uniqueHandles.Contains((uint)qualifier.ObjectInfo))
				{
					uniqueHandles.Add((uint)qualifier.ObjectInfo);
					string objectName = Utilities.GetQualifierName(qualifier.ObjectInfo & 0x7FFF, qualifier.Type);
					eventObjects.AppendLine($"std::shared_ptr<ObjectSelector> {SanitizeObjectName(objectName)}_{qualifier.ObjectInfo}_selector;");
					objectSelectorsInit.AppendLine($"{SanitizeObjectName(objectName)}_{qualifier.ObjectInfo}_selector = std::make_shared<ObjectSelector>(ObjectInstances, {qualifier.ObjectInfo - 32768}, true);");
				}
			}

			frameHeader = frameHeader.Replace("{{ OBJECT_SELECTORS }}", eventObjects.ToString());
			frameCpp = frameCpp.Replace("{{ OBJECT_SELECTORS_INIT }}", objectSelectorsInit.ToString());

			//Group enabled on start
			StringBuilder groupActive = new StringBuilder();
			for (int j = 0; j < MfaData.Frames[i].Events.Items.Count; j++)
			{
				var evt = MfaData.Frames[i].Events.Items[j];
				if (evt.Conditions[0].ObjectType == -1 && evt.Conditions[0].Num == -10)
				{
					int groupId = (evt.Conditions[0].Items[0].Loader as Group).Id;
					bool isActiveOnStart = !(evt.Conditions[0].Items[0].Loader as Group).Flags.GetFlag("InactiveOnStart");
					groupActive.Append($"SetGroupActive({groupId}, {isActiveOnStart.ToString().ToLower()});\n");
				}
			}
			frameCpp = frameCpp.Replace("{{ GROUP_ACTIVE }}", groupActive.ToString());

			//Events
			StringBuilder eventIncludes = new StringBuilder();
			StringBuilder eventUpdateLoop = new StringBuilder();
			StringBuilder eventFunctions = new StringBuilder();
			StringBuilder eventLoopIncludes = new StringBuilder();
			StringBuilder runOnceCondition = new StringBuilder();
			StringBuilder oneActionLoop = new StringBuilder();

			List<string> uniqueLoopNames = new List<string>();

			for (int j = 0; j < MfaData.Frames[i].Events.Items.Count; j++)
			{
				var evt = MfaData.Frames[i].Events.Items[j];

				string eventName = $"Event_{j + 1}";

				for (int k = 0; k < evt.RestrictCpt; k++)
				{
					eventUpdateLoop.Append("\t");
				}

				if (evt.Conditions[0].ObjectType == -1 && evt.Conditions[0].Num == -10) // Group Start
				{
					eventUpdateLoop.Append($"if (IsGroupActive({(evt.Conditions[0].Items[0].Loader as Group).Id})) {{\n");
					continue;
				}
				else if (evt.Conditions[0].ObjectType == -1 && evt.Conditions[0].Num == -11) // Group End
				{
					eventUpdateLoop.Remove(eventUpdateLoop.Length - 1, 1); //Remove the last tab
					eventUpdateLoop.Append("}\n");
					continue;
				}

				//Check if there is an on loop condition
				bool onLoop = false;
				foreach (var cond in evt.Conditions)
				{
					if (cond.ObjectType == -1 && cond.Num == -16)
					{
						onLoop = true;

						//Get the loop name and add it to the list of unique loops
						string loopName = ((cond.Items[0].Loader as ExpressionParameter).Items[0].Loader as StringExp).Value.ToString();
						if (!uniqueLoopNames.Contains(loopName))
						{
							uniqueLoopNames.Add(loopName);

							string loopNameSanitized = SanitizeObjectName(loopName);
							eventLoopIncludes.Append($"bool loop_{loopNameSanitized}_running = false;\n");
							eventLoopIncludes.Append($"int loop_{loopNameSanitized}_index = 0;\n\n");
						}

						break;
					}
				}

				if (!onLoop) eventUpdateLoop.Append($"{eventName}();\n");

				eventIncludes.Append($"void {eventName}();\n");
				eventFunctions.Append($"void GeneratedFrame{i}::{eventName}()\n");
				eventFunctions.Append("{\n");

				//Get relevent object instances and reset them
				List<Tuple<int, string>> relevantObjectInfos = new List<Tuple<int, string>>();
				foreach (var cond in evt.Conditions) CheckForRelevantObjectInfos(cond);
				foreach (var act in evt.Actions) CheckForRelevantObjectInfos(act);

				void CheckForRelevantObjectInfos(EventBase eventBase)
				{
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

					foreach (var evtObj in MfaData.Frames[CurrentFrame].Events.Objects)
					{
						if (evtObj.Handle == objectInfo)
						{
							objectName = evtObj.Name;
							objectType = evtObj.ObjectType;
							systemQualifier = evtObj.SystemQualifier;

							//Find object name in ccn frame
							foreach (var ccnObj in GameData.Frames[CurrentFrame].objects)
							{
								if (objectName == GameData.frameitems[(int)ccnObj.objectInfo].name)
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

					if (objectInfo == -1) return;

					if (!relevantObjectInfos.Contains(new Tuple<int, string>(objectInfo, objectName))) relevantObjectInfos.Add(new Tuple<int, string>(objectInfo, objectName));
				}

				//Run Once condition
				bool hasRunOnceCondition = false;
				bool hasOneActionLoop = false;
				foreach (var cond in evt.Conditions)
				{
					if (cond.ObjectType == -1 && cond.Num == -6)
					{
						hasRunOnceCondition = true;
						runOnceCondition.Append($"bool event_{j + 1}_run_once = false;\n");
						break;
					}
					else if (cond.ObjectType == -1 && cond.Num == -7)
					{
						hasOneActionLoop = true;
						oneActionLoop.Append($"bool event_{j + 1}_actions_executed_last_frame = false;\n");
						break;
					}
				}

				if (hasRunOnceCondition)
				{
					eventFunctions.AppendLine($"if (event_{j + 1}_run_once) goto event_{j + 1}_end;");
				}

				if (hasOneActionLoop)
				{
					eventFunctions.AppendLine($"bool allConditionsMet = false;");
				}

				foreach (var obj in relevantObjectInfos)
				{
					string objectName = SanitizeObjectName(obj.Item2) + "_" + obj.Item1;
					eventFunctions.AppendLine($"{objectName}_selector->Reset();");
				}

				//if there no OR conditions, and one of the conditions is false, we go to the end label
				//if there is are Or conditions, and the conditions prior to the OR are false, we go to the next label after the OR
				int numOfOrs = evt.Conditions.Count(x => x.ObjectType == -1 && x.Num == -25);
				int orIndex = 0;

				string nextLabel = $"event_{j + 1}_end";
				if (numOfOrs > 0)
				{
					nextLabel = $"event_{j + 1}_or_{orIndex}";
				}


				//Conditions
				for (int k = 0; k < evt.Conditions.Count; k++)
				{
					var cond = evt.Conditions[k];

					eventFunctions.AppendLine($"// (ObjectType: {cond.ObjectType}, Num: {cond.Num})");

					string ifStatement = (cond.OtherFlags & 1) == 0 ? "if (!" : "if (";

					switch (cond.ObjectType)
					{
						case -7:
							switch (cond.Num)
							{
								case -6: // Repeat while Player Controls Down
									eventFunctions.AppendLine($"{ifStatement} (Application::Instance().GetInput()->IsControlsDown({cond.ObjectInfo}, {((Short)cond.Items[0].Loader).Value}))) goto {nextLabel};");
									break;
								case -4: // Upon Player Controls pressed
									eventFunctions.AppendLine($"{ifStatement} (Application::Instance().GetInput()->IsControlsPressed({cond.ObjectInfo}, {((Short)cond.Items[0].Loader).Value}))) goto {nextLabel};");
									break;
							}
							break;
						case -6:
							switch (cond.Num)
							{
								case -9: // Upon pressing any key
									eventFunctions.AppendLine($"{ifStatement} (Application::Instance().GetInput()->IsAnyKeyPressed())) goto {nextLabel};");
									break;
								case -8: // Repeat while mouse-key is pressed
									eventFunctions.AppendLine($"{ifStatement} (Application::Instance().GetInput()->IsMouseButtonDown({((KeyParameter)cond.Items[0].Loader).Key}))) goto {nextLabel};");
									break;
								case -7: // Upon clicking on object
									Click click = (Click)cond.Items[0].Loader;
									int button = click.Button;
									if (button == 0)
										button = 1;
									else if (button == 1)
										button = 4;
									else if (button == 4)
										button = 1;

									eventFunctions.AppendLine($"{ifStatement} (Application::Instance().GetInput()->IsMouseButtonPressed({button}, {(click.IsDouble == 0 ? false : true).ToString().ToLower()}))) goto {nextLabel};"); //check if the user is clicking	

									//Check mouse is over object
									ParamObject paramObj = (ParamObject)cond.Items[1].Loader;
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(paramObj.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    {ifStatement} (IsColliding(&(**it), GetMouseX(), GetMouseY()))) it.deselect();");
									eventFunctions.AppendLine("}");
									eventFunctions.AppendLine($"if ({GetSelector(paramObj.ObjectInfo)}->Count() == 0) goto {nextLabel};");

									break;
								case -5: // Upon clicking with mouse-key
									int button2 = ((Click)cond.Items[0].Loader).Button;
									if (button2 == 0)
										button2 = 1;
									else if (button2 == 1)
										button2 = 4;
									else if (button2 == 4)
										button2 = 1;

									eventFunctions.AppendLine($"{ifStatement} (Application::Instance().GetInput()->IsMouseButtonPressed({button2}, {(((Click)cond.Items[0].Loader).IsDouble == 0 ? false : true).ToString().ToLower()}))) goto {nextLabel};");
									break;
								case -4: // Mouse pointer is over
									ParamObject obj = (ParamObject)cond.Items[0].Loader;
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(obj.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    {ifStatement} (IsColliding(&(**it), GetMouseX(), GetMouseY()))) it.deselect();");
									eventFunctions.AppendLine("}");
									eventFunctions.AppendLine($"if ({GetSelector(obj.ObjectInfo)}->Count() == 0) goto {nextLabel};");
									break;
								case -2: // Repeat While Key Pressed
									eventFunctions.AppendLine($"{ifStatement} (Application::Instance().GetInput()->IsKeyDown({((KeyParameter)cond.Items[0].Loader).Key}))) goto {nextLabel};");
									break;
								case -1: // Upon pressing a key
									eventFunctions.AppendLine($"{ifStatement} (Application::Instance().GetInput()->IsKeyPressed({((KeyParameter)cond.Items[0].Loader).Key}))) goto {nextLabel};");
									break;
							}
							break;
						case -4: // Timer
							switch (cond.Num)
							{
								case -8: // Every
								case -4:
									eventFunctions.AppendLine($"{ifStatement} (GameTimer.CheckEvent({j}, {((Time)cond.Items[0].Loader).Timer}, TimerEventType::Every))) goto {nextLabel};");
									break;
								case -7: // Timer equal to
									eventFunctions.AppendLine($"{ifStatement} (GameTimer.CheckEvent({j}, {((Time)cond.Items[0].Loader).Timer}, TimerEventType::Equals))) goto {nextLabel};");
									break;
								case -2: // Timer less than
									eventFunctions.AppendLine($"{ifStatement} (GameTimer.CheckEvent({j}, {((Time)cond.Items[0].Loader).Timer}, TimerEventType::LessThan))) goto {nextLabel};");
									break;
								case -1: // Timer greater than
									eventFunctions.AppendLine($"{ifStatement} (GameTimer.CheckEvent({j}, {((Time)cond.Items[0].Loader).Timer}, TimerEventType::GreaterThan))) goto {nextLabel};");
									break;
							}
							break;
						case -3:
							switch (cond.Num)
							{
								case -1: // Start of Frame
									eventFunctions.AppendLine($"{ifStatement} (Application::Instance().GetCurrentState() == GameState::StartOfFrame)) goto {nextLabel};");
									break;
							}
							break;
						case -1:
							switch (cond.Num)
							{
								case -25: // Or (logical)
										  //if we got here then the conditions mustve succeeded, so we can skip to the actions
									eventFunctions.AppendLine("goto event_" + (j + 1) + "_actions;");

									eventFunctions.AppendLine("event_" + (j + 1) + "_or_" + orIndex + ":;");

									orIndex++;

									if (orIndex == numOfOrs)
									{
										nextLabel = "event_" + (j + 1) + "_end";
									}
									else
									{
										nextLabel = "event_" + (j + 1) + "_or_" + orIndex;
									}

									//Reset instances
									foreach (var relevantObjectInfo in relevantObjectInfos)
									{
										eventFunctions.AppendLine(SanitizeObjectName(relevantObjectInfo.Item2) + "_" + relevantObjectInfo.Item1 + "_selector->Reset();");
									}

									break;
								case -3: // Compare 2 general values
									eventFunctions.AppendLine($"{ifStatement} ({ConvertExpression((ExpressionParameter)cond.Items[0].Loader, cond)} {GetComparisonSymbol(((ExpressionParameter)cond.Items[1].Loader).Comparsion)} {ConvertExpression((ExpressionParameter)cond.Items[1].Loader, cond)})) goto {nextLabel};");
									break;
								case -2: // Never
									eventFunctions.AppendLine($"goto {nextLabel};");
									break;
							}
							break;
						case >= 0:
							switch (cond.Num)
							{
								case -81: // Counter Compare
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(cond.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.AppendLine($"    auto value = std::dynamic_pointer_cast<Value>(commonProperties->oValue);");
									eventFunctions.AppendLine($"    if (value->GetValue() {GetOppositeComparison(((ExpressionParameter)cond.Items[0].Loader).Comparsion)} {ConvertExpression((ExpressionParameter)cond.Items[0].Loader, cond)}) it.deselect();");
									eventFunctions.AppendLine("}");

									//If no instances are selected, we go to the end label
									eventFunctions.AppendLine($"if ({GetSelector(cond.ObjectInfo)}->Count() == 0) goto {nextLabel};");
									break;
								case -27: // Compare Alterable Value
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(cond.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.AppendLine($"    if (commonProperties->oAlterableValues->GetValue({((Short)cond.Items[0].Loader).Value}) {GetOppositeComparison(((ExpressionParameter)cond.Items[1].Loader).Comparsion)} {ConvertExpression((ExpressionParameter)cond.Items[1].Loader, cond)}) it.deselect();");
									eventFunctions.AppendLine("}");

									//If no instances are selected, we go to the end label
									eventFunctions.AppendLine($"if ({GetSelector(cond.ObjectInfo)}->Count() == 0) goto {nextLabel};");
									break;
								case -25: // Flag is on
								case -24: // Flag is off
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(cond.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.AppendLine($"    bool flag = commonProperties->oAlterableFlags->GetFlag({ConvertExpression((ExpressionParameter)cond.Items[0].Loader, cond)});");

									if (cond.Num == -25)
										eventFunctions.AppendLine($"    if (!flag) it.deselect();");
									else
										eventFunctions.AppendLine($"    if (flag) it.deselect();");

									eventFunctions.AppendLine("}");

									//If no instances are selected, we go to the end label
									eventFunctions.AppendLine($"if ({GetSelector(cond.ObjectInfo)}->Count() == 0) goto {nextLabel};");
									break;
								case -23: // Overlapping a backdrop
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(cond.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    {ifStatement} IsCollidingWithBackground(&(**it))) it.deselect();");
									eventFunctions.AppendLine("}");

									//If no instances are selected, we go to the end label
									eventFunctions.AppendLine($"if ({GetSelector(cond.ObjectInfo)}->Count() == 0) goto {nextLabel};");
									break;
								case -17: // Compare X position
								case -16: // Compare Y position
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(cond.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    if ({(cond.Num == -17 ? "instance->X" : "instance->Y")} {GetOppositeComparison(((ExpressionParameter)cond.Items[0].Loader).Comparsion)} {ConvertExpression((ExpressionParameter)cond.Items[0].Loader, cond)}) it.deselect();");
									eventFunctions.AppendLine("}");

									//If no instances are selected, we go to the end label
									eventFunctions.AppendLine($"if ({GetSelector(cond.ObjectInfo)}->Count() == 0) goto {nextLabel};");
									break;
								case -4: // Object is overlapping an another object
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(cond.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    bool hasCollision = false;");
									eventFunctions.AppendLine($"    for (ObjectIterator other(*{GetSelector(((ParamObject)cond.Items[0].Loader).ObjectInfo)}); !other.end(); ++other) {{");
									eventFunctions.AppendLine($"        if (IsColliding(&(**it), &(**other))) {{");
									eventFunctions.AppendLine($"            hasCollision = true;");
									eventFunctions.AppendLine($"            break;");
									eventFunctions.AppendLine($"        }}");
									eventFunctions.AppendLine($"    }}");
									eventFunctions.AppendLine($"    {ifStatement} hasCollision) it.deselect();");
									eventFunctions.AppendLine("}");

									//If no instances are selected, we go to the end label
									eventFunctions.AppendLine($"if ({GetSelector(cond.ObjectInfo)}->Count() == 0) goto {nextLabel};");
									break;
								case -2: // Animation is over
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(cond.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.AppendLine($"    auto animations = std::dynamic_pointer_cast<Animations>(commonProperties->oAnimations);");
									eventFunctions.AppendLine($"    if (!animations->IsSequenceOver({((Short)cond.Items[0].Loader).Value})) it.deselect();");
									eventFunctions.AppendLine("}");

									eventFunctions.AppendLine($"if ({GetSelector(cond.ObjectInfo)}->Count() == 0) goto {nextLabel};");
									break;
								case -1: // Current frame of
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(cond.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.AppendLine($"    auto animations = std::dynamic_pointer_cast<Animations>(commonProperties->oAnimations);");
									eventFunctions.AppendLine($"    if (animations->GetCurrentFrameIndex() != {ConvertExpression((ExpressionParameter)cond.Items[0].Loader, cond)}) it.deselect();");
									eventFunctions.AppendLine("}");

									eventFunctions.AppendLine($"if ({GetSelector(cond.ObjectInfo)}->Count() == 0) goto {nextLabel};");
									break;
								default:
									eventFunctions.AppendLine($"{ifStatement} (false)) goto {nextLabel};");
									break;
							}
							break;
					}
				}

				//Actions
				eventFunctions.AppendLine($"event_{j + 1}_actions:;\n");

				if (hasRunOnceCondition)
				{
					eventFunctions.AppendLine($"event_{j + 1}_run_once = true;");
				}

				if (hasOneActionLoop)
				{
					eventFunctions.AppendLine($"allConditionsMet = true;");
					eventFunctions.AppendLine($"if (event_{j + 1}_actions_executed_last_frame) goto event_{j + 1}_end;");
					eventFunctions.AppendLine($"event_{j + 1}_actions_executed_last_frame = true;");

				}

				for (int k = 0; k < evt.Actions.Count; k++)
				{
					var act = evt.Actions[k];

					eventFunctions.AppendLine($"// (ObjectType: {act.ObjectType}, Num: {act.Num})");

					switch (act.ObjectType)
					{
						case -7: // Player
							switch (act.Num)
							{
								case 0: // Set Score
									eventFunctions.AppendLine($"Application::Instance().GetAppData()->SetScore({act.ObjectInfo}, {ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
									break;
								case 1: // Set Lives
									eventFunctions.AppendLine($"Application::Instance().GetAppData()->SetLives({act.ObjectInfo}, {ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
									break;
								case 4: // Add to Score
									eventFunctions.AppendLine($"Application::Instance().GetAppData()->AddScore({act.ObjectInfo}, {ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
									break;
								case 5: // Add to Lives
									eventFunctions.AppendLine($"Application::Instance().GetAppData()->AddLives({act.ObjectInfo}, {ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
									break;
								case 6: // Subtract from Score
									eventFunctions.AppendLine($"Application::Instance().GetAppData()->SubtractScore({act.ObjectInfo}, {ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
									break;
								case 7: // Subtract from Lives
									eventFunctions.AppendLine($"Application::Instance().GetAppData()->SubtractLives({act.ObjectInfo}, {ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
									break;
							}
							break;
						case -5: // New objects
							switch (act.Num)
							{
								case 0: // Create object
									Create create = (Create)act.Items[0].Loader;

									eventFunctions.AppendLine("{");
									if (create.Position.ObjectInfoParent != ushort.MaxValue)
									{
										eventFunctions.AppendLine($"auto parent = *{GetSelector((int)create.Position.ObjectInfoParent)}->begin();");
									}
									eventFunctions.AppendLine($"CreateInstance({create.Position.X}, {create.Position.Y}, {create.Position.Layer}, {create.ObjectInfo}, {create.Position.Angle}, {(create.Position.ObjectInfoParent != ushort.MaxValue ? "parent.get()" : "nullptr")});");
									//add to selector
									eventFunctions.AppendLine($"{GetSelector(create.ObjectInfo)}->AddExternalInstance(ObjectInstances.back());");
									eventFunctions.AppendLine("}");
									break;
								case 2: // Create object at
									ParamObject obj = (ParamObject)act.Items[0].Loader;
									string X = ConvertExpression((ExpressionParameter)act.Items[1].Loader, act);
									string Y = ConvertExpression((ExpressionParameter)act.Items[2].Loader, act);
									string layer = ConvertExpression((ExpressionParameter)act.Items[3].Loader, act);
									eventFunctions.AppendLine($"CreateInstance({X}, {Y}, ({layer}) - 1, {obj.ObjectInfo}, 0);");
									//add to selector
									eventFunctions.AppendLine($"{GetSelector(obj.ObjectInfo)}->AddExternalInstance(ObjectInstances.back());");
									break;
							}
							break;
						case -3: // Storyboard Controls
							switch (act.Num)
							{
								case 0: // Next Frame
									eventFunctions.AppendLine("Application::Instance().QueueStateChange(GameState::NextFrame);");
									break;
								case 1: // Previous Frame
									eventFunctions.AppendLine("Application::Instance().QueueStateChange(GameState::PreviousFrame);");
									break;
								case 2: // Jump to Frame
									string frame;
									if (act.Items[0].Loader is ExpressionParameter)
										frame = $"(({ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)}) - 1)";
									else
										frame = GameData.frameHandles.Items[((Short)act.Items[0].Loader).Value].ToString();

									eventFunctions.AppendLine($"Application::Instance().QueueStateChange(GameState::JumpToFrame, {frame});");
									break;
								case 4: // End Application
									eventFunctions.AppendLine("Application::Instance().QueueStateChange(GameState::EndApplication);");
									break;
								case 5: // Restart Application
									eventFunctions.AppendLine("Application::Instance().QueueStateChange(GameState::RestartApplication);");
									break;
								case 6: // Restart Current Frame
									eventFunctions.AppendLine("Application::Instance().QueueStateChange(GameState::RestartFrame);");
									break;
								case 7: // Center Display
									Position position = (Position)act.Items[0].Loader;
									if (position.ObjectInfoParent == ushort.MaxValue) // Absolute position
									{
										eventFunctions.AppendLine($"SetScroll({position.X}, {position.Y});");
									}
									else // Relative position from object
									{
										eventFunctions.AppendLine("{");
										eventFunctions.AppendLine($"auto parent = *{GetSelector((int)position.ObjectInfoParent)}->begin();");
										eventFunctions.AppendLine($"SetScroll({position.X} + parent->X, {position.Y} + parent->Y, parent->Layer);");
										eventFunctions.AppendLine("}");
									}
									break;
								case 8: // Center Display at X
									act.ObjectInfoList = -1; // TODO: im doing this because or else it will write it as an "SetScrollX(instance->X)" rather than "SetScrollX(player_selector->begin()->X)"
									eventFunctions.AppendLine($"SetScrollX({ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
									break;
								case 9: // Center Display at Y
									act.ObjectInfoList = -1; // TODO: im doing this because or else it will write it as an "SetScrollX(instance->X)" rather than "SetScrollX(player_selector->begin()->X)"
									eventFunctions.AppendLine($"SetScrollY({ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
									break;
							}
							break;
						case -1: // Special
							switch (act.Num)
							{
								case 14: // Start Loop
									{
										string loopName = SanitizeObjectName(ConvertExpression((ExpressionParameter)act.Items[0].Loader, act).ToString());
										loopName = loopName.Substring(2, loopName.Length - 4); // remove first 2 letters and last 2 letters as they are quotes

										act.ObjectInfoList = -1; // TODO: im doing this because or else it will write it as an "instance->???" rather than "player_selector->begin()->???"

										eventFunctions.AppendLine($"loop_{loopName}_running = true;");
										eventFunctions.AppendLine($"loop_{loopName}_index = 0;");
										eventFunctions.AppendLine($"int loopTimes = {ConvertExpression((ExpressionParameter)act.Items[1].Loader, act)};");
										eventFunctions.AppendLine($"while (loop_{loopName}_running && loop_{loopName}_index < loopTimes) {{");
										eventFunctions.AppendLine($"    {loopName}_loop();");
										eventFunctions.AppendLine($"    if (!loop_{loopName}_running) break;");
										eventFunctions.AppendLine($"    loop_{loopName}_index++;");
										eventFunctions.AppendLine("}");
									}
									break;
								case 15: // End Loop
									{
										string loopName = SanitizeObjectName(ConvertExpression((ExpressionParameter)act.Items[0].Loader, act).ToString());
										loopName = loopName.Substring(2, loopName.Length - 4); // remove first 2 letters and last 2 letters as they are quotes

										eventFunctions.AppendLine($"loop_{loopName}_running = false;");
									}
									break;
							}
							break;
						case >= 0:
							switch (act.Num)
							{
								default:
									if (act.ObjectType < 32)
									{
										switch (act.ObjectType)
										{
											case 7: // Counter
												switch (act.Num)
												{
													case 80: // Set Counter
														eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
														eventFunctions.AppendLine($"    auto instance = *it;");
														eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
														eventFunctions.AppendLine($"    auto value = std::dynamic_pointer_cast<Value>(commonProperties->oValue);");
														eventFunctions.AppendLine($"    value->SetValue({ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
														eventFunctions.AppendLine("}");
														break;
													case 81: // Add to Counter
														eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
														eventFunctions.AppendLine($"    auto instance = *it;");
														eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
														eventFunctions.AppendLine($"    auto value = std::dynamic_pointer_cast<Value>(commonProperties->oValue);");
														eventFunctions.AppendLine($"    value->AddValue({ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
														eventFunctions.AppendLine("}");
														break;
													case 82: // Subtract from Counter
														eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
														eventFunctions.AppendLine($"    auto instance = *it;");
														eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
														eventFunctions.AppendLine($"    auto value = std::dynamic_pointer_cast<Value>(commonProperties->oValue);");
														eventFunctions.AppendLine($"    value->SubtractValue({ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
														eventFunctions.AppendLine("}");
														break;
												}
												break;
										}
									}
									break;
								case 1: //Set position at
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");

									Position position = (Position)act.Items[0].Loader;
									if (position.ObjectInfoParent == ushort.MaxValue) // Absolute position
									{
										eventFunctions.AppendLine($"    instance->X = {position.X};");
										eventFunctions.AppendLine($"    instance->Y = {position.Y};");
									}
									else // Relative position from object
									{
										//get the object
										eventFunctions.AppendLine($"    auto parent = {GetSelector((int)position.ObjectInfoParent)}->At(it.index());");
										eventFunctions.AppendLine($"    instance->X = {position.X} + parent->X;");
										eventFunctions.AppendLine($"    instance->Y = {position.Y} + parent->Y;");
									}
									eventFunctions.AppendLine("}");
									break;
								case 2: //Set X Position
								case 3: //Set Y Position
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    instance->{(act.Num == 2 ? "X" : "Y")} = {ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)};");
									eventFunctions.AppendLine("}");
									break;
								case 15: // Stop Animation
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.AppendLine($"    auto animations = std::dynamic_pointer_cast<Animations>(commonProperties->oAnimations);");
									eventFunctions.AppendLine($"    animations->Stop();");
									eventFunctions.AppendLine("}");
									break;
								case 16: // Start Animation
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.AppendLine($"    auto animations = std::dynamic_pointer_cast<Animations>(commonProperties->oAnimations);");
									eventFunctions.AppendLine($"    animations->Start();");
									eventFunctions.AppendLine("}");
									break;
								case 17: // Change animation sequence
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.AppendLine($"    auto animations = std::dynamic_pointer_cast<Animations>(commonProperties->oAnimations);");
									eventFunctions.AppendLine($"    animations->SetCurrentSequenceIndex({((Short)act.Items[0].Loader).Value});");
									eventFunctions.AppendLine("}");
									break;
								case 24: // Destroy
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    MarkForDeletion(instance.get());");
									eventFunctions.AppendLine("}");

									// TODO: REMOVE FROM SELECTOR

									break;
								case 26: // Make Invisible
								case 27: // Reappear
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.AppendLine($"    commonProperties->Visible = {(act.Num == 26 ? false : true).ToString().ToLower()};");
									eventFunctions.AppendLine("}");
									break;
								case 31: // Set Alterable Value
								case 32: // Add to Alterable Value
								case 33: // Subtract from Alterable Value
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.Append($"    commonProperties->oAlterableValues->");
									switch (act.Num)
									{
										case 31:
											eventFunctions.AppendLine($"SetValue({((Short)act.Items[0].Loader).Value}, {ConvertExpression((ExpressionParameter)act.Items[1].Loader, act)});");
											break;
										case 32:
											eventFunctions.AppendLine($"AddValue({((Short)act.Items[0].Loader).Value}, {ConvertExpression((ExpressionParameter)act.Items[1].Loader, act)});");
											break;
										case 33:
											eventFunctions.AppendLine($"SubtractValue({((Short)act.Items[0].Loader).Value}, {ConvertExpression((ExpressionParameter)act.Items[1].Loader, act)});");
											break;
									}
									eventFunctions.AppendLine("}");
									break;
								case 35: // Set Flag On
								case 36: // Set Flag Off
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.AppendLine($"    commonProperties->oAlterableFlags->SetFlag({ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)}, {(act.Num == 35 ? true : false).ToString().ToLower()});");
									eventFunctions.AppendLine("}");
									break;
								case 37: // Toggle Flag
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.AppendLine($"    commonProperties->oAlterableFlags->ToggleFlag({ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
									eventFunctions.AppendLine("}");
									break;
								case 40: // Force Animation Frame
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.AppendLine($"    commonProperties->oAnimations->SetForcedFrame({ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
									eventFunctions.AppendLine("}");
									break;
								case 41: // Restore Animation Frame
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
									eventFunctions.AppendLine($"    commonProperties->oAnimations->RestoreForcedFrame();");
									eventFunctions.AppendLine("}");
									break;
								case 66: // RGB Coefficient
									eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
									eventFunctions.AppendLine($"    auto instance = *it;");
									eventFunctions.AppendLine($"    instance->OI->RGBCoefficient = {ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)};");
									eventFunctions.AppendLine("}");
									break;
								case 88: // Set angle

									if (act.ObjectType == 3)
									{
										eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
										eventFunctions.AppendLine($"  auto instance = *it;");
										eventFunctions.AppendLine($"  auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
										eventFunctions.AppendLine($"  auto paragraphs = std::dynamic_pointer_cast<ObjectParagraphs>(commonProperties->oParagraphs);");
										eventFunctions.AppendLine($"  paragraphs->SetAlterableText({ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
										eventFunctions.AppendLine("}");
									}
									else if (act.ObjectType == 8)
									{
									}
									else
									{
										eventFunctions.AppendLine($"for (ObjectIterator it(*{GetSelector(act.ObjectInfo)}); !it.end(); ++it) {{");
										eventFunctions.AppendLine($"    auto instance = *it;");
										eventFunctions.AppendLine($"    instance->SetAngle({ConvertExpression((ExpressionParameter)act.Items[0].Loader, act)});");
										eventFunctions.AppendLine("}");
									}
									break;
							}
							break;
					}
				}

				eventFunctions.AppendLine($"event_{j + 1}_end:;\n");

				if (hasOneActionLoop)
				{
					eventFunctions.AppendLine($"if (!allConditionsMet) event_{j + 1}_actions_executed_last_frame = false;");
				}

				eventFunctions.Append("}\n\n");
			}

			foreach (var loopName in uniqueLoopNames)
			{
				eventIncludes.AppendLine($"void {SanitizeObjectName(loopName)}_loop();");

				eventFunctions.Append($"void GeneratedFrame{i}::{SanitizeObjectName(loopName)}_loop()\n");
				eventFunctions.Append("{\n");

				//go through every event and find the ones with the on loop condition
				for (int j = 0; j < GameData.Frames[i].events.Items.Count; j++)
				{
					var evt = GameData.Frames[i].events.Items[j];
					if (evt.Conditions[0].ObjectType == -1 && evt.Conditions[0].Num == -16)
					{
						string loopNameSanitized = SanitizeObjectName((((evt.Conditions[0].Items[0].Loader as ExpressionParameter).Items[0].Loader as StringExp).Value.ToString()));
						if (loopNameSanitized == SanitizeObjectName(loopName))
						{
							//TODO: Check if group is active?
							eventFunctions.AppendLine($"\tEvent_{j + 1}();");
						}
					}
				}

				eventFunctions.Append("}\n\n");
			}

			frameHeader = frameHeader.Replace("{{ EVENT_INCLUDES }}", eventIncludes.ToString());
			frameHeader = frameHeader.Replace("{{ LOOP_INCLUDES }}", eventLoopIncludes.ToString());
			frameHeader = frameHeader.Replace("{{ RUN_ONCE_CONDITION }}", runOnceCondition.ToString());
			frameHeader = frameHeader.Replace("{{ ONLY_ONE_ACTION_WHEN_LOOP_CONDITION }}", oneActionLoop.ToString());
			frameCpp = frameCpp.Replace("{{ EVENT_UPDATE_LOOP }}", eventUpdateLoop.ToString());
			frameCpp = frameCpp.Replace("{{ EVENT_FUNCTIONS }}", eventFunctions.ToString());

			// Write frame files
			SaveFile(Path.Combine(OutputPath.FullName, "include", $"GeneratedFrame{i}.h"), frameHeader);
			SaveFile(Path.Combine(OutputPath.FullName, "source", $"GeneratedFrame{i}.cpp"), frameCpp);

			// Add to factory
			frameIncludes += $"#include \"GeneratedFrame{i}.h\"\n";
			frameCases += $"        case {i}:\n            return std::make_unique<GeneratedFrame{i}>();\n";
		}

		// Write factory implementation
		string frameFactoryCpp = frameFactoryCppTemplate.Replace("{{ FRAME_INCLUDES }}", frameIncludes);
		frameFactoryCpp = frameFactoryCpp.Replace("{{ FRAME_CASES }}", frameCases);

		SaveFile(Path.Combine(OutputPath.FullName, "source", "FrameFactory.cpp"), frameFactoryCpp);

		//Delete the template files
		File.Delete(Path.Combine(OutputPath.FullName, "include", "Frame.template.h"));
		File.Delete(Path.Combine(OutputPath.FullName, "source", "Frame.template.cpp"));
		File.Delete(Path.Combine(OutputPath.FullName, "source", "FrameFactory.template.cpp"));
	}

	string ConvertExpression(ExpressionParameter expressions, EventBase eventBase = null)
	{

		string result = string.Empty;
		for (int i = 0; i < expressions.Items.Count; i++)
		{
			Expression expression = expressions.Items[i];
			if (expression.ObjectType == -6 && expression.Num == 0) // XMouse
			{
				result += "Application::Instance().GetInput()->GetMouseX()";
			}
			else if (expression.ObjectType == -6 && expression.Num == 1) // YMouse
			{
				result += "Application::Instance().GetInput()->GetMouseY()";
			}
			else if (expression.ObjectType == -6 && expression.Num == 2) // WheelDelta
			{
				result += "Application::Instance().GetInput()->GetMouseWheelMove()";
			}
			else if (expression.ObjectType == -5 && expression.Num == 0) // Total Objects
			{
				result += $"ObjectInstances.size()";
			}
			else if (expression.ObjectType == -4 && expression.Num == 0) // Timer
			{
				result += "GameTimer.GetTime()";
			}
			else if (expression.ObjectType == -4 && expression.Num == 1) // Hundreds
			{
				result += "GameTimer.GetHundreds()";
			}
			else if (expression.ObjectType == -4 && expression.Num == 2) // seconds
			{
				result += "GameTimer.GetSeconds()";
			}
			else if (expression.ObjectType == -4 && expression.Num == 3) // Minutes
			{
				result += "GameTimer.GetMinutes()";
			}
			else if (expression.ObjectType == -4 && expression.Num == 4) // Hours
			{
				result += "GameTimer.GetHours()";
			}
			else if (expression.ObjectType == -4 && expression.Num == 5) // Event Index
			{
				result += $"0"; // TODO
			}
			else if (expression.ObjectType == -3 && (expression.Num == 0 || expression.Num == 8)) // Frame
			{
				result += $"Index + 1";
			}
			else if (expression.ObjectType == -3 && expression.Num == 10) // FrameRate
			{
				result += "Application::Instance().GetAppData()->GetTargetFPS()"; // TODO: Verify this
			}
			else if (expression.ObjectType == -3 && expression.Num == 14) // DisplayMode
			{
				result += "0"; // TODO
			}
			else if (expression.ObjectType == -3 && expression.Num == 15) // PixelShaderVersion
			{
				result += "0"; // TODO
			}
			else if (expression.ObjectType == -2 && expression.Num == 12) // ChannelSampleName$
			{
				result += "std::to_string("; // TODO
			}
			else if (expression.ObjectType == -1 && expression.Num == -3)
			{
				result += ", ";
			}
			else if (expression.ObjectType == -1 && expression.Num == -2)
			{
				result += ")";
			}
			else if (expression.ObjectType == -1 && expression.Num == -1)
			{
				result += "(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 0)
			{
				ExpressionLoader loader = (ExpressionLoader)expression.Loader;

				if (loader is StringExp) result += $"\"{(loader as StringExp).Value}\"";
				else if (loader is DoubleExp) result += (loader as DoubleExp).FloatValue;
				else result += loader.Value.ToString();
			}
			else if (expression.ObjectType == -1 && expression.Num == 1) // Random(
			{
				result += "Application::Instance().Random(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 3)
			{
				result += $"\"{expression.Loader.ToString()}\"";
			}
			else if (expression.ObjectType == -1 && expression.Num == 4) // Str$
			{
				result += $"std::to_string(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 6) // Appdrive$
			{
				result += "\"\""; // TODO
			}
			else if (expression.ObjectType == -1 && expression.Num == 7) // Appdir$
			{
				result += "\"\""; // TODO
			}
			else if (expression.ObjectType == -1 && expression.Num == 8) // Apppath$
			{
				result += "\"\""; // TODO
			}
			else if (expression.ObjectType == -1 && expression.Num == 9) // Appname$
			{
				result += "\"\""; // TODO
			}
			else if (expression.ObjectType == -1 && expression.Num == 23)
			{
				result += (expression.Loader as DoubleExp).FloatValue;
			}
			else if (expression.ObjectType == -1 && expression.Num == 29) // Abs(
			{
				result += "std::abs(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 41) // Max(
			{
				result += "std::max(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 46) // LoopIndex
			{
				result += "0"; // TODO
							   //skip
				i += 2;
			}
			else if (expression.ObjectType == -1 && expression.Num == 50) // Global String
			{
				result += $"Application::Instance().GetAppData()->GetGlobalStrings()[{(expression.Loader as GlobalCommon).Value}]";
			}
			else if (expression.ObjectType == -1 && expression.Num == 56) // AppTempPath$
			{
				result += "\"\""; // TODO
			}
			else if (expression.ObjectType == -1 && expression.Num == 65) // RRandom
			{
				result += "Application::Instance().RandomRange(";
			}
			else if (expression.ObjectType == -1 && expression.Num == 67) // RuntimeName$
			{
				result += "Application::Instance().GetBackend()->GetPlatformName()";
			}
			else if (expression.ObjectType == 0 && expression.Num == 2) // Add
			{
				result += " + ";
			}
			else if (expression.ObjectType == 0 && expression.Num == 4) // Sub
			{
				result += " - ";
			}
			else if (expression.ObjectType == 0 && expression.Num == 6) // Multiply
			{
				result += " * ";
			}
			else if (expression.ObjectType == 0 && expression.Num == 8) // Division
			{
				result += " /MathHelper::safeDivision/ ";
			}
			else if (expression.ObjectType > 0 && expression.Num == 1) // Y Position
			{
				if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
					result += "instance->Y";
				else
					result += $"(*{GetSelector(expression.ObjectInfo)}->begin())->Y";
			}
			else if (expression.ObjectType > 0 && expression.Num == 2) // Image
			{
				if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
					result += "std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties)->oAnimations->GetCurrentFrameIndex()";
				else
					result += $"std::dynamic_pointer_cast<CommonProperties>((*{GetSelector(expression.ObjectInfo)}->begin())->OI->Properties)->oAnimations->GetCurrentFrameIndex()";
			}
			else if (expression.ObjectType > 0 && expression.Num == 11) // X Position
			{
				if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
					result += "instance->X";
				else
					result += $"(*{GetSelector(expression.ObjectInfo)}->begin())->X";
			}
			else if (expression.ObjectType > 0 && expression.Num == 12) // Fixed Value
			{
				result += "0"; // TODO
			}
			else if (expression.ObjectType > 0 && expression.Num == 14) // Animation Number
			{
				if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
					result += "std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties)->oAnimations->GetCurrentSequenceIndex()";
				else
					result += $"std::dynamic_pointer_cast<CommonProperties>((*{GetSelector(expression.ObjectInfo)}->begin())->OI->Properties)->oAnimations->GetCurrentSequenceIndex()";
			}
			else if (expression.ObjectType > 0 && expression.Num == 16) // Alterable Value
			{
				if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
					result += "std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties)->oAlterableValues->GetValue(" + ((ShortExp)expression.Loader).Value + ")";
				else
					result += $"std::dynamic_pointer_cast<CommonProperties>((*{GetSelector(expression.ObjectInfo)}->begin())->OI->Properties)->oAlterableValues->GetValue({((ShortExp)expression.Loader).Value})";
			}
			else if (expression.ObjectType > 0 && expression.Num == 22) // Font Color
			{
				result += "0"; // TODO
			}
			else if (expression.ObjectType > 0 && expression.Num == 46)
			{
				result += "instance->InstanceValue";
			}
			else if (expression.ObjectType > 0 && expression.Num == 80)
			{
				if (expression.ObjectType == 7) // Counter
				{
					string selector = GetSelector(expression.ObjectInfo);
					result += $"std::dynamic_pointer_cast<CommonProperties>((*({selector}->begin()))->OI->Properties)->oValue->GetValue()";
				}
			}
			else if (expression.ObjectType > 0 && expression.Num == 83)
			{
				if (expression.ObjectType == 2) // Angle
				{
					if (expression.ObjectInfo == eventBase.ObjectInfo && expression.ObjectInfoList == eventBase.ObjectInfoList)
						result += "instance->GetAngle()";
					else
						result += $"(*{GetSelector(expression.ObjectInfo)}->begin())->GetAngle()";
				}
			}
			else if (expression.ObjectType > 32)
			{
				return "0"; // TODO
			}
			else
			{
				Logger.Log($"No expresion match, ObjectType: {expression.ObjectType}, Num: {expression.Num}");
			}
		}
		return result;
	}

	// Helper methods
	void CopyFilesRecursively(string sourcePath, string targetPath)
	{
		//Now Create all of the directories
		foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
		{
			Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
		}

		//Copy all the files & Replaces any files with the same name
		foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
		{
			SaveFile(newPath.Replace(sourcePath, targetPath), File.ReadAllText(newPath));
		}
	}

	void SaveFile(string path, string content)
	{
		//Check if the content is different from the file (Helps with compile times)
		if (!File.Exists(path))
		{
			File.WriteAllText(path, content);
		}
		else
		{
			string currentContent = File.ReadAllText(path);
			if (currentContent != content)
			{
				File.WriteAllText(path, content);
			}
		}
	}

	string SanitizeString(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return "";
		}

		var result = new StringBuilder();
		foreach (char c in input)
		{
			switch (c)
			{
				case '"':
					result.Append("\\\"");
					break;
				case '\\':
					result.Append("\\\\");
					break;
				case '\r':
					result.Append("\\r");
					break;
				case '\n':
					result.Append("\\n");
					break;
				case '\t':
					result.Append("\\t");
					break;
				case '\b':
					result.Append("\\b");
					break;
				case '\f':
					result.Append("\\f");
					break;
				case '\v':
					result.Append("\\v");
					break;
				case '\a':
					result.Append("\\a");
					break;
				case '\0':
					result.Append("\\0");
					break;
				default:
					if (c < 32 || c > 126)
					{
						result.Append($"\\u{(int)c:X4}");
					}
					else
					{
						result.Append(c);
					}
					break;
			}
		}
		return result.ToString();
	}


	string SanitizeObjectName(string input)
	{
		//if the first character is a number, add an underscore
		if (char.IsDigit(input[0]))
		{
			input = "_" + input;
		}
		return SanitizeString(input).Replace(" ", "_").Replace(".", "_").Replace("-", "_").Replace(":", "_").Replace(";", "_").Replace(",", "_").Replace("!", "_").Replace("?", "_").Replace("*", "_").Replace("/", "_").Replace("\\", "_").Replace("|", "_").Replace("`", "_").Replace("'", "_").Replace("\"", "_").Replace("'", "_").Replace("\"", "_").Replace("'", "_").Replace("\"", "_").Replace("&", "_");
	}

	string ColorToRGB(Color color)
	{
		return $"0x{color.R:X2}{color.G:X2}{color.B:X2}";
	}

	int ColorToArgb(Color color)
	{
		return color.ToArgb();
	}

	string GetSelector(int objectInfo)
	{
		Tuple<int, string> obj = GetObject(objectInfo);
		return $"{SanitizeObjectName(obj.Item2)}_{obj.Item1}_selector";
	}

	//Because MFA objectinfos don't match up with the CCN, we have to do this chicanery
	Tuple<int, string> GetObject(int objectInfo)
	{
		string objectName = "";
		int objectType = 0;
		int systemQualifier = 0;

		foreach (var evtObj in MfaData.Frames[CurrentFrame].Events.Objects)
		{
			if (evtObj.Handle == objectInfo)
			{
				objectName = evtObj.Name;
				objectType = evtObj.ObjectType;
				systemQualifier = evtObj.SystemQualifier;

				//Find object name in ccn frame
				foreach (var ccnObj in GameData.Frames[CurrentFrame].objects)
				{
					if (objectName == GameData.frameitems[(int)ccnObj.objectInfo].name)
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

		return new Tuple<int, string>(objectInfo, objectName);
	}

	public string GetComparisonSymbol(short comparison)
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

	public string GetOppositeComparison(short comparison)
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
}
