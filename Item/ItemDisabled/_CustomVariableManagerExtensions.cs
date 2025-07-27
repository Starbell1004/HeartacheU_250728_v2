/*using Naninovel;
using System.Reflection;

public static class CustomVariableManagerExtensions
{
    private static FieldInfo variablesField;

    /// <summary>
    /// Ŀ���� ������ �����ϴ��� Ȯ���մϴ�.
    /// </summary>
    public static bool VariableExists(this ICustomVariableManager variableManager, string name)
    {
        if (variableManager == null || string.IsNullOrEmpty(name))
            return false;

        try
        {
            // Reflection�� ����Ͽ� ���� variables ��ųʸ��� ����
            if (variablesField == null)
            {
                var type = variableManager.GetType();
                variablesField = type.GetField("variables", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (variablesField != null)
            {
                var variables = variablesField.GetValue(variableManager) as System.Collections.Generic.Dictionary<string, CustomVariableValue>;
                return variables != null && variables.ContainsKey(name);
            }

            // Reflection�� �����ϸ� try-catch�� Ȯ��
            variableManager.GetVariableValue(name);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// ���� ���� �����ϰ� �����ɴϴ�. ������ ������ �⺻���� ��ȯ�մϴ�.
    /// </summary>
    public static string GetVariableValueSafe(this ICustomVariableManager variableManager, string name, string defaultValue = "")
    {
        if (!variableManager.VariableExists(name))
            return defaultValue;

        try
        {
            return variableManager.GetVariableValue(name).ToString();
        }
        catch
        {
            return defaultValue;
        }
    }
}*/