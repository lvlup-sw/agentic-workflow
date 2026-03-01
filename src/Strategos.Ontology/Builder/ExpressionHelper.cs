using System.Linq.Expressions;
using System.Reflection;

namespace Strategos.Ontology.Builder;

internal static class ExpressionHelper
{
    public static string ExtractMemberName<T, TResult>(Expression<Func<T, TResult>> expression)
    {
        var member = ExtractMemberExpression(expression.Body);
        return member.Member.Name;
    }

    public static Type ExtractMemberType<T, TResult>(Expression<Func<T, TResult>> expression)
    {
        var member = ExtractMemberExpression(expression.Body);
        return member.Member switch
        {
            PropertyInfo prop => prop.PropertyType,
            FieldInfo field => field.FieldType,
            _ => typeof(object),
        };
    }

    private static MemberExpression ExtractMemberExpression(Expression expression) =>
        expression switch
        {
            MemberExpression member => member,
            UnaryExpression { Operand: MemberExpression member } => member,
            _ => throw new ArgumentException(
                $"Expression '{expression}' does not refer to a member."),
        };
}
