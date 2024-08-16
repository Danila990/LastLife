using System.Collections.Generic;

namespace Dialogue.Services
{
	public class DiVariableStorage : Yarn.IVariableStorage
	{
		private readonly Dictionary<string, object> _variables = new Dictionary<string, object>();
		private readonly Dictionary<string, System.Type> _variableTypes = new Dictionary<string, System.Type>(); // needed for serialization
		
		public void SetValue(string variableName, string stringValue)
		{
			ValidateVariableName(variableName);

			_variables[variableName] = stringValue;
			_variableTypes[variableName] = typeof(string);
		}

		public void SetValue(string variableName, float floatValue)
		{
			ValidateVariableName(variableName);
            
			_variables[variableName] = floatValue;
			_variableTypes[variableName] = typeof(float);
		}

		public void SetValue(string variableName, bool boolValue)
		{
			ValidateVariableName(variableName);
            
			_variables[variableName] = boolValue;
			_variableTypes[variableName] = typeof(bool);
		}

		/// <summary>
		/// Retrieves a <see cref="Value"/> by name.
		/// </summary>
		/// <param name="variableName">The name of the variable to retrieve
		/// the value of. Don't forget to include the "$" at the
		/// beginning!</param>
		/// <returns>The <see cref="Value"/>. If a variable by the name of
		/// <paramref name="variableName"/> is not present, returns a value
		/// representing `null`.</returns>
		/// <exception cref="System.ArgumentException">Thrown when
		/// variableName is not a valid variable name.</exception>
		public bool TryGetValue<T>(string variableName, out T result)
		{
			ValidateVariableName(variableName);

			// If we don't have a variable with this name, return the null
			// value
			if (_variables.TryGetValue(variableName, out var resultObject) == false)
			{
				result = default(T);
				return false;
			}

			if (resultObject is T obj)
			{
				result = obj;
				return true;
			}
			else
			{
				throw new System.InvalidCastException($"Variable {variableName} exists, but is the wrong type (expected {typeof(T)}, got {resultObject.GetType()}");
			}
		}
		
		public void Clear()
		{
			_variables.Clear();
			_variableTypes.Clear();
		}
		
		
		
		private static void ValidateVariableName(string variableName)
		{
			if (variableName.StartsWith("$") == false)
			{
				throw new System.ArgumentException($"{variableName} is not a valid variable name: Variable names must start with a '$'. (Did you mean to use '${variableName}'?)");
			}
		}
	}
}