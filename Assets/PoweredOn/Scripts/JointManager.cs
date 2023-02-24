using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PoweredOn.CardBox.PlayingCards;
using PoweredOn.CardBox.Games.Solitaire;
using PoweredOn.Managers;
using PoweredOn;
using UnityEngine;
using UnityEngine.Assertions;

namespace PoweredOn.Managers {
    
    public class JointManager: MonoBehaviour 
    {
        public float spring_min_distance = 0.01f;
        public float spring_max_distance = 0.1f;
        public float spring_spring = 100f;
        public float spring_damper = 0.01f;

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

        public static void DestroyJoint(GameObject jointGameObject)
        {
            // remove all configurableJoints from object
            foreach(var joint in jointGameObject.GetComponents<ConfigurableJoint>())
            {
                DestroyImmediate(joint);
            }
        }

        public JointManager(){

            Enable();
        }

        public void Reset()
        {
            Disable();
            Enable();
        }

        public void Disable()
        {
            // clean up all joints
            foreach (var chain in pileJointChains)
            {
                chain.DestroyAllJoints();
            }
            foreach (var chain in horizontalJointChains)
            {
                chain.DestroyAllJoints();
            }
            pileJointChains = new List<JointChain>();
            horizontalJointChains = new List<JointChain>();
        }

        public void Enable()
        {
            if (GameManager.Instance && GameManager.Instance.GoalAnimationSystemEnabled)
            {
                // ignore if the joint system is disabled
                return;
            }
            // === per-pile "vertical" chains ===

            pileJointChains = new List<JointChain>();

            // chains for foundations 0-3
            for (var i = 0; i < 4; i++)
            {
                if (Enum.TryParse<SolitaireGameObject>($"Foundation{i + 1}_Base", out var foundationObject))
                {
                    pileJointChains.Add(new JointChain(foundationObject));
                }
                else
                {
                    Debug.LogError($"Could not parse SolitaireGameObject for Foundation{i + 1}_Base");
                }
            }

            // chains for tableau 4-10
            for (var i = 0; i < 7; i++)
            {
                if (Enum.TryParse<SolitaireGameObject>($"Tableau{i + 1}_Base", out var tableauObject))
                {
                    pileJointChains.Add(new JointChain(tableauObject));
                }
                else
                {
                    Debug.LogError($"Could not parse SolitaireGameObject for Tableau{i + 1}_Base");
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
            for (var i = 0; i < 20; i++)
            {
                horizontalJointChains.Add(new JointChain(7));
            }
        }

        public void UpdateJointsForCard(SolitaireCard card, float delay = 0.0f)
        {
            if (GameManager.Instance.GoalAnimationSystemEnabled)
            {
                return;
            }
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
                    if(pChain.Count > 1){
                        int prevIndex = card.previousPlayfieldSpot.subindex + 1; // keep it offset by one to account for the "base" entry in each chain
                        if(prevIndex < 1){
                            Debug.LogError("trying to remove a card link that can't exist");
                            prevIndex = 1;
                        }
                        if(prevIndex > pChain.Count - 1){
                            Debug.LogError("trying to remove a card link that can't exist");
                            prevIndex = pChain.Count - 2;
                        }
                        var prevObjType = pChain[prevIndex];
                        var prevObj = GameManager.Instance.game.GetGameObjectByType(prevObjType);
                        RemoveConfigurableJointsConnectedTo(prevObj, card.monoCard.gameObject.GetComponent<Rigidbody>());
                        pChain.RemoveCard(card.gameObjectType);
                    }else{
                        Debug.LogError($"RemoveCardFromChain: {card.gameObjectType} has no previous card in chain index {pileJointChains.IndexOf(pChain)}. {pChain.Count} cards in chain.");
                    }
                }
            }

        }

