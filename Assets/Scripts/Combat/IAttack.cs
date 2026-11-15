using System;
using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// An attack an AI can use: range, cooldown and a wind-up before it resolves. The AI only decides when
    /// to call <see cref="TryStartAttack"/>; how the attack hits (melee, projectile...) is up to the
    /// implementation. Implemented by the 3D <see cref="MeleeAttack"/> and the 2D attacks.
    /// </summary>
    public interface IAttack
    {
        /// <summary>Raised when the wind-up starts.</summary>
        event Action AttackStarted;

        /// <summary>Raised when the attack resolves or is cancelled. Arg: whether it landed (dealt damage or fired).</summary>
        event Action<bool> AttackFinished;

        float Range { get; }
        bool IsAttacking { get; }
        bool IsReady { get; }

        /// <summary>True if the target could be attacked from here now (range, line of sight), ignoring cooldown.</summary>
        bool CanReach(Transform target);

        /// <summary>Starts the wind-up if ready, alive and the target is reachable.</summary>
        bool TryStartAttack(Transform target);

        void Cancel();

        /// <summary>Applies attack stats from data (e.g. an enemy definition).</summary>
        void Configure(float damage, float range, float cooldown, float windup);
    }
}
