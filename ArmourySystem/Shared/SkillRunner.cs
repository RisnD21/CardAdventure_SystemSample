using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillRunner : MonoBehaviour
{
    public IEnumerator PlaySkill(SkillSO skill, ActorBase caster, IList<ActorBase> allies, IList<ActorBase> foes, ActorBase target = null)
    {
        if (skill == null || caster == null) yield break;

        string msg;
        if (caster is MonsterActor mon)
        {
            msg = $"{mon.monsterName} use skill: {skill.skillName}";
        } else msg = $"Player use skill: {skill.skillName}";
        SimplePrinter.Instance.Show(msg);
        
        var targets = SelectTargets(skill.targetMode, caster, allies, foes, target);

        if (skill.Shield > 0)
        {
            int shield = skill.Shield;
            foreach (var t in targets) t.GainShield(shield);            
        }

        if (skill.Heal > 0)
        {
            int heal = skill.Heal;
            foreach (var t in targets) t.Heal(heal);
        }

        if (skill.Damage > 0)
        {
            int dmg = skill.Damage + caster.BaseAttack;
            foreach (var t in targets) t.TakeDamage(dmg);
        }

        // VFX / SFX, change GetInstanceFrom pool if needed
        if (skill.onSelfVFX) Instantiate(skill.onSelfVFX, caster.Position, Quaternion.identity);
        if (skill.onTargetVFX) foreach (var t in targets) Instantiate(skill.onTargetVFX, t.Position, Quaternion.identity);
        if (skill.sfx) AudioSource.PlayClipAtPoint(skill.sfx, caster.Position);

        caster.PlayAttackAnim();
        
        yield break;
    }

    List<ActorBase> SelectTargets(TargetMode mode, ActorBase caster, IList<ActorBase> allies,IList<ActorBase> foes, ActorBase target = null)
    {
        if(target == null) target = foes[0];
        switch (mode)
        {
            case TargetMode.AllAllies: return new(allies.Where(t => t.IsAlive));
            case TargetMode.Self: return new() {caster};
            case TargetMode.AllEnemies: return new(foes.Where(t => t.IsAlive));
            case TargetMode.SingleEnemy:
            case TargetMode.Single: return new() {target};
            default:
                return (foes != null && foes.Count > 0) ? new() { foes[0] } : new();
        }
    }
}