        public void AddCardToChain(SolitaireCard card)
        {
            //Debug.LogWarning($"AddCardToChain {card.gameObjectType} to {card.playfieldSpot.area} {card.playfieldSpot.index} {card.playfieldSpot.subindex}");
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

            if(prevTopGOT == card.gameObjectType){
                Debug.LogWarning($"AddCardToChain_internal: prevTopGOT is same as card. cannot link a card to itself! {card.gameObjectType} count:{chain.Count} pjcI:{pileJointChains.IndexOf(chain)}");
                if(chain.Count > 1){
                    // get previous card (odd that we could even get into this state tho, no?)
                    prevTopGOT = chain[chain.Count - 2];
                }else{
                    Debug.LogError($"AddCardToChain_internal: prevTopGOT is same as card. cannot link a card to itself! {card.gameObjectType} count:{chain.Count} pjcI:{pileJointChains.IndexOf(chain)}");
                    return;
                }
            }

            

            // if(prevTopGOT == SolitaireGameObject.Stock_Base){
            //     Debug.LogWarning($"AddCardToChain_internal: prevTopGOT is Stock_Base. adding {card.gameObjectType} to chain");
            // }

            // record the linkage in our chain list
            chain.AddCard(card.gameObjectType);

            //Debug.LogWarning($"AddCardToChain_internal: fallbackBase:{fallbackBase} prevTopGOT:{prevTopGOT}");
            GameObject prevObj = GameManager.Instance.game.GetGameObjectByType(prevTopGOT);

            // create the actual spring joint from the prev top card (or chain fallback base object) to the new top card
            Vector3 goalPosition = GetGoalPositionForChainObject(chain, card.gameObjectType);
            AddConfigurableJoint(prevObj, card.gameObject, goalPosition);
        }

        public void AddConfigurableJoint(GameObject objA, GameObject objB, Vector3 goalPosition)
        {
            if(objA.name != "Stock" && objB.name != "ace_of_spades"){
                return;
            }
            if(objA == null || objB == null){
                Debug.LogError("AddConfigurableJoint: null object(s) passed in");
                return;
            }
            if(objA == objB){
                Debug.LogError($"AddConfigurableJoint: objA and objB are the same object {objA.name}|{objB.name}");
                return;
            }

            // remove any existing joints
            var existingJoints = objB.GetComponents<ConfigurableJoint>();
            foreach (var j in existingJoints)
            {
                DestroyImmediate(j);
            }

            // if objA doesn't have a Rigidbody, add one
            // if (objA.GetComponent<Rigidbody>() == null){
            //     var rb = objA.AddComponent<Rigidbody>();
            //     rb.useGravity = false;
            //     rb.isKinematic = true;
            // }

            var joint = objB.AddComponent<ConfigurableJoint>();

            joint.anchor = Vector3.zero;
            joint.connectedAnchor = goalPosition;

            // add SoftJointLimitSpring
            var spring = new SoftJointLimitSpring();
            spring.spring = spring_spring;
            spring.damper = spring_damper;
            // spring.minDistance = spring_min_distance; //0.001f;
            // spring.maxDistance = spring_max_distance; //0.005f;

            joint.linearLimitSpring = spring;

            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;

            
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            // set linear motion limits to match old spring min/max distances
            var linearLimit = new SoftJointLimit();
            linearLimit.limit = spring_max_distance;
            linearLimit.bounciness = 0f;
            joint.linearLimit = linearLimit;

            joint.enablePreprocessing = false;

            joint.enableCollision = false; // true;
            joint.autoConfigureConnectedAnchor = false; //true;
            joint.configuredInWorldSpace = true;

            joint.projectionMode = JointProjectionMode.PositionAndRotation;
            joint.projectionDistance = 0.001f;

            // todo: conditional Y offset if this is a tableau card (fanned)
            joint.connectedAnchor = new Vector3(0, -0.05f, 0.01f);

            //joint.connectedBody = objB.GetComponent<Rigidbody>();
        }

