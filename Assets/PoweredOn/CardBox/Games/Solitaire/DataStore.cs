using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PoweredOn.CardBox.PlayingCards;
using static PoweredOn.CardBox.Games.Solitaire.DataStore;
using PoweredOn.Managers;
using UnityEditor.XR.LegacyInputHelpers;

namespace PoweredOn.CardBox.Games.Solitaire
{
    public class DataStore
    {
        private string dataFilePath;
        public UserData userData
        {
            get; private set;
        }

        public class UserData
        {
            public int best_score;
            public int best_time;
            public int best_moves;

            public int current_score;
            public int current_time;
            public int current_moves;

            // do we need to retain / load "dealtOrder" ? (yes for "restart" mechanism (vs. new game))
            public int[] dealtOrder;

            public int[] deckCards;
            public int[] handCards;
            
            public int[] stockCards;
            public int[] wasteCards;

            public int[] foundation1Cards;
            public int[] foundation2Cards;
            public int[] foundation3Cards;
            public int[] foundation4Cards;
            
            public int[] tableau1Cards;
            public int[] tableau2Cards;
            public int[] tableau3Cards;
            public int[] tableau4Cards;
            public int[] tableau5Cards;
            public int[] tableau6Cards;
            public int[] tableau7Cards;
        }

        public DataStore()
        {
            
            userData = new UserData();
        }

        public void Reset()
        {
            userData = new UserData();
        }

        public bool LoadData()
        {
            dataFilePath = Application.persistentDataPath + "/userdata.json";
            if (File.Exists(dataFilePath))
            {
                string json = File.ReadAllText(dataFilePath);
                Debug.LogWarning($"loaded from {dataFilePath}:  {json}");
                userData = JsonUtility.FromJson<UserData>(json);
                return true;
            }
            return false;
        }

        public void StoreData()
        {
            string json = JsonUtility.ToJson(userData);
            File.WriteAllText(dataFilePath, json);
        }

        public void UpdateAndStore(SolitaireGame game)
        {
            UpdateData(game);
            StoreData();
        }

        public void UpdateData(SolitaireGame game)
        {
            UpdateScoreData(game);
            UpdateDealtOrder(game);

            SolitaireGameState gameState = game.GetGameState();
            UpdateDeckCards(gameState);
            UpdateHandCards(gameState);
            UpdateStockCards(gameState);
            UpdateWasteCards(gameState);
            UpdateTableauCards(gameState);
            UpdateFoundationCards(gameState);
        }

        private void UpdateFoundationCards(SolitaireGameState gameState)
        {
            for (int i = 0; i < 4; i++)
            {
                string propName = $"foundation{i + 1}Cards";
                //var type = userData.GetType();
                //Debug.LogWarning($"type.Name: {type.Name}");
                /*var fields = userData.GetType().GetFields();
                foreach (var prop in fields)
                {
                    Debug.Log($"prop: {prop.Name}: {prop.GetValue(userData)}");
                }*/
                FieldInfo fieldInfo = userData.GetType().GetField(propName);
                if (fieldInfo == null)
                {
                    Debug.LogError($"fieldInfo null {propName}");
                    continue;
                }
                int[] listNext = new int[gameState.FoundationPileGroup[i].Count];
                var j = 0;
                foreach (SuitRank id in gameState.FoundationPileGroup[i])
                {
                    listNext[j] = (int)id;
                    j++;
                }
                fieldInfo.SetValue(userData, listNext);
            }

        }

        private void UpdateTableauCards(SolitaireGameState gameState)
        {
            for(int i = 0; i < 7; i++)
            {
                string propName = $"tableau{i + 1}Cards";
                FieldInfo fieldInfo = userData.GetType().GetField(propName);
                if(fieldInfo == null)
                {
                    Debug.LogError($"fieldInfo null {propName}");
                    continue;
                }
                int[] listNext = new int[gameState.TableauPileGroup[i].Count];
                var j = 0;
                foreach(SuitRank id in gameState.TableauPileGroup[i])
                {
                    SolitaireCard card = GameManager.Instance.game.deck.GetCardBySuitRank(id);
                    listNext[j] = (int)id + (card.IsFaceUp ? 1000 : 0);
                    j++;
                }
                fieldInfo.SetValue(userData, listNext);
            }
        }

        private void UpdateWasteCards(SolitaireGameState gameState)
        {
            var i = 0;
            userData.wasteCards = new int[gameState.WastePile.Count]; 
            foreach (SuitRank id in gameState.WastePile)
            {
                userData.wasteCards[i] = (int)id;
                i++;
            }
        }

        private void UpdateStockCards(SolitaireGameState gameState)
        {
            var i = 0;
            userData.stockCards = new int[gameState.StockPile.Count];
            foreach (SuitRank id in gameState.StockPile)
            {
                userData.stockCards[i] = (int)id;
                i++;
            }
        }

        private void UpdateHandCards(SolitaireGameState gameState)
        {
            var i = 0;
            userData.handCards = new int[gameState.HandPile.Count];
            foreach (SuitRank id in gameState.HandPile)
            {
                userData.handCards[i] = (int)id;
                i++;
            }
        }

        private void UpdateDeckCards(SolitaireGameState gameState)
        {
            var i = 0;
            userData.deckCards = new int[gameState.DeckPile.Count];
            foreach (SuitRank id in gameState.DeckPile)
            {
                userData.deckCards[i] = (int)id;
                i++;
            }
        }

        public void UpdateScoreData(SolitaireGame game)
        {
            userData.best_score = game.scoreKeeper.best_score;
            userData.best_time = game.scoreKeeper.best_time;
            userData.best_moves = game.scoreKeeper.best_moves;

            userData.current_score = game.scoreKeeper.score;
            userData.current_time = game.scoreKeeper.time;
            userData.current_moves = game.scoreKeeper.moves;
        }

        public void UpdateDealtOrder(SolitaireGame game)
        {
            PlayingCardIDList dealtOrder = game.GetDealtOrder();
            userData.dealtOrder = new int[dealtOrder.Count];
            int i = 0;
            foreach (SuitRank id in dealtOrder)
            {
                userData.dealtOrder[i] = (int)id;
                i++;
            }
        }
    }
}
