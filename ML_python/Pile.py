class Pile:
    def __init__(self, pile_type, index):
        self.cards = []
        self.pile_type = pile_type
        self.index = index

    def add_card(self, card):
        self.cards.append(card)

    def remove_card(self):
        return self.cards.pop()

    def get_top_card(self):
        if self.cards:
            return self.cards[-1]
        else:
            return None

    def is_empty(self):
        return not bool(self.cards)

    def get_type(self):
        return self.pile_type

    def get_index(self):
        return self.index