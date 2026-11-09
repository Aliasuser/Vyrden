using System.Collections.Generic;
using UnityEngine;

namespace Vyrden.Combat
{
    /// <summary>
    /// Marks a character as belonging to a team and keeps a registry of active members,
    /// so AI can find targets without scene-wide searches. Supports several players later.
    /// </summary>
    public class TeamMember : MonoBehaviour
    {
        private static readonly List<TeamMember> ActiveMembers = new List<TeamMember>();

        [SerializeField] private Team _team = Team.Neutral;

        private Health _health;

        public static IReadOnlyList<TeamMember> Active => ActiveMembers;

        public Team Team => _team;
        public Health Health => _health;
        public bool IsAlive => _health == null || _health.IsAlive;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            ActiveMembers.Add(this);
        }

        private void OnDisable()
        {
            ActiveMembers.Remove(this);
        }

        public bool IsHostileTo(Team other)
        {
            return _team != Team.Neutral && other != Team.Neutral && _team != other;
        }

        /// <summary>Closest living member hostile to <paramref name="team"/> within range, or null.</summary>
        public static TeamMember FindClosestHostile(Team team, Vector3 position, float range)
        {
            TeamMember closest = null;
            float closestSqr = range * range;

            for (int i = 0; i < ActiveMembers.Count; i++)
            {
                TeamMember member = ActiveMembers[i];
                if (!member.IsAlive || !member.IsHostileTo(team))
                {
                    continue;
                }

                float sqr = (member.transform.position - position).sqrMagnitude;
                if (sqr <= closestSqr)
                {
                    closestSqr = sqr;
                    closest = member;
                }
            }

            return closest;
        }

        // Keeps the registry clean when domain reload is disabled for faster Play mode.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            ActiveMembers.Clear();
        }
    }
}
