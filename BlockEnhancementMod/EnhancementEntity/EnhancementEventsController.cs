using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using Modding.Levels;
using UnityEngine;

namespace BlockEnhancementMod
{
    public class EnhancementEventsController:SingleInstance<EnhancementEventsController>
    {
        public override string Name { get; } = "Events Controller";

        List<Entity> RestoreEntities = new List<Entity>();

        public void OnGroup(LogicChain logicChain,IDictionary<string,EventProperty> propertise)
        {
            RestoreEntities.Clear();

            Transform Parent = logicChain.Entity.GameObject.transform.parent;

            List<Entity> entities = ((EventProperty.Picker)propertise["Picker"]).Entities;
            foreach (var entity in entities)
            {
                if (entity.InternalObject.isStatic)
                {
                    entity.GameObject.transform.SetParent(logicChain.Entity.GameObject.transform);
                    //entity.GameObject.transform.localPosition = logicChain.Entity.GameObject.transform.localPosition;
                    RestoreEntities.Add(entity);
                    entity.GameObject.AddComponent<restorScript>().Parent = Parent; 
                }
            }
           
        }

       
    }

    public class restorScript : MonoBehaviour
    {
        public Transform Parent;

        void Awake()
        {
            Events.OnSimulationToggle += (value) => 
            {
                if (value == false)
                {
                    StartCoroutine(Restore());
                }
            };
        }

        private IEnumerator Restore()
        {
            yield return 0;
            transform.SetParent(Parent);
            Destroy(GetComponent<restorScript>());
        }

    }
}
