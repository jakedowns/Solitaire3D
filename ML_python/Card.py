class Card:
    SUITS = ('hearts', 'diamonds', 'clubs', 'spades')
    RANKS = ('2', '3', '4', '5', '6', '7', '8', '9', '10', 'jack', 'queen', 'king', 'ace')
    
    def __init__(self, suit, rank):
        self.suit = suit
        self.rank = rank

        self.color = "black" if suit in ["clubs", "spades"] else "red"

    def __str__(self):
        if self.rank == 1:
            return f"Ace of {self.suit}"
        elif self.rank == 11:
            return f"Jack of {self.suit}"
        elif self.rank == 12:
            return f"Queen of {self.suit}"
        elif self.rank == 13:
            return f"King of {self.suit}"
        else:
            return f"{self.rank} of {self.suit}"
