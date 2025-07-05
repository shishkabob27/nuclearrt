using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class SetAlterableTextAction : ActionBase
{
	public override int ObjectType { get; set; } = 3;
	public override int Num { get; set; } = 88;

	public override string Build(EventBase eventBase, Dictionary<string, string>? parameters = null, string ifStatement = "if (", string nextLabel = "")
	{
		StringBuilder result = new StringBuilder();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"  auto instance = *it;");
		result.AppendLine($"  auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");
		result.AppendLine($"  auto paragraphs = std::dynamic_pointer_cast<ObjectParagraphs>(commonProperties->oParagraphs);");
		result.AppendLine($"  paragraphs->SetAlterableText({ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase)});");
		result.AppendLine("}");

		return result.ToString();
	}
}
