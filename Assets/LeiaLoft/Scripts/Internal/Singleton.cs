/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Leia Inc. and its suppliers, if any.  The
* intellectual and technical concepts contained herein are
* proprietary to Leia Inc. and its suppliers and may be covered
* by U.S. and Foreign Patents, patents in process, and are
* protected by trade secret or copyright law.  Dissemination of
* this information or reproduction of this materials strictly
* forbidden unless prior written permission is obtained from
* Leia Inc.
*
****************************************************************
*/
using UnityEngine;

namespace LeiaLoft
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
		private const string DestroyInstance = "Destroying extra Instance of {0}";

        private static T _instance = null;
        private static bool _isQuiting = false;

        public virtual string ObjectName { get { return typeof(T).Name; } }

        public static T Instance
        {
            get
            {
                if (_instance == null && !_isQuiting)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying && Application.isEditor)
                        return null;
#endif
                    /*
                    LogUtil.Debug("Singleton: Creating new " + typeof(T).Name + " gameObject");
                    var go = new GameObject().AddComponent<T>();
                    if (go != null)
                    {
                        go.name = go.ObjectName;
                    }
                    */
                }

                return _instance;
            }
        }

        public static bool InstanceIsNull
        { get { return _instance == null; } }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = (T)this;
            }

            if (_instance != null && _instance != this)
            {
                this.Warning(string.Format(DestroyInstance, typeof(T).Name));
                Destroy(this.gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            _isQuiting = true;
        }
    }
}