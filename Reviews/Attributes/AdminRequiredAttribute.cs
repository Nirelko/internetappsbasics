using System;

namespace Reviews.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AdminRequiredAttribute : Attribute
    {
    }
}