using System;
using UnityEngine;

namespace GameDevBuddies.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public const SliderFieldPosition DefaultMinFieldPosition = SliderFieldPosition.Left;
        public const SliderFieldPosition DefaultMaxFieldPosition = SliderFieldPosition.Right;

        public readonly float Min;
        public readonly float Max;
        public readonly string MaxVariableName;
        public readonly string DisplayName;
        public readonly SliderFieldPosition MinFieldPosition;
        public readonly SliderFieldPosition MaxFieldPosition;

        public MinMaxSliderAttribute(float min, float max, string maxVariableName, string displayName = null, SliderFieldPosition minFieldPosition = DefaultMinFieldPosition, SliderFieldPosition maxFieldPosition = DefaultMaxFieldPosition) : this(min, max, minFieldPosition, maxFieldPosition)
        {
            DisplayName = displayName;
            MaxVariableName = maxVariableName;
        }

        public MinMaxSliderAttribute(float min, float max, SliderFieldPosition minFieldPosition = DefaultMinFieldPosition, SliderFieldPosition maxFieldPosition = DefaultMaxFieldPosition)
        {
            Min = min;
            Max = max;
            MinFieldPosition = minFieldPosition;
            MaxFieldPosition = maxFieldPosition;
        }
    }

    public enum SliderFieldPosition
    {
        None = 0,
        Left = 1,
        Right = 2
    }
}