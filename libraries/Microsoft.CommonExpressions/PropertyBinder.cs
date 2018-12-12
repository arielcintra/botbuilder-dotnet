﻿using System;
using System.Collections.Generic;

namespace Microsoft.Expressions
{
    /// <summary>
    /// Get the value of a property of an object
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public delegate object GetValueDelegate(object instance, string key);

    /// <summary>
    /// Look up identifier binding from environment scope
    /// </summary>
    public static class PropertyBinder
    {
        /// <summary>
        /// Automatically use reflection or dictionary based on instance
        /// </summary>
        public static GetValueDelegate Auto = (object instance, string property) =>
        {
            if (instance is IDictionary<string, object>)
            {
                return Dictionary(instance, property);
            }
            else if (instance.GetType().IsArray)
            {
                if (property.StartsWith("[") && property.EndsWith("]"))
                {
                    return null; // return ((Array)instance)[int.Parse(property.Trim('[', ']'))];
                }
            }
            return Reflection(instance, property);
        };

        /// <summary>
        /// Use reflection to bind to properties of instance object
        /// </summary>
        public static GetValueDelegate Reflection = (object instance, string property) => instance.GetType().GetProperty(property).GetValue(instance);

        /// <summary>
        /// Use IDictionary<string, object> to get acces to properties of instance object</string>
        /// </summary>
        public static GetValueDelegate Dictionary = (object instance, string property) => ((IDictionary<string, object>)instance)[property];
    }
}
