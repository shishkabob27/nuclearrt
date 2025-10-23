using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class SelectMovementAction : ActionBase
{
	public override int ObjectType { get; set; } = 2;
	public override int Num { get; set; } = 13;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new StringBuilder();

		result.AppendLine($"for (ObjectIterator it(*{GetSelector(eventBase.ObjectInfo)}); !it.end(); ++it) {{");
		result.AppendLine($"    auto instance = *it;");
		result.AppendLine($"    auto commonProperties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);");

		string movementIndex = "0";
		if (eventBase.Items[0].Loader is Short shortLoader)
		{
			movementIndex = shortLoader.Value.ToString();
		}
		else
		{
			movementIndex = ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[0].Loader, eventBase);
		}

		result.AppendLine($"    commonProperties->oMovements->SetMovement({movementIndex});");
		result.AppendLine("}");
		return result.ToString();
	}
}
