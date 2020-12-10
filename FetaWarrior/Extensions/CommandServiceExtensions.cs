using Discord.Commands;
using FetaWarrior.DiscordFunctionality;
using FetaWarrior.DiscordFunctionality.TypeReaders;
using System;
using System.Linq;
using System.Reflection;

namespace FetaWarrior.Extensions
{
    public static class CommandServiceExtensions
    {
        public static void AddTypeReader<TObject, TReader>(this CommandService service)
            where TReader : TypeReader<TObject>, new()
        {
            service.AddTypeReader<TObject>(new TReader());
        }
        public static void AddTypeReaders(this CommandService service, Assembly assembly)
        {
            var typeReaders = assembly.GetTypes().Where(t => t.GetCustomAttribute<MandatoryTypeReaderAttribute>() != null);
            foreach (var t in typeReaders)
            {
                var typeReaderBaseType = t;
                while (!typeReaderBaseType.IsGenericType || typeReaderBaseType.GetGenericTypeDefinition() != typeof(TypeReader<>))
                    typeReaderBaseType = typeReaderBaseType.BaseType;

                service.AddTypeReader(typeReaderBaseType.GenericTypeArguments[0], t.GetConstructor(Type.EmptyTypes).Invoke(null) as TypeReader);
            }
        }
    }
}
