using System.Reflection;

namespace RTAutoSprintEx {
// Utilities
	public static class Utils
	{
        /// <summary>
        /// Try to parse a bool that's either formatted as "true"/"false" or a whole number "0","1". Values above 0 are considered "truthy" and values equal or lower than zero are considered "false".
        /// </summary>
        /// <param name="input">the string to parse</param>
        /// <param name="result">the result if parsing was correct.</param>
        /// <returns>True if the string was parsed correctly. False otherwise</returns>
		internal static bool TryParseBool(string input, out bool result) {
            if(bool.TryParse(input,out result)){
                return true;
            }
            if (int.TryParse(input, out int val)){
                result = val > 0;
                return true;
            }
            return false;
        }

		internal static T GetInstanceField<T>(this object instance, string fieldName)
		{
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo field = instance.GetType().GetField(fieldName, bindingAttr);
			return (T)((object)field.GetValue(instance));
		}

		internal static void SetInstanceField<T>(this object instance, string fieldName, T value)
		{
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo field = instance.GetType().GetField(fieldName, bindingAttr);
			field.SetValue(instance, value);
		}
	}
}