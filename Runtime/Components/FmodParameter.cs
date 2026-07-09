using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Liga um parametro BetterFMOD a uma fonte de valor sem codigo de gameplay customizado.
/// </summary>
public class FmodParameter : MonoBehaviour
{
    [SerializeField] private string eventId;
    [SerializeField] private string parameter;
    [SerializeField] private bool globalParameter;
    [SerializeField] private FmodParameterSource source;
    [SerializeField] private float floatValue;
    [SerializeField] private int intValue;
    [SerializeField] private Slider slider;
    [SerializeField] private Rigidbody rigidbodySource;
    [SerializeField] private Animator animator;
    [SerializeField] private string animatorParameter;
    [SerializeField] private Transform distanceFrom;
    [SerializeField] private Transform distanceTo;
    [SerializeField] private Component componentSource;
    [SerializeField] private string memberName;
    [SerializeField] private float multiplier = 1f;

    private void Update()
    {
        Apply();
    }

    /// <summary>
    /// Aplica o valor atual da fonte ao parametro BetterFMOD configurado.
    /// </summary>
    public void Apply()
    {
        if (string.IsNullOrWhiteSpace(parameter))
            return;

        float value = ReadValue() * multiplier;

        if (globalParameter)
            FmodB8.SetGlobalParameter(parameter, value);
        else
            FmodB8.SetParameter(eventId, parameter, value);
    }

    /// <summary>
    /// Define o valor float manual usado por este vinculo de parametro.
    /// </summary>
    public void SetFloat(float value)
    {
        floatValue = value;
        Apply();
    }

    /// <summary>
    /// Define o valor inteiro manual usado por este vinculo de parametro.
    /// </summary>
    public void SetInt(int value)
    {
        intValue = value;
        Apply();
    }

    private float ReadValue()
    {
        switch (source)
        {
            case FmodParameterSource.Int:
                return intValue;
            case FmodParameterSource.Slider:
                return slider == null ? 0f : slider.value;
            case FmodParameterSource.RigidbodySpeed:
#if UNITY_6000_0_OR_NEWER
                return rigidbodySource == null ? 0f : rigidbodySource.linearVelocity.magnitude;
#else
                return rigidbodySource == null ? 0f : rigidbodySource.velocity.magnitude;
#endif
            case FmodParameterSource.Animator:
                return animator == null || string.IsNullOrWhiteSpace(animatorParameter) ? 0f : animator.GetFloat(animatorParameter);
            case FmodParameterSource.Distance:
                return distanceFrom == null || distanceTo == null ? 0f : Vector3.Distance(distanceFrom.position, distanceTo.position);
            case FmodParameterSource.ComponentMember:
                return ReadComponentMember();
            default:
                return floatValue;
        }
    }

    private float ReadComponentMember()
    {
        if (componentSource == null || string.IsNullOrWhiteSpace(memberName))
            return 0f;

        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo field = componentSource.GetType().GetField(memberName, flags);

        if (field != null)
            return ConvertToFloat(field.GetValue(componentSource));

        PropertyInfo property = componentSource.GetType().GetProperty(memberName, flags);

        if (property != null)
            return ConvertToFloat(property.GetValue(componentSource));

        return 0f;
    }

    private float ConvertToFloat(object value)
    {
        return value switch
        {
            float floatResult => floatResult,
            int intResult => intResult,
            double doubleResult => (float)doubleResult,
            bool boolResult => boolResult ? 1f : 0f,
            _ => 0f
        };
    }
}

/// <summary>
/// Define uma fonte de valor para vinculo de parametros BetterFMOD.
/// </summary>
public enum FmodParameterSource
{
    Float,
    Int,
    Slider,
    RigidbodySpeed,
    Animator,
    Distance,
    ComponentMember
}
