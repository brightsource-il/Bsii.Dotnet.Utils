using System;
using System.Collections.Generic;
using System.Linq;
using FastMember;

namespace Bsii.Dotnet.Utils.Reflection
{
    public static class ObjectFiller
    {
        public delegate object MemberValueResolver(string propertyPath, Type objectType);

        public static void FillObject(object instance, MemberValueResolver valueResolver,
            ICollection<string> propertyPaths, string propertyPathSeparator = ".")
        {
            var rootTypeAccessor = TypeAccessor.Create(instance.GetType(), false);
            foreach (var property in propertyPaths)
            {
                var propertyPath = property.Split(new[] { propertyPathSeparator }, StringSplitOptions.RemoveEmptyEntries);
                var accessor = rootTypeAccessor;
                var currentValue = instance;
                foreach (var path in propertyPath.Take(propertyPath.Length - 1))
                {
                    if (accessor[currentValue, path] == null)
                    {
                        var member = accessor.GetMembers().First(m => m.Name == path);
                        accessor[currentValue, path] = Activator.CreateInstance(member.Type);

                    }
                    currentValue = accessor[currentValue, path];
                    accessor = TypeAccessor.Create(currentValue.GetType(), false);
                }
                accessor = TypeAccessor.Create(currentValue.GetType(), false);
                {
                    var memberInfo = accessor.GetMembers().First(m => m.Name == propertyPath.Last());
                    accessor[currentValue, memberInfo.Name] = valueResolver(property, memberInfo.Type);
                }
            }
        }
    }
}
