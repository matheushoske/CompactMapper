using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class CompactMapperExtension
{
    private static readonly Dictionary<(Type source, Type destination), Delegate> _customMappingActions = new Dictionary<(Type source, Type destination), Delegate>();

    public static void AddCustomMapping<TSource, TDestination>(Action<TSource, TDestination> mappingAction)
    {
        _customMappingActions[(typeof(TSource), typeof(TDestination))] = mappingAction;
    }

    public static TDestination MapTo<TDestination>(
        this object source,
        bool recursive = true,
        Func<string, object, object> valueTransformer = null)
        where TDestination : new()
    {
        if (source == null) return default;

        var method = typeof(CompactMapperExtension)
            .GetMethod(nameof(InternalMapTo), BindingFlags.Static | BindingFlags.NonPublic)
            .MakeGenericMethod(source.GetType(), typeof(TDestination));

        return (TDestination)method.Invoke(null, new object[] { source, recursive, valueTransformer });
    }

    private static TDestination InternalMapTo<TSource, TDestination>(
        this TSource source,
        bool recursive = true,
        Func<string, object, object> valueTransformer = null)
        where TDestination : new()
    {
        if (source == null) return default;

        var destination = new TDestination();
        var sourceType = typeof(TSource);
        var destType = typeof(TDestination);

        // Custom mapping  
        if (_customMappingActions.TryGetValue((sourceType, destType), out var customDelegate))
        {
            var customAction = (Action<TSource, TDestination>)customDelegate;
            customAction(source, destination);
        }

        // Default property mapping  
        foreach (var destProp in destType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!destProp.CanWrite) continue;

            var sourceProp = sourceType.GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (sourceProp == null || !sourceProp.CanRead) continue;

            var sourceValue = sourceProp.GetValue(source);
            if (sourceValue == null)
            {
                destProp.SetValue(destination, null);
                continue;
            }

            object finalValue = null;

            try
            {
                var destTypeProp = destProp.PropertyType;
                var sourceTypeProp = sourceProp.PropertyType;

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(destTypeProp) && destTypeProp != typeof(string))
                {
                    var sourceElementType = GetCollectionElementType(sourceTypeProp);
                    var destElementType = GetCollectionElementType(destTypeProp);

                    if (sourceElementType != null && destElementType != null && sourceValue is System.Collections.IEnumerable sourceEnumerable)
                    {
                        var listType = typeof(List<>).MakeGenericType(destElementType);
                        var destinationList = (System.Collections.IList)Activator.CreateInstance(listType);

                        foreach (var item in sourceEnumerable)
                        {
                            if (item == null)
                            {
                                destinationList.Add(null);
                                continue;
                            }

                            var internalMapMethod = typeof(CompactMapperExtension)
                                .GetMethod(nameof(InternalMapTo), BindingFlags.Static | BindingFlags.NonPublic)
                                .MakeGenericMethod(item.GetType(), destElementType);

                            var mappedItem = internalMapMethod.Invoke(null, new object[] { item, recursive, valueTransformer });
                            destinationList.Add(mappedItem);
                        }

                        if (destTypeProp.IsArray)
                        {
                            var toArrayMethod = listType.GetMethod("ToArray");
                            finalValue = toArrayMethod.Invoke(destinationList, null);
                        }
                        else
                        {
                            finalValue = destinationList;
                        }
                    }
                    else
                    {
                        finalValue = sourceValue; // fallback  
                    }
                }
                else if (recursive && IsComplexType(destTypeProp) && IsComplexType(sourceTypeProp))
                {
                    var internalMapMethod = typeof(CompactMapperExtension)
                        .GetMethod(nameof(InternalMapTo), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(sourceTypeProp, destTypeProp);

                    finalValue = internalMapMethod.Invoke(null, new object[] { sourceValue, recursive, valueTransformer });
                }
                else if (IsNullableEnum(destTypeProp))
                {
                    finalValue = Enum.Parse(Nullable.GetUnderlyingType(destTypeProp), sourceValue.ToString());
                }
                else if (destTypeProp.IsEnum)
                {
                    finalValue = Enum.Parse(destTypeProp, sourceValue.ToString());
                }
                else if (IsNullableType(destTypeProp))
                {
                    finalValue = Convert.ChangeType(sourceValue, Nullable.GetUnderlyingType(destTypeProp));
                }
                else
                {
                    finalValue = Convert.ChangeType(sourceValue, destTypeProp);
                }

                if (valueTransformer != null)
                {
                    finalValue = valueTransformer(destProp.Name, finalValue);
                }

                destProp.SetValue(destination, finalValue);
            }
            catch
            {
                // Ignore mapping errors  
                continue;
            }
        }

        return destination;
    }

    private static Type GetCollectionElementType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GetGenericArguments()[0];

        var ienum = type.GetInterfaces()
            .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return ienum?.GetGenericArguments()[0];
    }

    private static bool IsNullableType(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

    private static bool IsNullableEnum(Type type) =>
        IsNullableType(type) && Nullable.GetUnderlyingType(type).IsEnum;

    private static bool IsComplexType(Type type) =>
        type.IsClass && type != typeof(string);
}
