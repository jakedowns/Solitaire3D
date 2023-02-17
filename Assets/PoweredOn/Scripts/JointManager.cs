using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PoweredOn.CardBox.PlayingCards;
using PoweredOn.CardBox.Games.Solitaire;
using PoweredOn.Managers;
using PoweredOn;
using UnityEngine;

namespace PoweredOn.Managers {
    
    public class JointManager: MonoBehaviour 
    {
        public float spring_min_distance = 0.01f;
        public float spring_max_distance = 0.1f;

        public static List<JointChain> horizontalJointChains = new List<JointChain>();

        public static List<JointChain> pileJointChains = new List<JointChain>();
        // Index List:
        // 0-3: Foundations
        // 4-10: Tableau
        const int TABLEAU_PILE_INDEX = 4;
        // 11: Stock
        const int STOCK_PILE_INDEX = 11;
        // 12: Waste
        const int WASTE_PILE_INDEX = 12;
        // 13: Deck
        const int DECK_PILE_INDEX = 13;
        // 14: Hand
        const int HAND_PILE_INDEX = 14;


        private static JointManager _instance;
        public static JointManager Instance {
            get {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<JointManager>();
                }
                return _instance;
            }
            private set {
                _instance = value;
            } 
        }

        public JointManager(){

            // === per-pile "vertical" chains ===

            pileJointChains = new List<JointChain>();

            // chains for foundations 0-3
            for(var i = 0; i < 4; i++){
                if(Enum.TryParse<SolitaireGameObject>($"Foundation{i+1}_Base", out var foundationObject)){
                    pileJointChains.Add(new JointChain(foundationObject));
                }else{
                    Debug.LogError($"Could not parse SolitaireGameObject for Foundation{i+1}_Base");
                }
            }

            // chains for tableau 4-10
            for(var i = 0; i < 7; i++){
                if(Enum.TryParse<SolitaireGameObject>($"Tableau{i+1}_Base", out var tableauObject)){
                    pileJointChains.Add(new JointChain(tableauObject));
                }else{
                    Debug.LogError($"Could not parse SolitaireGameObject for Tableau{i+1}_Base");
                }
            }

            // chain for stock, waste, deck, and hand 11-14
            pileJointChains.Add(new JointChain(SolitaireGameObject.Stock_Base)); // 11
            pileJointChains.Add(new JointChain(SolitaireGameObject.Waste_Base)); // 12
            pileJointChains.Add(new JointChain(SolitaireGameObject.Deck_Offset)); // 13
            pileJointChains.Add(new JointChain(SolitaireGameObject.Hand_Base)); // 14

            Debug.Log("[JointManager] pileJointChains: " + pileJointChains.Count + " chains");
            
            // === horizontal chains ===

            horizontalJointChains = new List<JointChain>();

            // horizontal chain 0: for foundations <-> waste <-> stock
            horizontalJointChains.Add(new JointChain(){
                SolitaireGameObject.Foundation1_Base, 
                SolitaireGameObject.Foundation2_Base, 
                SolitaireGameObject.Foundation3_Base, 
                SolitaireGameObject.Foundation4_Base,
                SolitaireGameObject.Waste_Base, 
                SolitaireGameObject.Stock_Base
            });

            // 20 additional horizontal chains for each tableau "depth" level 
            // (placeholders, pre-mapped memory)
            for(var i = 0; i < 20; i++){
                horizontalJointChains.Add(new JointChain(7));
            }
        }

        public void UpdateJointsForCard(SolitaireCard card, float delay = 0.0f)
        {
            StartCoroutine(UpdateJointsForCardCoroutine(card, delay));
        }

        IEnumerator UpdateJointsForCardCoroutine(SolitaireCard card, float delay)
        {
            yield return new WaitForSeconds(delay);
            UpdateJointsForCard_internal(card);
        }

