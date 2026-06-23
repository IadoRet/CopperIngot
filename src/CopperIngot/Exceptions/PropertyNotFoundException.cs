using System.Reflection;

namespace CopperIngot.Exceptions;

public class PropertyNotFoundException(string propertyName, MemberInfo type) 
    : Exception($"Property {propertyName} of type {type.Name} was not found");