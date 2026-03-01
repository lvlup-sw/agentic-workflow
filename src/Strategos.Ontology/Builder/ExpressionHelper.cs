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

    public static string ExtractPredicateString<T>(Expression<Func<T, bool>> predicate)
    {
        var body = predicate.Body.ToString();

        // Strip the parameter prefix (e.g., "p." or "p => p.")
        var paramName = predicate.Parameters[0].Name ?? "p";
        body = body.Replace($"{paramName}.", string.Empty);

        return body;
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
