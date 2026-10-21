using System.Collections.Generic;
using UnityEngine;

namespace AnglerVillage.UI
{
    // Modal UI owns input blocks; movement and fishing remain separate systems.
    public static class GameInputBlock
    {
        private static readonly HashSet<Object> Owners = new HashSet<Object>();
        public static bool IsBlocked
        {
            get
            {
                Owners.RemoveWhere(owner => owner == null ||
                    (owner is Behaviour behaviour && !behaviour.isActiveAndEnabled));
                return Owners.Count > 0;
            }
        }
        public static void Set(Object owner, bool blocked)
        {
            if (owner == null) return;
            if (blocked) Owners.Add(owner); else Owners.Remove(owner);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => Owners.Clear();
    }
}
