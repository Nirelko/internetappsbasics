using System;

namespace Reviews.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ControllerLoginRequired : Attribute
    {
    }
}