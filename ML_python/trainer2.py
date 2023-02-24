from keras.models import Sequential
from keras.layers import Dense

class TrainingDashboard:
    def __init__(self, num_threads=4):
        self.training_manager = TrainingManager(num_threads)
        
    def begin_training(self):
        # start the training loop
        pass
        
class TrainingManager:
    def __init__(self, num_threads):
        self.num_threads = num_threads
        self.trainers = []
        self.current_epoch = 1
        
    def init_trainers(self):
        # initialize trainers and their networks
        for i in range(self.num_threads):
            self.trainers.append(Trainer())
    
    def init_epoch(self):
        # initialize a new epoch and trainers
        pass
    
    def init_episode(self):
        # initialize a new episode and game for each trainer
        pass
    
    def step_episodes(self):
        # step through all episodes in the epoch
        pass
    
    def step_episode(self, trainer):
        # step through an individual episode for a trainer
        pass
    
class Trainer:
    def __init__(self, game):
        self.game = game
        self.network = None
        self.is_training = False
        self.initialize_network()
        
    def initialize_network(self):
        # initialize a network for the trainer
        this.network = Network()
    
    def begin_training(self):
        # begin training the network
        pass
    
    def next_training_step(self):
        # perform the next training step
        pass

class Network:
    def __init__(self):
        # init a keras model
        self.model = Sequential()
        # input_layer: 235 32-bit values
        self.model.add(Dense(100, input_dim=235, activation='relu'))
        # hidden_layers: 2 layers with 100 neurons each
        self.model.add(Dense(100, activation='relu'))
        self.model.add(Dense(100, activation='relu'))
        # output_layer: array of 6 32-bit values
        self.model.add(Dense(6, activation='softmax'))

        self.model.compile(loss='categorical_crossentropy', optimizer='adam')

        
    def predict(self, input_data):
        # make a prediction with the current network weights
        prediction = self.model.predict(input_data)
        return prediction

class Game:
    def __init__(self):
        self.deck = None
        self.playfield = None
        self.score = 0
        
    def shuffle(self):
        # shuffle the deck (50/50 chance of returning a "stacked" deck)
        pass
        
class Move:
    def __init__(self, subject, start_pile, end_pile):
        self.subject = subject
        self.start_pile = start_pile
        self.end_pile = end_pile
        
class Card:
    def __init__(self, suit, rank):
        self.suit = suit
        self.rank = rank
        
class Deck:
    def __init__(self):
        self.cards = []
        
    def shuffle(self):
        # shuffle the deck (50/50 chance of returning a "stacked" deck)
        pass
        
class Playfield:
    def __init__(self):
        self.piles = []
        
class Pile:
    def __init__(self):
        self.cards = []
        
class DecodedGameState:
    def __init__(self):
        self.playfield = None
        self.draw_pile = None
        self.discard_pile = None
        
class EncodedGameState:
    def __init__(self):
        self.input_layer = None
        
class EncodedPrediction:
    def __init__(self):
        self.output_layer = None
        
class DecodedPrediction:
    def __init__(self):
        self.move = None
        self.score = 0