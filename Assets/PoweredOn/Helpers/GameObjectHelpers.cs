using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PoweredOn
{
    public class GameObjectHelpers
    {
        /** Search all objects, even inactive ones, for a GameObject with the given name. */
        public static GameObject[] FindGameObjects(string name)
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            var result = new List<GameObject>();

            for (int i = 0; i < allObjects.Length; i++)
            {
                if (allObjects[i].name == name)
                {
                    result.Add(allObjects[i]);
                }
            }

            return result.ToArray();
        }
    }
}
