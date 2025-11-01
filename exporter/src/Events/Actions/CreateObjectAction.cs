using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class CreateObjectAction : ActionBase
{
	public override int ObjectType { get; set; } = -5;
	public override int Num { get; set; } = 0;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();

		Create create = (Create)eventBase.Items[0].Loader;

		result.AppendLine("{");
		if (create.Position.ObjectInfoParent != ushort.MaxValue)
		{
			result.AppendLine($"ObjectInstance* parent = nullptr;");
			result.AppendLine($"if ({GetSelector((int)create.Position.ObjectInfoParent)}->Size() > 0) {{");
			result.AppendLine($"    parent = *{GetSelector((int)create.Position.ObjectInfoParent)}->begin();");
			result.AppendLine($"}}");
		}
		var objectInfo = ExpressionConverter.GetObject(create.ObjectInfo, IsGlobal);
		result.AppendLine($"ObjectInstance* instance = CreateInstance(ObjectFactory::Instance().CreateInstance_{StringUtils.SanitizeObjectName(objectInfo.Item2)}_{objectInfo.Item1}(), {create.Position.X}, {create.Position.Y}, {create.Position.Layer}, 0, {objectInfo.Item1}, {create.Position.Angle}{(create.Position.ObjectInfoParent != ushort.MaxValue ? ", parent" : "")});");
		//add to selector
		result.AppendLine($"{GetSelector(create.ObjectInfo)}->AddInstance(instance);");
		result.AppendLine("}");

		return result.ToString();
	}
}
