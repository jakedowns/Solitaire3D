from solitairemodel import SolitaireModel, init_solitaire_model

# class SolitaireTrainer:
#     def __init__(self, network, game):
#         self.network = network
#         self.game = game
#         self.max_moves = 100
#         self.max_epochs = 1000
#         self.max_generations = 100
#         self.population_size = 50

#         self.best_score = 0
#         self.best_moves = self.max_moves
#         self.loss_threshold = 0.01

class SolitaireTrainer:
    def __init__(self, epochs=1000, population_size=50, mutation_rate=0.1):
        self.epochs = epochs
        self.max_moves = 100
        self.population_size = population_size
        self.mutation_rate = mutation_rate
        self.model = init_solitaire_model()
        self.best_network = None

    def run(self):
        for epoch in range(self.epochs):
            # create initial population
            population = self.create_population()

            # evaluate initial population
            scores = self.evaluate_population(population)

            # get the best network from the initial population
            best_network = self.get_best_network(population, scores)

            for i in range(1, self.population_size):
                # create a new network by either mutating or crossing over
                new_network = self.create_new_network(best_network, population)

                # evaluate the new network
                new_score = self.evaluate_network(new_network)

                # replace the worst network with the new network if it is better
                # worst_index = self.get_worst_network_index(scores)
                # if new_score > scores[worst_index]:
                #     population[worst_index] = new_network
                #     scores[worst_index] = new_score

            # update the best network
            best_network = self.get_best_network(population, scores)

            # print the best score for the epoch
            print("Epoch {}: Best score: {}".format(epoch, scores[0]))

        # save the best network
        best_network.save_weights("best_network.h5")

    def train(self):
        for epoch in range(self.max_epochs):
            # Create a new generation of networks
            population = self.create_population()
            
            for generation in range(self.max_generations):
                for i in range(self.population_size):
                    # Play the game with the current network
                    self.play_game(population[i])
                    
                    # Evaluate the network and update the best network if needed
                    score, moves, loss = self.evaluate_network(population[i])
                    if score > self.best_score or (score == self.best_score and moves < self.best_moves):
                        self.best_network = population[i]
                        self.best_score = score
                        self.best_moves = moves
                    
                    # Check if the training should stop
                    if score >= self.best_score and moves <= self.best_moves:
                        print("Training stopped: new high score or lowest num moves reached")
                        return
                    
                    if loss <= self.loss_threshold:
                        print("Training stopped: minimum loss reached")
                        return
                    
        print("Training stopped: max epochs reached")
    
    def create_population(self):
        # Spawn a new generation of networks using elitism, crossover, and mutation
        # Return a list of networks
        pass
        
    def play_game(self, network):
        # Play the game using the given network and the current game state
        pass
        
    def evaluate_network(self, network):
        # Evaluate the network by playing the game and calculating the score, number of moves, and loss
        # Return the score, number of moves, and loss
        pass
    
    def get_best_network(self):
        return self.best_network
