using System.Text;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class CreateObjectAtAction : ActionBase
{
	public override int ObjectType { get; set; } = -5;
	public override int Num { get; set; } = 2;

	public override string Build(EventBase eventBase, ref string nextLabel, ref int orIndex, Dictionary<string, object>? parameters = null, string ifStatement = "if (")
	{
		StringBuilder result = new();

		ParamObject obj = (ParamObject)eventBase.Items[0].Loader;
		string X = ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[1].Loader, eventBase);
		string Y = ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[2].Loader, eventBase);
		string layer = ExpressionConverter.ConvertExpression((ExpressionParameter)eventBase.Items[3].Loader, eventBase);
		result.AppendLine($"CreateInstance({X}, {Y}, ({layer}) - 1, {ExpressionConverter.GetObject(obj.ObjectInfo).Item1}, 0);");
		//add to selector
		result.AppendLine($"{ExpressionConverter.GetSelector(obj.ObjectInfo)}->AddExternalInstance(ObjectInstances.back());");

		return result.ToString();
	}
}