        public void RemoveJointsToCard(SolitaireCard card)
        {
            var rb = card.gameObject.GetComponent<Rigidbody>();

            // for each of the pile jointchains
            foreach(var chain in pileJointChains){
                // if the chain contains this card
                if (chain.Contains(card.gameObjectType)) {
                    // find the object the points to this one
                    var index = chain.IndexOf(card.gameObjectType);

                    Assert.IsTrue(index != 0); // index should never be zero. there should always be a "baseObj" the lowest spot of a pile jointchain

                    var prevObjType = chain[index - 1];
                    GameObject prevObj = GameManager.Instance.game.GetGameObjectByType(prevObjType);
                    RemoveConfigurableJointsConnectedTo(prevObj, rb);

                    // remove the card from the chain
                    chain.RemoveCard(card.gameObjectType);


                    // >> if we're moving from DECK->STOCK or STOCK->TABLEAU // if we're Dealing,
                    // >> we need to repair the chain by linking the prevObj to any object "downstream" from our position
                    // find objects that THIS card points to
                    var curObj = card.monoCard;
                    var nextIndex = index + 1;
                    if (nextIndex < 0 || nextIndex > chain.Count - 1)
                    {
                        //Debug.LogError("invalid nextIndex");
                    }
                    else
                    {
                        var nextObjType = chain[nextIndex];
                        var nextObj = GameManager.Instance.game.GetGameObjectByType(nextObjType);

                        // remove any joints on the current card being removed
                        int rmCount = 0;
                        foreach(ConfigurableJoint cfjoint in curObj.GetComponents<ConfigurableJoint>())
                        {
                            rmCount++;
                            DestroyImmediate(cfjoint);
                        }
                        if (rmCount > 0)
                            Debug.LogWarning("RemoveJointsToCard:patching chain");

                        // patch the chain
                        Vector3 goalPosition = GetGoalPositionForChainObject(chain, card.gameObjectType);
                        AddConfigurableJoint(prevObj, nextObj, goalPosition);
                    }
                    
                    
                    // >> if we're moving from tableau to tableau (with a substack) we should let the subchain remain in-tact,
                    // but we need to make sure it gets moved to the Hand as a full unit? (maybe we just repair the link, same as above, and let the loop of MoveCardToNewSpot handle plucking the nextmost cards for the rest of the substack/subchain
                    
                    
                }
            }
            // todo: horizontal chains
        }

        public void RemoveConfigurableJointsConnectedTo(GameObject objA, Rigidbody rb)
        {
            int rmCount = 0;
            var springJoints = objA.GetComponents<ConfigurableJoint>();
            foreach (var springJoint in springJoints)
            {
                if (springJoint.connectedBody == rb)
                {
                    rmCount++;
                    DestroyImmediate(springJoint);
                }
            }
            if(rmCount > 0)
                Debug.LogWarning($"RemoveConfigurableJointsConnectedTo: removed {rmCount} joints from {objA.name} to {rb.gameObject.name}");
        }

        public void RemoveConfigurableJoints(GameObject objA)
        {
            var springJoints = objA.GetComponents<ConfigurableJoint>();
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
                RemoveConfigurableJoints(GameManager.Instance.game.GetGameObjectByType(chain[0]));
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
                RemoveConfigurableJoints(prevObj);
                
                // add new joint
                Vector3 goalPosition = GetGoalPositionForChainObject(chain, chain[i]);
                AddConfigurableJoint(prevObj, GameManager.Instance.game.GetGameObjectByType(chain[i]), goalPosition);
            }
        }

        public Vector3 GetGoalPositionForChainObject(JointChain chain, SolitaireGameObject obj){
            int index = chain.IndexOf(obj) - 1;
            // only apply yOffset if we're dealing with a tableau chain
            float yOffset = 0;
            var baseObj = GameManager.Instance.game.GetGameObjectByType(chain[0]);
            var tbase = baseObj.GetComponent<MonoSolitaireCardPileBase>();
            if(tbase != null && tbase.playfieldArea == PlayfieldArea.TABLEAU){
                yOffset = -0.3f * index;
            }
            
            Vector3 goalPosition = GameManager.Instance.game.GetGameObjectByType(chain[0]).transform.position;
            goalPosition += new Vector3(0, yOffset, -0.1f * index);
            return goalPosition;
        }
    }
}