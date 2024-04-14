using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.Math;
using GHPC.Utility;
using GHPC.Weapons;
using TMPro;
using UnityEngine;

namespace PactIncreasedLethality
{
    public class UVBU : MonoBehaviour
    {
        public FireControlSystem fcs;
        public GameObject readout_go;
        public TextMeshProUGUI readout;
        float cd = 0f;

        void LateUpdate()
        {
            bool button = InputUtil.MainPlayer.GetButtonDown("Lase");

            if (cd > 0f && readout.IsActive())
            {
                cd -= Time.deltaTime;
            }

            if (cd <= 0f && readout.IsActive())
            {
                cd = 0f;
                readout_go.SetActive(false);
            }

            if (button)
            {
                cd = 2f;

                readout_go.SetActive(true);

                float flight_time = fcs._bc.GetFlightTime(fcs._bcAmmo, fcs._currentRange);       
                float x = fcs._averageTraverseRate.x * 0.017453292f * fcs._currentRange * flight_time * -10f;
                x /= (1f - fcs.transform.localPosition.x) * Mathf.Clamp(fcs._currentRange / 1500f, 0f, 1f);
                string sign = Math.Sign(x) > 0 ? "+" : "-";
                if ((int)x == 0) sign = "";

                int lead = Math.Abs(((int)MathUtil.RoundFloatToMultipleOf(x, 5)));

                if (lead > 999) lead = 999;

                readout.text = sign + lead.ToString("000");
            }
        }
    }
}
