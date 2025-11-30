using UnityEngine;

[CreateAssetMenu(fileName = "new Skill", menuName = "Scriptable Objects/SkillSO")]
public class SkillSO : ScriptableObject
{
    public string skillId;
    public string skillName;
    public TargetMode targetMode;
    public GameObject onSelfVFX;
    public GameObject onTargetVFX;
    public GameObject otherVFX;
    public AudioClip sfx;
    public int Damage;
    public int Shield;
    public int Heal;
}
