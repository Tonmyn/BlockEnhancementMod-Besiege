using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    class DelayCollision:MonoBehaviour
    {
        public float Delay { get; set; } = 0.2f;

        void OnEnable()
        {
            Invoke("delayCollision", Delay);
        }

        void delayCollision()
        {
            GetComponent<Collider>().enabled = true;
            GetComponent<Rigidbody>().detectCollisions = true;
        }
    }
}
