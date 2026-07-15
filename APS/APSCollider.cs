using GHPC.Utility;
using GHPC;
using System.Linq;
using UnityEngine;

namespace ActiveProtectionSystem
{
    internal class APSCollider : MonoBehaviour
    {
        public IUnit parent_unit;
        private APSLauncher[] assigned_launchers;
        public APSSchema schema;

        void Awake()
        {
            parent_unit = this.GetComponentInParent<LateFollow>().ParentUnit;
        }

        public void Init(APSLauncher[] launchers)
        {
            assigned_launchers = launchers;
            foreach (APSLauncher launcher in assigned_launchers)
            {
                launcher.parent_unit = parent_unit;
            }
        }

        public bool TryFireProjectile(Vector3 pos)
        {
            APSLauncher launcher = assigned_launchers.Where(o => !o.IsEmpty).FirstOrDefault();

            if (launcher == null) return false;

            launcher.FireProjectile(pos);

            return true;
        }
    }
}
