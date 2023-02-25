using PoweredOn.CardBox.Animations;
using PoweredOn.CardBox.Cards;
using PoweredOn.CardBox.PlayingCards;
using PoweredOn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireDeck: SolitaireCardPile
    {
        // enables calling ->gameObject on this class to pass thru to parent PlayingCardPile->gameObject
        // which in turn passes this type to:
        // GameManager.Instance.game.GetGameObjectByType() ~> SolitaireGame.GetGameObjectByType()
        /**
         * yay... xml...
         * <see cref="SolitaireGame.GetGameObjectByType(SolitaireGameObject)"/>
         * see: SolitaireGame.GetGameObjectByType(SolitaireGameObject)
         * @see SolitaireGame.GetGameObjectByType(SolitaireGameObject)
         **/
        public new const SolitaireGameObject gameObjectType = SolitaireGameObject.Deck_Base;

        public const SolitaireGameObject offsetGameObjectType = SolitaireGameObject.Deck_Offset;

        private SolitaireGame game;

        private bool _isShuffling = false;
        public bool IsShuffling { get { return this._isShuffling; } }
        
        private bool _isCollectingCardsToDeck = false;

        /*public static List<SuitRank> Cards()
        {
            
        }*/
        
        public bool IsCollectingCardsToDeck {  get { return this._isCollectingCardsToDeck;  } }

        public new int Count
        {
            get
            {
                return this.deckCardPile.Count;
            }
        }

        GameManager gmInstance {
            get
            {
                var instance = GameManager.Instance ?? GameObject.FindObjectOfType<GameManager>();
                if(instance == null)
                {
                    Debug.LogError("instance STILL null???");
                }
                if(instance?.game == null)
                {
                    Debug.LogError("instance isn't but game is STILL null???");

                    // try start new game
                    instance.NewGame();
                }
                return instance;
            }
        }

        public List<SolitaireCard> cards;
        Dictionary<SuitRank, int> cardIndexLookup;
        
        public PlayingCardIDList deckOrderList;

        DeckCardPile deckCardPile = DeckCardPile.EMPTY;
        public DeckCardPile DeckCardPile
        {
            get
            {
                return this.deckCardPile.Clone();
            }
        }
        List<List<ShuffleMove>> shuffleLog;

        public static PlayingCardIDList DEFAULT_DECK_ORDER
        {
            get
            {
                PlayingCardIDList defaultDeckOrder = new PlayingCardIDList();
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 13; j++)
                    {
                        defaultDeckOrder.Add(new SuitRank((Suit)i, (Rank)j));
                    }
                }
                return defaultDeckOrder;
            }
        }

        public void AddCardToDeck(SolitaireCard card)
        {
            this.deckCardPile.Add(card.GetSuitRank());
        }

        public void AddCardToDeck(SuitRank id)
        {
            this.deckCardPile.Add(id);
        }

        public SuitRank TakeTopCardFromDeck()
        {
            if(this.deckCardPile.Count < 1)
            {
                return SuitRank.NONE;
            }
            SuitRank top = this.deckCardPile.Last();
            this.deckCardPile.RemoveAt(this.deckCardPile.Count - 1);
            return top;
        }

        public void RemoveCardFromDeck(SuitRank card)
        {
            int index = this.deckCardPile.IndexOf(card);
            if(index > -1)
            {
                this.deckCardPile.RemoveAt(index);
            }
        }

        public SolitaireDeck(SolitaireGame game)
        {
            this.game = game; // is this passed by reference or value?
            
            // Instantiate in-memory cards
            cards = new List<SolitaireCard>();
            shuffleLog = new List<List<ShuffleMove>>();
            cardIndexLookup = new Dictionary<SuitRank, int>();
            deckOrderList = new PlayingCardIDList();
            deckCardPile = DeckCardPile.EMPTY;

            // for 4 suits, and 13 ranks, create 52 cards
            int deckOrder = 0;
            foreach(SuitRank suitRank in SolitaireDeck.DEFAULT_DECK_ORDER)
            {
                SolitaireCard newCard = new SolitaireCard(suitRank.suit, suitRank.rank, deckOrder);
                
                cards.Add(newCard);

                cardIndexLookup.Add(newCard.GetSuitRank(), deckOrder);

                deckOrderList.Add(newCard.GetSuitRank());

                deckCardPile.Add(newCard.GetSuitRank());

                deckOrder++;

                // generate a string of the game object name based on the current suit & rank
                // ex: "ace_of_clubs" or "king_of_spades" or "2_of_diamonds"
                string gameObjectName = newCard.GetGameObjectName();
                //DebugOutput.Instance?.Log("Finding " + gameObjectName);

                // ignore GO related stuff when running tests
                if (this.game.IsRunningInTestMode)
                {
                    //Debug.Log("Skipping GO-related stuff");
                    continue;
                }
                else
                {
                    
                    var goDeck = gmInstance.game.GetGameObjectByType(SolitaireGameObject.Deck_Base);
                    if ((int)suitRank.rank == 0 || (int)suitRank.rank > 7)
                    {
                        // texture group 2 (empty) 9_ace
                        gameObjectName = "9_ace/" + gameObjectName;
                    }
                    else
                    {
                        // texture group 1 (empty) 2_8
                        gameObjectName = "2_8/" + gameObjectName;
                    }
                    Transform child = goDeck.transform.Find(gameObjectName);
                    if (!child)
                    {
                        DebugOutput.Instance?.LogError("child not found " + gameObjectName);
                        continue;
                    }

                    // remove any existing MonoSolitaireCard components
                    MonoSolitaireCard[] prevCards = child.GetComponents<MonoSolitaireCard>();
                    foreach (MonoSolitaireCard prevCard in prevCards)
                    {
                        GameObject.DestroyImmediate(prevCard);
                    }

                    child.gameObject.AddComponent(typeof(MonoSolitaireCard));

                    MonoSolitaireCard monoCard = child.GetComponent<MonoSolitaireCard>();

                    // pass the game object reference to the SolitaireGame.gameObjectReference Dictionary
                    this.game.AddGameObjectReference(newCard.gameObjectType, child.gameObject);

                    // call SetCard on the matching child's cardInteractive component
                    monoCard.SetCard(newCard);

                    // remove any existing click impulse components
                    ClickImpulse[] prevClickImpulses = child.GetComponents<ClickImpulse>();
                    foreach (ClickImpulse prevClickImpulse in prevClickImpulses)
                    {
                        GameObject.DestroyImmediate(prevClickImpulse);
                    }

                    // add a click impulse component
                    /*ClickImpulse clickImpulse = child.gameObject.AddComponent(typeof(ClickImpulse)) as ClickImpulse;
                    // set click_impulse_force to .15
                    clickImpulse.click_impulse_force = .15f;*/

                    // link the cardInteractive component to the card
                    // needs to be done AFTER attaching the cardInteractive component to the child
                    // TODO: try to remember why
                    newCard.SetMonoCard(monoCard);

                    newCard.SetGoalIdentity(new GoalIdentity(monoCard.gameObject, GameManager.Instance.game.GetGameObjectByType(SolitaireGameObject.Deck_Offset))); 
                }
            }
        }

        public void SetDeckOrderList(List<SuitRank> suitRankList)
        {
            this.deckOrderList = new PlayingCardIDList(suitRankList);
            this.deckCardPile = new DeckCardPile(this.deckOrderList);
        }

        public SuitRank GetCardIDByIndex(int index)
        {
            return this.deckOrderList.ElementAtOrDefault(index);
        }

        public SolitaireCard GetCardBySuitRank(SuitRank suitrank)
        {
            //if(suitrank == SuitRank.NONE)
            if(suitrank.suit == Suit.NONE || suitrank.rank == Rank.NONE)
            {
                Debug.LogWarning("attempt to get NONE card from deck");
                return SolitaireCard.NONE;
            }
            int cardIndex = cardIndexLookup[suitrank];
            return cards[cardIndex];
        }

        public int GetCardIndex(SuitRank suitRank)
        {
            return cardIndexLookup[suitRank];
        }

        public void Shuffle(int numShuffles)
        {
            shuffleLog.Clear();
            for (int i = 0; i < numShuffles; i++)
            {
                Shuffle();
            }
        }

        public struct ShuffleMove
        {

            public SuitRank suitRank;
            public int from;
            public int to;
            public ShuffleMove(SuitRank suitRank, int from, int to)
            {
                this.suitRank = suitRank;
                this.from = from;
                this.to = to;
            }
        }

        public void Shuffle()
        {
            _isShuffling = true;
            
            PlayingCardIDList prevDeckOrderList = deckOrderList.Count > 0 ? deckOrderList : new PlayingCardIDList(DEFAULT_DECK_ORDER);

            // set seed to current epoch time in seconds
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);

            deckOrderList = new PlayingCardIDList(prevDeckOrderList);
            List<ShuffleMove> iteration_log = new List<ShuffleMove>();
            for (int j = 0; j < 52; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, 52);
                SuitRank temp = deckOrderList[j];
                deckOrderList[j] = deckOrderList[randomIndex];
                deckOrderList[randomIndex] = temp;

                // record the moves so we can animate them
                iteration_log.Add(new ShuffleMove(temp, j, randomIndex));
                iteration_log.Add(new ShuffleMove(deckOrderList[randomIndex], randomIndex, j));

                UpdateCardDeckOrder(cards[j], j);
                UpdateCardDeckOrder(cards[randomIndex], randomIndex);
            }
            if (deckOrderList.Count != 52)
            {
                throw new Exception("invalid deckOrderList count after shuffle");
            }
            shuffleLog.Add(iteration_log);

            // update the deckCardPile
            deckCardPile = new DeckCardPile(deckOrderList);

            _isShuffling = false;
        }

        void UpdateCardDeckOrder(SolitaireCard card, int deckOrder)
        {
            // record the updated order within the cards themselves
            card.SetDeckOrder(deckOrder);
            // update the indexLookup table
            cardIndexLookup[card.GetSuitRank()] = deckOrder;
        }

        public void SetCardsToDefaultSortOrder()
        {
            PlayingCardIDList deckOrderListNext = new PlayingCardIDList();
            // loop through the suits and ranks, and set the deck order of each card to match the relative order within the loop
            int deckOrder = 0;

            PlayingCardIDList DEFAULT_DECK_ORDER = SolitaireDeck.DEFAULT_DECK_ORDER;
            
            foreach (SuitRank cardID in DEFAULT_DECK_ORDER)
            {
                int currentCardIndex = GetCardIndex(cardID);
                SolitaireCard card = cards[currentCardIndex];
                deckOrderListNext.Add(cardID);

                UpdateCardDeckOrder(card, deckOrder);

                deckOrder++;
            }
            deckOrderList = deckOrderListNext;
        }

        public static int[][] SuitsByColor
        {
            get
            {
                return new int[2][] {
                    new int[2] { (int)Suit.CLUBS, (int)Suit.SPADES },
                    new int[2] { (int)Suit.DIAMONDS, (int)Suit.HEARTS }
                };
            }
        }

        public static bool SuitColorsAreOpposite(Suit suitA, Suit suitB)
        {
            int[][] suitsByColor = SuitsByColor;
            int[] blackSuits = suitsByColor[0];
            int[] redSuits = suitsByColor[1];
            if (
                blackSuits.Contains((int)suitA) && redSuits.Contains((int)suitB)
                || redSuits.Contains((int)suitA) && blackSuits.Contains((int)suitB)
            )
            {
                return true;
            }
            return false;
        }

        public SuitRank ElementAtOrDefault(int index)
        {
            return this.deckOrderList
                .DefaultIfEmpty(SuitRank.NONE)
                .ElementAtOrDefault(index);
        }

        public bool IsInDefaultSortOrder()
        {
            bool orderBroken = false;
            int i = 0;
            while (i < DEFAULT_DECK_ORDER.Count && !orderBroken)
            {
                SuitRank a = DEFAULT_DECK_ORDER[i];
                SuitRank b = GetCardIDByIndex(i);
                if (
                    a.suit != b.suit
                    ||
                    a.rank != b.rank
                )
                {
                    orderBroken = true;
                    break;
                }
                i++;
            }
            return !orderBroken;
        }

        public void FanCardsOut()
        {
            CollectCardsToDeck();
            SetAllFaceUp();

            const float X_SPACING = .065f;
            const float Y_SPACING = .1f;

            Vector2 offset = new((13/2) * X_SPACING, -.05f + (4/2) * Y_SPACING);

            foreach (SolitaireCard card in cards)
            {
                float xPos = (float)card.GetRank() * X_SPACING;
                float yPos = (float)card.GetSuit() * Y_SPACING;

                xPos -= offset.x;
                yPos -= offset.y;

                card.SetPosition(new Vector3(xPos, yPos, 0));
                card.SetRotation(Quaternion.Euler(new Vector3(0.0f, 180.0f, 90.0f)));
            }
        }

        // SetCardGoalsToDeckPositions is called when we want to collect all the cards into a stack
        // This GameManager class lives on the parent GameObject, and has all the card GameObjects as children
        // we want to use the GameManager's transform as the local zero position for the center of bottommost card
        // each card should be given a goal position, offset by a positive .0002 m on the local y for each card "below" it in the deck,
        // so the topmost card would have a y position of .0002 (card thickness) * 51 (51 cards below it)
        // the deckOrder property (GetDeckOrder) of each Card is what determines this offset, and the 0th card is the bottom,
        // with the 51st deck position being the top
        public void CollectCardsToDeck()
        {
            _isCollectingCardsToDeck = true;
            
            // get the deck's "offset" world position
            GameObject go = game.GetGameObjectByType(SolitaireDeck.offsetGameObjectType);
            if(go == null)
            {
                Debug.LogWarning("Solitaire Deck has no game object?");
                _isCollectingCardsToDeck = false;
                return;
            }

            // Set all card's "goal identity" to deck's "offset" empties' world position
            foreach (SuitRank cardID in DEFAULT_DECK_ORDER)
            {
                SolitaireCard card =  GetCardBySuitRank(cardID);
                float delay = DEFAULT_DECK_ORDER.IndexOf(cardID) * .01f;
                GameManager.Instance.game.MoveCardToNewSpot(ref card, PlayfieldSpot.DECK, false, delay); // always face-down in deck
            }

            _isCollectingCardsToDeck = false;
        }

        public void SetAllFaceUp()
        {
            foreach (SolitaireCard card in cards)
            {
                card.SetIsFaceUp(true);
            }
        }

        public void SetAllFaceDown()
        {
            foreach (SolitaireCard card in cards)
            {
                card.SetIsFaceUp(false);
            }
        }

        internal static Suit GetOppositeColorSuit(Suit suit)
        {
            int[][] suitsByColor = SuitsByColor;
            int[] blackSuits = suitsByColor[0];
            int[] redSuits = suitsByColor[1];
            if (blackSuits.Contains((int)suit))
            {
                return (Suit)redSuits[0];
            }
            return (Suit)blackSuits[0];
        }
    }
}
