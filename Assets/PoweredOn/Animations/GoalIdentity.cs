﻿using PoweredOn.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace PoweredOn.CardBox.Animations
{
    public class GoalIdentity
    {
        public float delaySetAt;
        public float delayStart;
        private bool _instant;
        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _scale;
        private Vector3 _offset;

        private bool useCustomRotation = false;
        private bool useCustomScale = true;

        private GameObject _gameObject;
        public GameObject gameObject
        {
            get
            {
                if(this._gameObject == null)
                {
                    // NOTE: this actually spams object tree :G
                    // need some kind of stub object :(
                    // can you create one that's temp and DOESN'T get added to tree?
                    return null;
                    //return new GameObject(); // TODO: make a reusable "Invalid" or "Missing" or Faux/Stub/Placeholder game object for when we don't have access but we still want to run tests without the code blowing up... TODO: completely isolate all gameobject related code to an abstraction that can be swapped when we're running SolitaireGame.TestGame's
                }
                return this._gameObject;
            }
            set
            {
                this._gameObject = value;
            }
        }

        public void SetUseCustomRotation(bool value)
        {
            useCustomRotation = value;
        }

        public void SetUseCustomScale(bool value)
        {
            useCustomScale = value;
        }

#nullable enable
        public GameObject? goalObject;
        
        public Vector3 position
        {
            get
            {
                if (goalObject != null)
                    // world -> local
                    return goalObject.transform.position + _offset;
                    //return gameObject.transform.InverseTransformPoint(goalObject.transform.position + this._offset);
                
                return _position + _offset;
            }
            set
            {
                _position = value;
            }
        }

        public Quaternion rotation
        {
            get
            {
                if (goalObject != null && !useCustomRotation)
                {
                    return goalObject.transform.localRotation;

                    // world -> local
                    // return Quaternion.Inverse(gameObject.transform.rotation) * goalObject.transform.rotation;
                }
                return this._rotation;
            }
            set
            {
                this._rotation = value;
            }
        }

        public Vector3 scale
        {
            get
            {
                if (goalObject != null && !useCustomScale)
                    return goalObject.transform.localScale;
                return this._scale;
            }
            set
            {
                this._scale = value;
            }
        }

        /*public void SetGoalPositionFromWorldPosition(Vector3 worldPosition)
        {
            this._position = gameObject.transform.InverseTransformPoint(worldPosition);
        }*/

        public GoalIdentity(GameObject gameObject, GameObject goalObject)
        {
            this.gameObject = gameObject;
            this.goalObject = goalObject;
            this._position = Vector3.zero;
            this._rotation = Quaternion.identity;
            this._scale = Vector3.one;
            this.delaySetAt = 0.0f;
            this.delayStart = 0.0f;
            this._offset = Vector3.zero;
        }

        public GoalIdentity(GameObject gameObject, GameObject goalObject, Vector3 offset)
        {
            this.gameObject = gameObject;
            this.goalObject = goalObject;
            this._position = Vector3.zero;
            this._rotation = Quaternion.identity;
            this._scale = Vector3.one;
            this.delaySetAt = 0.0f;
            this.delayStart = 0.0f;
            this._offset = offset;
        }

        public GoalIdentity(GameObject gameObject, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.gameObject = gameObject;
            this.goalObject = null;
            this._position = position;
            this._rotation = rotation;
            this._scale = scale;
            this.delaySetAt = 0.0f;
            this.delayStart = 0.0f;
            this._offset = Vector3.zero;
        }

        public void SetDelay(float delay)
        {
            delaySetAt = Time.realtimeSinceStartup;
            delayStart = delay;
        }

        internal void SetInstant(bool instant)
        {
            _instant = instant;
        }
        public bool IsInstant
        {
           get {
                return _instant;
           }
        }
    }
}