        void UpdateJointsForCard_internal(SolitaireCard card){
            RemoveCardFromChain(card); // previousPlayfieldSpot <-> chain
            AddCardToChain(card); // playfieldSpot <-> chain

            // impart a force on the Z axis to make the card "pop" up
            card.monoCard.gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0,-0.5f), ForceMode.Impulse);

            // rotate the card to face up or down
            if(card.IsFaceUp){
                card.monoCard.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            }else{
                card.monoCard.gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }

        public void RemoveCardFromChain(SolitaireCard card)
        {   
            if (card.previousPlayfieldSpot.area == PlayfieldArea.TABLEAU)
            {
                // Horizontal Chain // TODO
                // var hChain = horizontalJointChains[card.previousPlayfieldSpot.index + 1];
                // if (hChain != null)
                // {
                //     // i | t0 | t1 | t3
                //     // 0 | x  | x  | x
                //     // 1 | o  | x  | 0




                //     hChain.RemoveCard(card.gameObjectType);
                // }

                // Pile Chain
                var pChain = pileJointChains[TABLEAU_PILE_INDEX + card.previousPlayfieldSpot.index];
                if (pChain != null)
                {
                    // get card that is one lower than the one we're removing
                    var prevObjType = pChain[card.previousPlayfieldSpot.subindex - 1];
                    var prevObj = GameManager.Instance.game.GetGameObjectByType(prevObjType);
                    RemoveSpringJointsConnectedTo(prevObj, card.monoCard.gameObject.GetComponent<Rigidbody>());
                    pChain.RemoveCard(card.gameObjectType);
                }
            }

        }

        public void AddCardToChain(SolitaireCard card)
        {
            Debug.LogWarning($"AddCardToChain {card.gameObjectType} to {card.playfieldSpot.area} {card.playfieldSpot.index} {card.playfieldSpot.subindex}");
            if(card.playfieldSpot.area == PlayfieldArea.FOUNDATION){
                var chain = pileJointChains[card.playfieldSpot.index];
                var fallbackBase = $"Foundation{card.playfieldSpot.index + 1}_Base";
                AddCardToChain_internal(chain, card, fallbackBase);
            }
            
            else if(card.playfieldSpot.area == PlayfieldArea.TABLEAU){
                var chain = pileJointChains[TABLEAU_PILE_INDEX + card.playfieldSpot.index];
                var fallbackBase = $"Tableau{card.playfieldSpot.index + 1}_Base";
                AddCardToChain_internal(chain, card, fallbackBase);
            }

            else if(card.playfieldSpot.area == PlayfieldArea.STOCK){
                var chain = pileJointChains[STOCK_PILE_INDEX];
                var fallbackBase = "Stock_Base";
                AddCardToChain_internal(chain, card, fallbackBase);
            }

            else if(card.playfieldSpot.area == PlayfieldArea.WASTE){
                var chain = pileJointChains[WASTE_PILE_INDEX];
                var fallbackBase = "Waste_Base";
                AddCardToChain_internal(chain, card, fallbackBase);
            }
            
            else if(card.playfieldSpot.area == PlayfieldArea.DECK){
                var chain = pileJointChains[DECK_PILE_INDEX];
                var fallbackBase = "Deck_Offset";
                AddCardToChain_internal(chain, card, fallbackBase);
            }

            else if(card.playfieldSpot.area == PlayfieldArea.HAND){
                var chain = pileJointChains[HAND_PILE_INDEX];
                var fallbackBase = "Hand_Base";
                AddCardToChain_internal(chain, card, fallbackBase);
            }
            
            else{
                Debug.LogError($"AddCardToChain: unknown area {card.playfieldSpot.area}");
            }
        }

        internal void AddCardToChain_internal(JointChain chain, SolitaireCard card, string fallbackBase)
        {
            if(chain == null){
                Debug.LogError("AddCardToChain_internal: null chain passed in");
                return;
            }
            var prevTopGOT = chain.Last();
            if(prevTopGOT == null){
                // if no previous card in the chain, then add a spring joint to the base
                if(Enum.TryParse<SolitaireGameObject>(fallbackBase, out var baseGOT)){
                    prevTopGOT = baseGOT;
                }
            }

            if(prevTopGOT == SolitaireGameObject.Stock_Base){
                Debug.LogWarning($"AddCardToChain_internal: prevTopGOT is Stock_Base. adding {card.gameObjectType} to chain");
            }

            // record the linkage in our chain list
            chain.AddCard(card.gameObjectType);

            //Debug.LogWarning($"AddCardToChain_internal: fallbackBase:{fallbackBase} prevTopGOT:{prevTopGOT}");
            GameObject prevObj = GameManager.Instance.game.GetGameObjectByType(prevTopGOT);

            // create the actual spring joint from the prev top card (or chain fallback base object) to the new top card
            AddSpringJoint(prevObj, card.gameObject);
        }

        public void AddSpringJoint(GameObject objA, GameObject objB)
        {
            if(objA == null || objB == null){
                Debug.LogError("AddSpringJoint: null object(s) passed in");
                return;
            }

            // if objA doesn't have a Rigidbody, add one
            if(objA.GetComponent<Rigidbody>() == null){
                var rb = objA.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            var springJoint = objA.AddComponent<SpringJoint>();
            
            springJoint.spring = 10.0f;
            springJoint.damper = 0.02f;
            springJoint.minDistance = spring_min_distance; //0.001f;
            springJoint.maxDistance = spring_max_distance; //0.005f;
            springJoint.enableCollision = true;
            springJoint.autoConfigureConnectedAnchor = false; //true;
            springJoint.connectedAnchor = new Vector3(0, 0, -0.1f);

            springJoint.connectedBody = objB.GetComponent<Rigidbody>();
        }

        public void RemoveJointsToCard(SolitaireCard card)
        {
            var rb = card.gameObject.GetComponent<Rigidbody>();
            foreach(var chain in pileJointChains){
                if(chain.Contains(card.gameObjectType)){
                    var index = chain.IndexOf(card.gameObjectType);
                    if(index > 0){
                        var prevObjType = chain[index - 1];
                        var prevObj = GameManager.Instance.game.GetGameObjectByType(prevObjType);
                        RemoveSpringJointsConnectedTo(prevObj, rb);
                    }
                    // remove the card from the chain
                    chain.RemoveCard(card.gameObjectType);
                }
            }
            // todo: horizontal chains
        }

        public void RemoveSpringJointsConnectedTo(GameObject objA, Rigidbody rb)
        {
            int rmCount = 0;
            var springJoints = objA.GetComponents<SpringJoint>();
            foreach (var springJoint in springJoints)
            {
                if (springJoint.connectedBody == rb)
                {
                    rmCount++;
                    DestroyImmediate(springJoint);
                }
            }
            if(rmCount > 0)
                Debug.LogWarning($"RemoveSpringJointsConnectedTo: removed {rmCount} joints from {objA.name} to {rb.gameObject.name}");
        }

        public void RemoveSpringJoints(GameObject objA)
        {
            var springJoints = objA.GetComponents<SpringJoint>();
            foreach (var springJoint in springJoints)
            {
                DestroyImmediate(springJoint);
            }
        }

        public string GetDebugText(){
            string outString = "";
            outString += "=== Pile Chains ===\n";
            int i = 0;
            foreach(var chain in pileJointChains){
                string prefix = "";
                if(i < 4){
                    prefix = "f"+i;
                }else if(i <11){
                    prefix = "t"+(i-4);
                }else if(i == 11){
                    prefix = "s";
                }else if(i == 12){
                    prefix = "w";
                }else if(i == 13){
                    prefix = "d";
                }else if(i == 14){
                    prefix = "h";
                }
                outString += $"{prefix}:{chain.Count}, "; //chain.ToString();
                i++;
            }
            outString += "\n=== Horizontal Chains ===\n";
            foreach(var chain in horizontalJointChains){
                outString += $"h:{chain.Count} "; //chain.ToString();
            }
            return outString;
        }

        /**
        * Loop through all pile chains, and update the spring joints
        **/
        public void UpdateAllJoints(){
            foreach(var chain in pileJointChains){
                UpdateJointsForChain(chain);
            }
            // horizontal chains
            foreach(var chain in horizontalJointChains){
                UpdateJointsForChain(chain);
            }
        }

        public void UpdateJointsForChain(JointChain chain){
            // loop through all cards in the chain, and update the spring joints
            if(chain.Count > 0){
                // remove any existing joints from the base object
                RemoveSpringJoints(GameManager.Instance.game.GetGameObjectByType(chain[0]));
            }
            // start @ 1 to skip the base card
            for(var i = 1; i < chain.Count; i++){

                // skip first card in chain
                // if(i == 0){
                //     continue;
                // }

                var prevGOT = chain[i-1];
                var prevObj = GameManager.Instance.game.GetGameObjectByType(prevGOT);
                
                // remove exisiting joints
                RemoveSpringJoints(prevObj);
                
                // add new joint
                AddSpringJoint(prevObj, GameManager.Instance.game.GetGameObjectByType(chain[i]));
            }
        }
    }
}