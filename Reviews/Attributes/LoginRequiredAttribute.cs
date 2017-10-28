using System;

namespace Reviews.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LoginRequiredAttribute : Attribute
    {
    }
}