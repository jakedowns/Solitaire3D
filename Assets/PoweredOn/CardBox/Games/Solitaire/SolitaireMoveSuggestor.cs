using PoweredOn.CardBox.PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
//using Unity.VisualScripting;
//using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine.Assertions;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class SolitaireMoveSuggestor
    {
        public Dictionary<string, bool> suggestedMoves { get; internal set; } = new Dictionary<string, bool>();
        public SolitaireMoveSuggestor()
        {
            suggestedMoves = new Dictionary<string, bool>();
        }

        public void ResetSuggestedMoves() 
        { 
            suggestedMoves.Clear();
        }

        public void RecordPlayedSuggestion(SolitaireMove move)
        {
            if(move.FromSpot.area == PlayfieldArea.TABLEAU && move.ToSpot.area == PlayfieldArea.TABLEAU)
            {
                Debug.LogWarning("recording prev suggestion: " + move.MoveID);
                suggestedMoves[move.MoveID] = true;
            }
        }
        public SolitaireMoveList SuggestMoves(SolitaireGame game)
        {
            SolitaireGameState gameState = game.GetGameState();
            SolitaireMoveList movesToRank = new SolitaireMoveList();
            

            // 1. get moves and then rank them by type

            // 1.1 check spots in this order: waste, tableau, or return STOCK_TO_WASTE
            var TableauPileGroup = game.GetTableauCardPileGroup();
            SolitaireCardPile[] cardPiles = new SolitaireCardPile[] {
                game.GetWasteCardPile(),
                TableauPileGroup[0],
                TableauPileGroup[1],
                TableauPileGroup[2],
                TableauPileGroup[3],
                TableauPileGroup[4],
                TableauPileGroup[5],
                TableauPileGroup[6]
            };
            foreach (var pile in cardPiles)
            {
                if (pile.Count > 0)
                {
                    // note: instead of just checking the top card for each tableau pile, we check all face up cards
                    PlayingCardIDList cardIDsToCheck = new PlayingCardIDList(1) { pile.Last() };
                    if (pile.GetType() == typeof(TableauCardPile))
                    {
                        cardIDsToCheck = pile.GetFaceUpCards();
                    }
                    foreach(SuitRank cardID in cardIDsToCheck)
                    {
                        SolitaireMoveList movesForCard = SuggestMovesForCard(game.GetGameState(), Managers.GameManager.Instance.game.deck.GetCardBySuitRank(cardID));
                        if (movesForCard.Count > 0)
                        {
                            foreach (var move in movesForCard)
                            { 
                                movesToRank.Add(move);
                            }
                        }
                    }
                }
            }

            // filter out any King to empty tableau moves if the tableau it's leaving has all face up cards
            SolitaireMoveList filteredMoves = new SolitaireMoveList();
            foreach (var move in movesToRank)
            {

                // if we've already suggested the move
                bool isTabToTabMove = move.FromSpot.area == PlayfieldArea.TABLEAU
                    && move.ToSpot.area == PlayfieldArea.TABLEAU;

                if (isTabToTabMove)
                {
                    if (suggestedMoves.ContainsKey(move.MoveID))
                    {
                        //Debug.Log("skipping previously suggested move: " + move.MoveID);
                        //Debug.Log("current suggestedMoves count = " + suggestedMoves.Count);
                        // skip this move
                        continue;   
                    }
                }

                if (
                    move.Subject.GetRank() == Rank.KING
                    && isTabToTabMove
                )
                {
                    //var fromPile = cardPiles[move.FromSpot.index + 1]; // offset by one cause of Waste in array
                    var toPile = cardPiles[move.ToSpot.index + 1]; // offset by one cause of Waste in array
                    if(
                        move.FromSpot.subindex == 0
                        && toPile.Count == 0
                    )
                    {
                        // Don't just move kings from pile to pile if they're not revealing anything under themselves
                        //Debug.LogWarning("ignoring tableau<->tableau move for king");
                        continue;
                    }
                    else
                    {
                        // include
                        filteredMoves.Add(move);
                    }
                }
                else
                {
                    filteredMoves.Add(move);
                }
            }

            if (gameState.StockPile.Count > 0)
            {
                //SolitaireMoveList moves = new SolitaireMoveList();
                var TopStockCard = gameState.StockPile.GetTopCard();
                // Stock -> Waste
                filteredMoves.Add(new SolitaireMove(TopStockCard, TopStockCard.playfieldSpot, new PlayfieldSpot(PlayfieldArea.WASTE, gameState.WastePile.Count)));
            }

            //UnityEngine.Debug.Log("Suggested Moves before Ranking: " + movesToRank.Count);
            
            SolitaireMoveList rankedMoves = filteredMoves.SortByMoveRank();
            Assert.IsTrue(filteredMoves.Count == rankedMoves.Count);
            UnityEngine.Debug.Log("Suggested Moves After Ranking: " + rankedMoves);

            

            // Waste -> Stock
            if (gameState.WastePile.Count > 0 && gameState.StockPile.Count == 0)
            {
                rankedMoves.Add(SolitaireMove.WASTE_TO_STOCK);   
            }

            SolitaireMoveList tabShuffleCompare = new();

            // TODO if the best move is a "to_foundation" move, always return that move
            foreach(var move in rankedMoves)
            {
                if(move.ToSpot.area == PlayfieldArea.FOUNDATION)
                {
                    return new SolitaireMoveList() { move };
                }

                if(move.FromSpot.area == PlayfieldArea.WASTE && move.ToSpot.area == PlayfieldArea.TABLEAU)
                {
                    return new SolitaireMoveList() { move };
                }

                // T<->T, let's record and compare, the one that is moving FROM the pile with the fewest cards should be preferable
                // if it's an emptyTab to an emptyTab (King shuffling) just skip it and do a waste<->stock|stock<->waste
                // TODO: just filter out suggestions for moving kings between empty tableaus
                // TODO: prefer Tab<->Tab moves that actually REVEAL a face down card or empty a tableau OVER moves that leave face up cards on the pile (tho don't totally exclude them from suggestions, sometimes that's a valuable play to free up a spot for a waste<->tableau move that can be a pre-requisite for freeing up ANOTHER tableau or face down card)
                if(
                    move.FromSpot.area == PlayfieldArea.TABLEAU 
                    && move.ToSpot.area == PlayfieldArea.TABLEAU 
                    && move.Subject.GetRank() != Rank.KING
                )
                {
                    tabShuffleCompare.Add(move);
                }
            }

            //Debug.LogWarning("todo: compare tabShuffleMoves " + tabShuffleCompare.Count);

            // shuffle the moves
            // half the time, shuffle
            if(UnityEngine.Random.Range(0, 2) == 1)
            {
                rankedMoves.Shuffle();
            }else{
                if(UnityEngine.Random.Range(0, 2) == 1)
                {                    
                    rankedMoves.Reverse();
                }
            }

            return rankedMoves;
        }

        /*
        private static SolitaireMoveList GetMovesFromStockToWaste(SolitaireGame game)
        {
            SolitaireMoveList moves = new SolitaireMoveList();
            return moves;
        }

        private static SolitaireMoveList GetMovesFromWasteToStock(SolitaireGame game)
        {
            SolitaireMoveList moves = new SolitaireMoveList();
            return moves;
        }

        private static SolitaireMoveList GetMovesToTableau(SolitaireGame game)
        {
            SolitaireMoveList moves = new SolitaireMoveList();
            return moves;
        }

        private static SolitaireMoveList GetMovesToFoundation(SolitaireGame game)
        {
            SolitaireMoveList moveList = new SolitaireMoveList();
            return moveList;
        }
        */

        // ### Card Specific Variants

        public static SolitaireMoveList SuggestMovesForCard(SolitaireGameState gameState, SolitaireCard card)
        {
            SolitaireMoveList moves;

            moves = GetMovesToFoundationForCard(gameState, card);
            if (moves.Count > 0)
                return moves;

            moves = GetMovesToTableauForCard(gameState, card);
            if (moves.Count > 0)
                return moves;

            return moves;
        }

        public static SolitaireMoveList GetMovesToFoundationForCard(SolitaireGameState gameState, SolitaireCard card)
        {
            SolitaireMoveList moves = new SolitaireMoveList();

            // if this card is not the top card, then we can't move it to the foundation
            // TODO: move IsTopCardInPlayfieldSpot to SolitaireCard class
            if(Managers.GameManager.Instance.game.IsTopCardInPlayfieldSpot(card) == false)
                return moves;

            FoundationCardPile foundationCardPile = card.GetFoundationCardPile();

            if (foundationCardPile.CanReceiveCard(card))
            {
                moves.Add(new SolitaireMove(card, card.playfieldSpot, foundationCardPile.GetPlayfieldSpot()));
            }

            return moves;
        }

        public static SolitaireMoveList GetMovesToTableauForCard(SolitaireGameState gameState, SolitaireCard card)
        {
            SolitaireMoveList moves = new SolitaireMoveList();

            for (int i = 0; i < 7; i++)
            {
                TableauCardPile tableauCardPile = gameState.TableauPileGroup[i];
                if (tableauCardPile.CanReceiveCard(card))
                {
                    moves.Add(new SolitaireMove(card, card.playfieldSpot, tableauCardPile.GetPlayfieldSpot()));
                }
            }

            return moves;
        }
    }
}
