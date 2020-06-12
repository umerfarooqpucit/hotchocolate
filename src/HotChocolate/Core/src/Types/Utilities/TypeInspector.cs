using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace HotChocolate.Utilities
{
    public class TypeInspector : ITypeInfoFactory
    {
        private readonly ITypeInfoFactory[] _factories =
        {
            new NamedTypeInfoFactory(),
            new DotNetTypeInfoFactory()
        };

        private readonly ConcurrentDictionary<Type, TypeInfo> _cache =
            new ConcurrentDictionary<Type, TypeInfo>();

        public bool TryCreate(Type type, [NotNullWhen(true)]out TypeInfo typeInfo)
        {
            if (!_cache.TryGetValue(type, out typeInfo))
            {
                if (!TryCreateInternal(type, out typeInfo))
                {
                    typeInfo = default;
                    return false;
                }
                _cache.TryAdd(type, typeInfo);
            }
            return true;
        }

        private bool TryCreateInternal(Type type, out TypeInfo typeInfo)
        {
            foreach (ITypeInfoFactory factory in _factories)
            {
                if (factory.TryCreate(type, out typeInfo))
                {
                    return true;
                }
            }

            typeInfo = default;
            return false;
        }

        public void Clear() => _cache.Clear();

        public static TypeInspector Default { get; } = new TypeInspector();
    }
}
