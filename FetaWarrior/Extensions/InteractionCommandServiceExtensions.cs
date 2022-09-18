using Discord.Interactions;
using Garyon.Reflection;
using System;
using System.Linq;
using System.Reflection;

namespace FetaWarrior.Extensions;

public static class InteractionServiceExtensions
{
    private static readonly Type genericTypeConverter = typeof(TypeConverter<>);

    public static void AddTypeConverter<TObject, TConverter>(this InteractionService service)
        where TConverter : TypeConverter<TObject>, new()
    {
        service.AddTypeConverter<TObject>(new TConverter());
    }
    public static void AddTypeConverters(this InteractionService service, Assembly assembly)
    {
        var typeReaders = assembly.GetTypes().Where(t => t.Inherits<TypeConverter>());
        foreach (var t in typeReaders)
        {
            var typeReaderBaseType = t;

            while (typeReaderBaseType.GetGenericTypeDefinitionOrSame() != genericTypeConverter)
                typeReaderBaseType = typeReaderBaseType.BaseType;

            service.AddTypeConverter(typeReaderBaseType.GenericTypeArguments[0], t.GetConstructor(Type.EmptyTypes).Invoke(null) as TypeConverter);
        }
    }
}
