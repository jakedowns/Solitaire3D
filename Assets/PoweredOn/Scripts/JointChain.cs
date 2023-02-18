using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PoweredOn.CardBox.PlayingCards;
using PoweredOn.CardBox.Games.Solitaire;
using PoweredOn.Managers;
using UnityEngine;

namespace PoweredOn {
    public class JointChain: IEnumerable<SolitaireGameObject> {
        private List<SolitaireGameObject> gameObjects = new List<SolitaireGameObject>();

        public JointChain(int capacity = 0) {
            gameObjects = new List<SolitaireGameObject>(capacity);
        }

        public JointChain(SolitaireGameObject gameObject) {
            gameObjects.Add(gameObject);
        }

        public void SetJointChainType(SolitaireGameObject gameObjectType) {
            // JointChainType = gameObjectType;
        }

        // collection initializer
        public JointChain(IEnumerable<SolitaireGameObject> gameObjects) {
            this.gameObjects = gameObjects.ToList();
        }

        // get enumerator
        public IEnumerator<SolitaireGameObject> GetEnumerator() {
            return gameObjects.GetEnumerator();
        }

        // index of
        public int IndexOf(SolitaireGameObject gameObject) {
            return gameObjects.IndexOf(gameObject);
        }

        // IEnumerable.GetEnumerator()
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        // Add
        public void Add(SolitaireGameObject gameObject) {
            gameObjects.Add(gameObject);
        }

        // Indexer
        public SolitaireGameObject this[int index] {
            get { return gameObjects[index]; }
            set { gameObjects[index] = value; }
        }

        // constructor that can be followed by a list of {SolitaireGameObject, SolitaireGameObject, ...}'s
        public JointChain(params SolitaireGameObject[] gameObjects) {}

        public virtual void AddCard(SolitaireGameObject gameObject) {
            gameObjects.Add(gameObject);
        }
        public virtual void RemoveCard(SolitaireGameObject gameObject) {
            gameObjects.RemoveAll(x => x == gameObject);
        }
        public virtual bool ContainsCard(SolitaireGameObject gameObjectType) {
            return gameObjects.Contains(gameObjectType);
        }

        // ToString
        public override string ToString() {
            return string.Join(", ", gameObjects);
        }

        // Count
        public int Count {
            get { return gameObjects.Count; }
        }

        public void DestroyAllJoints(){
            foreach (SolitaireGameObject gameObjectType in gameObjects)
            {
                JointManager.DestroyJoint(GameManager.Instance.game.GetGameObjectByType(gameObjectType));
            }
        }
    }
}