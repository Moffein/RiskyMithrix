using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RiskyMithrix.Components
{
    public class WeaponSlamRateStorangeComponent : MonoBehaviour
    {
        private float rate = -1f;

        public void StoreOrigRate(float newRate)
        {
            rate = newRate;
        }

        public float GetOrigRate()
        {
            return rate;
        }

        public void ResetRate()
        {
            rate = -1f;
        }
    }
}
