using System;
using UnityEngine;

namespace GameDevBuddies.CustomAttributes
{
    /// <summary>
    /// Attribute that will show/hide the field if the value of the given property/field/method is true/false.
    /// </summary>
    public class ShowIfAttribute : PropertyAttribute
    {
        /// <summary>
        /// Name of the method/property.
        /// </summary>
        public string MemberName { get; private set; }
        public object Value { get; private set; }

        /// <summary>
        /// Constructor for using a boolean field name
        /// </summary>
        /// <param name="memberName">Name of the member. Use nameof(Method or Boolean property/field).</param>
        public ShowIfAttribute(string memberName)
        {
            MemberName = memberName;
        }

        /// <summary>
        /// Constructor for using a boolean field name
        /// </summary>
        /// <param name="memberName">Name of the member. Use nameof(Method or Boolean property/field).</param>
        /// <param name="enumValue">Required value.</param>
        public ShowIfAttribute(string memberName, object value)
        {
            MemberName = memberName;
            Value = value;
        }
    }
}
