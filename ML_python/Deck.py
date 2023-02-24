import random
from typing import List
from card import Card

class Deck:
    def __init__(self):
        self.cards = []
        for suit in Card.SUITS:
            for rank in Card.RANKS:
                self.cards.append(Card(suit, rank))

    def shuffle(self, stacked_prob: float = 0.5):
        if random.random() < stacked_prob:
            # Stacked deck, more winnable
            self.cards = [
                Card(suit, rank) for rank in Card.RANKS for suit in Card.SUITS
            ]
        else:
            # Truly random deck
            random.shuffle(self.cards)

    def deal(self, num_piles: int = 7) -> List[List[Card]]:
        piles = [[] for _ in range(num_piles)]
        for i, pile in enumerate(piles):
            for j in range(i+1):
                card = self.cards.pop(0)
                if j == i:
                    card.flip()
                pile.append(card)
        return piles
