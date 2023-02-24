class SolitaireGame:
    def __init__(self, deck):
        self.playfield = Playfield()
        self.deck = Deck(deck)
        self.score = 0
        self.moves = []
        
    def shuffle(self):
        self.deck.shuffle()
        
    def deal(self):
        for i in range(7):
            for j in range(i+1):
                self.playfield.tableau[i].add_card(self.deck.deal_one())
                
        for i in range(4):
            self.playfield.foundation[i].add_card(self.deck.deal_one())
            
        self.playfield.stock.add_cards(self.deck.cards)
        
    def reset(self):
        self.playfield.reset()
        self.deck.reset()
        self.score = 0
        self.moves = []
        
    def move(self, move):
        subject, source, destination = move
        card = self.playfield.get_card(subject, source)
        
        if not card:
            return False
        
        if not self.playfield.is_valid_move(card, destination):
            return False
        
        self.playfield.move_card(card, destination)
        self.moves.append(move)
        self.score += self.playfield.get_move_score(move)
        
        return True
    
    def is_won(self):
        for foundation in self.playfield.foundation:
            if len(foundation) != 13:
                return False
                
        return True
    
    def is_stuck(self):
        for tableau in self.playfield.tableau:
            for card in tableau:
                if self.playfield.is_valid_move(card, "stock") or \
                   self.playfield.is_valid_move(card, "foundation"):
                    return False
                    
        return True
    
    def is_max_moves_reached(self, max_moves):
        return len(self.moves) >= max_moves
    
    def is_min_score_reached(self, min_score):
        return self.score <= min_score
