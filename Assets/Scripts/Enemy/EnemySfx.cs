using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySfx : MonoBehaviour
{
    [SerializeField] private float runSpeedThreshold = 3f;

    [Header("Movement sources")]
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private EnemyCombatController enemyCombatController;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSourceLower;
    [SerializeField] private AudioSource audioSourceUpper;

    [Header("Step sounds")]
    [SerializeField] private AudioClip[] StepSounds;          // EnemyMovement
    [SerializeField] private AudioClip[] combatStepSounds;    // EnemyCombatController

    [Header("Attack sounds")]
    [SerializeField] private AudioClip[] attackSounds;

    // Animation Event
    public void StepWalk()
    {
        if (GetCurrentSpeed() >= runSpeedThreshold)
            return;

        AudioClip[] clips = GetCurrentStepSounds();
        AudioPlay.PlaySound(audioSourceLower, clips);
    }

    // Animation Event
    public void StepRun()
    {
        if (GetCurrentSpeed() < runSpeedThreshold)
            return;

        AudioClip[] clips = GetCurrentStepSounds();
        AudioPlay.PlaySound(audioSourceLower, clips);
    }

    // Animation Event
    public void PlayAttackSound()
    {
        AudioPlay.PlaySound(audioSourceUpper, attackSounds);
    }

    private float GetCurrentSpeed()
    {
        if (enemyCombatController != null && enemyCombatController.enabled)
            return enemyCombatController.combatMoveSpeed;

        if (enemyMovement != null && enemyMovement.enabled)
            return enemyMovement.speed;

        return 0f;
    }

    private AudioClip[] GetCurrentStepSounds()
    {
        if (enemyCombatController != null && enemyCombatController.enabled)
            return combatStepSounds;

        return StepSounds;
    }
}