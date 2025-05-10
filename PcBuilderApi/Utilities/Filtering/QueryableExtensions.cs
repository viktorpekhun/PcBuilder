using System.Linq.Expressions;
using System.Reflection;

namespace PcBuilderApi.Utilities.Filtering
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> query, ResourceParameters parameters)
        {
            return query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize);
        }

        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy, bool ascending = true)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
                return source;

            // Get property info
            var propertyInfo = typeof(T).GetProperty(orderBy,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null)
                return source;

            // Create expression parameter
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyInfo);
            var lambda = Expression.Lambda(property, parameter);

            // Create method
            var methodName = ascending ? "OrderBy" : "OrderByDescending";
            var method = typeof(Queryable).GetMethods()
                .Where(m => m.Name == methodName && m.IsGenericMethodDefinition)
                .Where(m => m.GetParameters().Length == 2)
                .Single();

            var genericMethod = method.MakeGenericMethod(typeof(T), propertyInfo.PropertyType);
            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { source, lambda });
        }

        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> source, Dictionary<string, string[]> filters)
        {
            if (filters == null || !filters.Any())
                return source;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression finalFilterExpression = null;

            foreach (var filter in filters)
            {
                // Get property info
                var propertyName = filter.Key;
                var propertyInfo = typeof(T).GetProperty(propertyName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                    continue;

                // Create property expression
                var property = Expression.Property(parameter, propertyInfo);

                // Get the actual type (handling nullable types)
                Type propertyType = propertyInfo.PropertyType;
                bool isNullable = propertyType.IsGenericType &&
                                  propertyType.GetGenericTypeDefinition() == typeof(Nullable<>);

                // Get underlying type if nullable
                Type underlyingType = isNullable ?
                    Nullable.GetUnderlyingType(propertyType) :
                    propertyType;

                // Create OR expressions for multiple values
                Expression propertyFilterExpression = null;

                foreach (var value in filter.Value)
                {
                    // Skip empty values
                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    // Convert the value to the appropriate type
                    object convertedValue;
                    try
                    {
                        convertedValue = Convert.ChangeType(value, underlyingType);
                    }
                    catch (Exception)
                    {
                        // Skip this value if conversion fails
                        continue;
                    }

                    // Create constant expression
                    Expression constant = isNullable
                        ? Expression.Constant(convertedValue, propertyType)
                        : Expression.Constant(convertedValue);

                    // Create comparison expression
                    Expression comparisonExpression;

                    if (propertyType == typeof(string))
                    {
                        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        comparisonExpression = Expression.Call(property, method, constant);
                    }
                    else
                    {
                        comparisonExpression = Expression.Equal(property, constant);
                    }

                    // Combine with OR for multiple values of the same property
                    propertyFilterExpression = propertyFilterExpression == null
                        ? comparisonExpression
                        : Expression.OrElse(propertyFilterExpression, comparisonExpression);
                }

                // If we have a valid expression for this property, combine with AND with other properties
                if (propertyFilterExpression != null)
                {
                    finalFilterExpression = finalFilterExpression == null
                        ? propertyFilterExpression
                        : Expression.AndAlso(finalFilterExpression, propertyFilterExpression);
                }
            }

            if (finalFilterExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(finalFilterExpression, parameter);
                return source.Where(lambda);
            }

            return source;
        }

        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> source, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return source;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression searchExpression = null;

            // Get all string properties
            var stringProperties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string));

            foreach (var prop in stringProperties)
            {
                var property = Expression.Property(parameter, prop);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchConstant = Expression.Constant(searchQuery);
                var containsExpression = Expression.Call(property, containsMethod, searchConstant);

                searchExpression = searchExpression == null
                    ? containsExpression
                    : Expression.OrElse(searchExpression, containsExpression);
            }

            if (searchExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
                return source.Where(lambda);
            }

            return source;
        }
    }
}
