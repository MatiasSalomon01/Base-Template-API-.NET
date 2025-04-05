using Domain.Abstractions.Common;
using Domain.Abstractions.Interfaces;
using Intrastructure.Abstractions.Interfaces.Pagination;
using Intrastructure.Abstractions.Models.Pagination;
using Intrastructure.Abstractions.Models.Search;
using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Intrastructure.Abstractions.Extensions;

public static class QueryableExtension
{
    private static readonly string[] dateFormats = ["dd/MM/yyyy HH:mm", "dd/MM/yyyy", "dd/MM", "dd"];
    private readonly static string[] directionOptions = ["asc", "desc"];

    public static IQueryable<TEntity> ApplyGenericFilter<TEntity>(this IQueryable<TEntity> query, Dictionary<string, string>? filters) where TEntity : class
    {
        if (filters is null || filters.Keys.Count == 0) return query;

        var parameter = Expression.Parameter(typeof(TEntity), "src");
        var propertiesNames = typeof(TEntity).GetProperties().Select(r => r.Name).ToList();

        foreach (var pair in filters)
        {
            if (typeof(IPaginatedOptions).GetProperties().Select(r => r.Name).Contains(pair.Key))
            {
                continue;
            }

            var expression = BuildNestedPropertyExpression(parameter, pair.Key, pair.Value);

            if (expression == null) continue;

            var bodyExpression = Expression.Lambda<Func<TEntity, bool>>(expression, parameter);

            query = query.Where(bodyExpression);
        }

        return query;
    }

