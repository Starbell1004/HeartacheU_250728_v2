/*using Naninovel;
using UnityEngine;

/// <summary>
/// ���ϳ뺧 ���� ó���� ���� ���� Ŭ����
/// </summary>
public static class NaninovelVariableHelper
{
    /// <summary>
    /// ���ϳ뺧 ���� ���� string���� �����ϰ� �����ɴϴ�
    /// </summary>
    /// <param name="variableName">������</param>
    /// <param name="defaultValue">�⺻�� (������ ���ų� ���� �� ��ȯ)</param>
    /// <returns>���� �� (string)</returns>
    public static string GetVariableAsString(string variableName, string defaultValue = "")
    {
        if (string.IsNullOrEmpty(variableName))
            return defaultValue;

        try
        {
            var variableManager = Engine.GetService<ICustomVariableManager>();
            if (variableManager == null)
            {
                Debug.LogWarning($"[NaninovelVariableHelper] ICustomVariableManager�� ã�� �� �����ϴ�.");
                return defaultValue;
            }

            var variableValue = variableManager.GetVariableValue(variableName);

            // CustomVariableValue�� ���� ToString()���� ��ȯ
            string stringValue = variableValue.ToString();

            // null�̳� �� ���ڿ��� ��� �⺻�� ��ȯ
            if (string.IsNullOrEmpty(stringValue))
            {
                return defaultValue;
            }

            return stringValue;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NaninovelVariableHelper] ���� '{variableName}' �������� ����: {ex.Message}");
            return defaultValue;
        }
    }

    /// <summary>
    /// ���ϳ뺧 ���� ���� int�� �����ϰ� �����ɴϴ�
    /// </summary>
    /// <param name="variableName">������</param>
    /// <param name="defaultValue">�⺻��</param>
    /// <returns>���� �� (int)</returns>
    public static int GetVariableAsInt(string variableName, int defaultValue = 0)
    {
        string stringValue = GetVariableAsString(variableName, defaultValue.ToString());

        if (int.TryParse(stringValue, out int result))
        {
            return result;
        }

        return defaultValue;
    }

    /// <summary>
    /// ���ϳ뺧 ���� ���� bool�� �����ϰ� �����ɴϴ�
    /// </summary>
    /// <param name="variableName">������</param>
    /// <param name="defaultValue">�⺻��</param>
    /// <returns>���� �� (bool)</returns>
    public static bool GetVariableAsBool(string variableName, bool defaultValue = false)
    {
        string stringValue = GetVariableAsString(variableName, defaultValue.ToString());

        // "1", "true", "True", "TRUE" ���� true�� ó��
        if (stringValue == "1" || string.Equals(stringValue, "true", System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // "0", "false", "False", "FALSE" ���� false�� ó��
        if (stringValue == "0" || string.Equals(stringValue, "false", System.StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // �Ľ��� �� ������ �⺻�� ��ȯ
        return defaultValue;
    }

    /// <summary>
    /// ���ϳ뺧 ������ string ���� �����ϰ� �����մϴ�
    /// </summary>
    /// <param name="variableName">������</param>
    /// <param name="value">������ ��</param>
    /// <returns>���� ����</returns>
    public static bool SetVariable(string variableName, string value)
    {
        if (string.IsNullOrEmpty(variableName))
        {
            Debug.LogWarning("[NaninovelVariableHelper] �������� ����ֽ��ϴ�.");
            return false;
        }

        try
        {
            var variableManager = Engine.GetService<ICustomVariableManager>();
            if (variableManager == null)
            {
                Debug.LogWarning("[NaninovelVariableHelper] ICustomVariableManager�� ã�� �� �����ϴ�.");
                return false;
            }

            variableManager.SetVariableValue(variableName, new CustomVariableValue(value ?? ""));
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NaninovelVariableHelper] ���� '{variableName}' ���� ����: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// ���ϳ뺧 ������ int ���� �����ϰ� �����մϴ�
    /// </summary>
    /// <param name="variableName">������</param>
    /// <param name="value">������ ��</param>
    /// <returns>���� ����</returns>
    public static bool SetVariable(string variableName, int value)
    {
        return SetVariable(variableName, value.ToString());
    }

    /// <summary>
    /// ���ϳ뺧 ������ bool ���� �����ϰ� �����մϴ� (1 �Ǵ� 0���� ����)
    /// </summary>
    /// <param name="variableName">������</param>
    /// <param name="value">������ ��</param>
    /// <returns>���� ����</returns>
    public static bool SetVariable(string variableName, bool value)
    {
        return SetVariable(variableName, value ? "1" : "0");
    }
}*/