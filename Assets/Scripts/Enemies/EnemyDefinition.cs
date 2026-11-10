using UnityEngine;

namespace Vyrden.Enemies
{
    /// <summary>Tuning data for one enemy type. Movement speed lives in its CharacterMotorSettings.</summary>
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Vyrden/Enemies/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName = "Enemy";

        [Header("Health")]
        [SerializeField, Min(1f)] private float _maxHealth = 60f;

        [Header("Awareness")]
        [Tooltip("Distance at which the enemy notices a target.")]
        [SerializeField, Min(0f)] private float _detectionRange = 15f;
        [Tooltip("Distance at which the enemy gives up the chase. Keep above detection range.")]
        [SerializeField, Min(0f)] private float _loseTargetRange = 22f;
        [Tooltip("Pause between noticing a target and starting to chase.")]
        [SerializeField, Min(0f)] private float _reactionTime = 0.3f;

        [Header("Attack")]
        [SerializeField, Min(0f)] private float _attackDamage = 10f;
        [SerializeField, Min(0.1f)] private float _attackRange = 1.8f;
        [SerializeField, Min(0f)] private float _attackCooldown = 1.5f;
        [Tooltip("Telegraph time before the hit lands.")]
        [SerializeField, Min(0f)] private float _attackWindup = 0.4f;
        [Tooltip("Time the enemy stands still after an attack.")]
        [SerializeField, Min(0f)] private float _recoverDuration = 0.6f;

        [Header("Rewards")]
        [Tooltip("Experience given to whoever lands the killing blow.")]
        [SerializeField, Min(0f)] private float _experienceReward = 10f;

        [Header("Death")]
        [Tooltip("Seconds the body stays before being removed.")]
        [SerializeField, Min(0f)] private float _corpseDuration = 3f;

        public string DisplayName => _displayName;
        public float MaxHealth => _maxHealth;
        public float DetectionRange => _detectionRange;
        public float LoseTargetRange => Mathf.Max(_loseTargetRange, _detectionRange);
        public float ReactionTime => _reactionTime;
        public float AttackDamage => _attackDamage;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public float AttackWindup => _attackWindup;
        public float RecoverDuration => _recoverDuration;
        public float ExperienceReward => _experienceReward;
        public float CorpseDuration => _corpseDuration;
    }
}