    public static Expression? BuildNestedPropertyExpression(Expression parameter, string key, string? value = null)
    {
        int index = 1;

        try
        {
            Expression property = parameter;
            var propertyPath = key.Split('.'); // Propiedades anidadas

            foreach (var propertyName in propertyPath)
            {
                property = Expression.PropertyOrField(property, propertyName);

                // Verificar si la propiedad es una colección (excluyendo string)
                if (typeof(IEnumerable).IsAssignableFrom(property.Type) && property.Type != typeof(string))
                {
                    var elementType = property.Type.GetGenericArguments().FirstOrDefault();
                    if (elementType == null) break;

                    // Crear un parámetro para el elemento dentro de Any
                    var internalParameter = Expression.Parameter(elementType, "x");

                    var internalProperty = BuildPropertyExpression(internalParameter, key, propertyPath.Skip(index));
                    if (internalProperty == null) continue;

                    var innerExpression = BuildBodyExpression(internalProperty, value);
                    if (innerExpression == null) continue;

                    // Crear la expresión Any
                    var anyLambda = Expression.Lambda(innerExpression, internalParameter);
                    var anyMethod = typeof(Enumerable)
                        .GetMethods()
                        .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                        .MakeGenericMethod(elementType);

                    return Expression.Call(anyMethod, property, anyLambda);
                }

                // Verificar si es necesario aplicar una condición de null para propiedades de referencia
                if (property.Type.IsClass && !property.Type.IsPrimitive && property.Type != typeof(string))
                {
                    var nullCheck = Expression.NotEqual(property, Expression.Constant(null));
                    property = Expression.Condition(nullCheck, property, Expression.Default(property.Type));
                }

                index++;
            }

            return BuildBodyExpression(property, value);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public static Expression? BuildConcatenatedPropertyExpression(Expression parameter, string property1, string property2, string? value)
    {
        try
        {
            // Crear expresiones para las dos propiedades
            var firstProperty = BuildPropertyExpression(parameter, property1);
            var secondProperty = BuildPropertyExpression(parameter, property2);

            if (firstProperty == null || secondProperty == null)
                return null;

            var space = Expression.Constant(" ");
            var concatExpression = Expression.Add(
                Expression.Add(firstProperty, space, typeof(string).GetMethod("Concat", [typeof(string), typeof(string)])),
                secondProperty,
                typeof(string).GetMethod("Concat", [typeof(string), typeof(string)])
            );

            return ContainsOrEqualForStrings(concatExpression, value);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public static Expression? BuildPropertyExpression(Expression parameter, string key, IEnumerable<string>? keys = null)
    {
        try
        {
            Expression property = parameter;

            var propertyPath = keys ?? key.Split('.'); //Propiedades anidadas

            foreach (var propertyName in propertyPath)
            {
                property = Expression.PropertyOrField(property, propertyName);

                if (property.Type.IsClass && !property.Type.IsPrimitive && property.Type != typeof(string))
                {
                    var nullCheck = Expression.NotEqual(property, Expression.Constant(null));
                    property = Expression.Condition(nullCheck, property, Expression.Default(property.Type));
                }
            }

            return property;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    private static Expression? BuildBodyExpression(Expression property, string? value)
    {
        if (value == string.Empty) return null;

        //Cuando es diferente a string y es null, hacer equal directamente
        if (value is null) return Expression.Equal(property, Expression.Constant(null));

        // Cuando es de tipo string, utilizar el contains
        if (property.Type == typeof(string)) return ContainsOrEqualForStrings(property, value);

        if (value == "!") return Expression.NotEqual(property, Expression.Constant(null));

        // Cuando es un array (valores múltiples separados por comas)
        if (value.Contains(',')) return ArrayExpression(property, value);

        if (value.Contains(';') && (property.Type == typeof(DateTime) || property.Type == typeof(DateTime?)))
        {
            int count = 0;

            var dates = value.Split(';')
                .Select(x => TryParseDate(x.Trim(), dateFormats))
                .Select(date => AdjustDate(ref count, date))
                .Where(date => date.HasValue)
                .ToList();

            if (dates.Count != 0 && dates.Count > 1)
            {
                return Between(property, Expression.Constant(dates.First(), property.Type), Expression.Constant(dates.Last(), property.Type));
            }
            else
            {
                var last = dates.First()!.Value.AddDays(1).AddTicks(-1);
                return Between(property, Expression.Constant(dates.First(), property.Type), Expression.Constant(last, property.Type));
            }
        }

        ConstantExpression constant;

        try
        {
            // Verificar si es Nullable<T> y obtener el tipo subyacente si lo es
            var targetType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;
            //var targetType = property.Type;

            object? convertedValue = null;

            // Convertir el valor al tipo subyacente
            if (targetType == typeof(Guid)) convertedValue = Guid.Parse(value);
            else if (targetType == typeof(TimeSpan) || targetType == typeof(TimeSpan?)) convertedValue = TimeSpan.Parse(value);
            else if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                DateTime? finalValue = TryParseDate(value.ToString()!.Trim(), dateFormats);

                constant = Expression.Constant(finalValue, property.Type);
                var constantRight = Expression.Constant(finalValue!.Value.Date.AddDays(1).AddTicks(-1), property.Type);

                return Between(property, constant, constantRight);
            }
            else convertedValue = Convert.ChangeType(value, targetType);

            // Crear la constante, asegurando el tipo original de la propiedad
            constant = Expression.Constant(convertedValue, property.Type);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return Expression.Equal(property, Expression.Constant(value, property.Type));
    }

    public static BinaryExpression Between(
       Expression property,
       ConstantExpression left,
       ConstantExpression right)
    {
        var greaterThan = Expression.GreaterThanOrEqual(property, left);
        var lessThan = Expression.LessThanOrEqual(property, right);
        return Expression.AndAlso(greaterThan, lessThan);
    }

    private static MethodCallExpression ArrayExpression(Expression property, string value)
    {
        Type arrayType = property.Type.MakeArrayType();

        //Lista de objects
        List<object> genericList = value.Split(',')
            .Select(value => Convert.ChangeType(value.Trim(), property.Type))
            .ToList();

        //Convertir la lista de objects a una lista tipada de acuerdo a la propiedad
        Array typedArray = ConvertToArray(genericList, property.Type);

        var constantArray = Expression.Constant(typedArray, arrayType);

        return ContainsForLists(property, constantArray);
    }

    private static MethodCallExpression ContainsForLists(Expression property, ConstantExpression constant)
    {
        var containsMethod = typeof(Enumerable)
            .GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(property.Type);

        if (property.Type == typeof(string))
        {
            var toLower = Expression.Call(property, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
            return Expression.Call(containsMethod, constant, toLower);
        }

        return Expression.Call(containsMethod, constant, property);
    }

    private static Expression ContainsOrEqualForStrings(Expression property, object? value)
    {
        if (value is null) return Expression.Equal(property, Expression.Constant(value));

        // Cuando es un array (valores múltiples separados por comas)
        if (value.ToString()!.Contains(',')) return ArrayExpression(property, value.ToString()!);

        var constant = Expression.Constant(value.ToString()!.ToLower());
        var toLower = Expression.Call(property, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
        var contains = typeof(string).GetMethod("Contains", [typeof(string)])!;

        return Expression.Call(toLower, contains, constant);
    }

    private static Array ConvertToArray(IList<object> list, Type elementType)
    {
        var array = Array.CreateInstance(elementType, list.Count);

        for (int i = 0; i < list.Count; i++)
        {
            var value = list[i];

            if (value is string)
            {
                value = value.ToString()?.ToLower();
            }

            array.SetValue(value, i);
        }

        return array;
    }

    private static DateTime? TryParseDate(string input, string[] formats)
    {
        if (DateTime.TryParseExact(input, formats, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var parsedDate))
        {
            return parsedDate;
        }

        return null;
    }

    private static DateTime? AdjustDate(ref int count, DateTime? date)
    {
        if (date.HasValue && count == 1)
        {
            date = date.Value.TimeOfDay == TimeSpan.Zero
                ? date.Value.AddDays(1).AddTicks(-1)
                : date.Value.AddSeconds(59);
        }

        count++;
        return date;
    }

    public static IOrderedQueryable<T> ApplySort<T>(this IQueryable<T> source, string? sortBy, string? direction)
    {
        if (!directionOptions.Contains(direction)) direction = "desc";
        if (string.IsNullOrWhiteSpace(sortBy)) sortBy = nameof(Entity.Id);

        var parameter = Expression.Parameter(typeof(T), "src");

        // Construye la expresión para la propiedad a ordenar
        var property = BuildPropertyExpression(parameter, sortBy);
        if (property is null) return (source as IOrderedQueryable<T>)!;

        var bodyExpression = Expression.Lambda(property, parameter);

        // Determina si usar OrderBy o OrderByDescending
        var method = direction == "asc" ? "OrderBy" : "OrderByDescending";

        // Obtén el método genérico adecuado
        var result = typeof(Queryable)
            .GetMethods()
            .First(m => m.Name == method && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.Type)
            .Invoke(null, [source, bodyExpression]);

        return (IOrderedQueryable<T>)result!;
    }

    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, IPaginated request) where T : class
    {
        return query.Skip(request.PageNumber - 1 * request.PageSize).Take(request.PageSize);
    }

    public static PaginatedResult<T> ToPaginated<T>(this IEnumerable<T> list, int count, IPaginated paginated, bool hasNext = false, bool hasPrevious = false)
    {
        return new PaginatedResult<T>(count, hasNext, hasPrevious, paginated.PageNumber, paginated.PageSize, list.ToList());
    }

    public static IQueryable<T> GeneralSearch<T>(this IQueryable<T> query, GeneralSearchOptions? search) where T : class
    {
        if (string.IsNullOrWhiteSpace(search?.Value)) return query;

        if (search.Properties == null && typeof(IGeneralSearchBy).IsAssignableFrom(typeof(T)))
        {
            var propInfo = typeof(T).GetProperty(nameof(IGeneralSearchBy.SearchByProperties), BindingFlags.Public | BindingFlags.Static);

            if (propInfo?.GetValue(null) is List<string> propsFromEntity)
            {
                search.Properties = propsFromEntity;
            }
        }

        var parameter = Expression.Parameter(typeof(T), "src");

        var expression = Search<T>(parameter, search.Value.Trim(), search.Properties);

        if (expression is null) return query;

        var lambda = Expression.Lambda<Func<T, bool>>(expression, parameter);

        return query.Where(lambda);
    }

    private static Expression? Search<T>(ParameterExpression parameter, string? value, List<string>? propertiesNames = null) where T : class
    {
        propertiesNames ??= [];

        Expression? orExpression = null;

        foreach (var property in propertiesNames)
        {
            Expression? propertyExpression = null;

            // Verificar si es una concatenación de dos propiedades
            if (property.Contains(","))
            {
                var properties = property.Split(',');
                if (properties.Length == 2)
                {
                    propertyExpression = BuildConcatenatedPropertyExpression(parameter, properties[0].Trim(), properties[1].Trim(), value);
                }
            }
            else
            {
                propertyExpression = BuildNestedPropertyExpression(parameter, property, value);
            }

            if (propertyExpression is null) continue;

            orExpression = orExpression == null
                ? propertyExpression
                : Expression.OrElse(orExpression, propertyExpression!);
        }

        return orExpression;
    }

    public static IQueryable<T> WhereNotNull<T>(this IQueryable<T> query, object? obj, Expression<Func<T, bool>> expression)
    {
        return obj is not null ? query.Where(expression) : query;
    }
}
