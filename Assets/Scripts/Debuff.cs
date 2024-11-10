using UnityEngine;

public abstract class Debuff : ScriptableObject
{
    public string debuffName;
    public string debuffDescription;
    public float duration;

    public abstract void Initialize(GameObject target);
    public abstract void ApplyEffect(GameObject target);
    public abstract void UpdateEffect(GameObject target);
    public abstract void RemoveEffect(GameObject target);



}
