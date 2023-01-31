using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoweredOn.Managers;

namespace PoweredOn
{
    internal class Solitaire3DTests
    {
        private SolitaireGame game;
        public void TestDeckManager()
        {
        }

        public void Run()
        {
            CanCreateDeck();
            CheckValidPickUpMoves();  
        }
        
        public void CanCreateDeck()
        {
            game = new SolitaireGame();
            game.NewGame();
            game.m_DebugOutput.LogWarning("deck count " + game.deck.cards.Count);
        }

        public void CheckValidPickUpMoves()
        {
            
        }

        public void CheckInvalidPickUpMoves()
        {
            
        }

        public void CheckValidPlaceMoves()
        {
            
        }

        public void CheckInvalidPlaceMoves()
        {
            
        }

        public void TestResetWasteToStock()
        {
            
        }
    }
}
