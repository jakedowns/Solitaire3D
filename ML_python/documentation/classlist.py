class Game:
+ name<str>
+ score<int>
+ max_moves<int>
+ moves_played<int>
> __init__(self)
> reset(self)
> play(self, move)
> won(self)
> stuck(self)
> max_moves_reached(self)
> min_score_reached(self)

class Pile:
+ type<str>
+ index<int>
+ cards<List[Card]>
> __init__(self, type, index)
> push(self, card)
> pop(self)
> top(self)
> is_empty(self)
> valid_play(self, card)

class Trainer:
- game<instance>
- network<instance>
- is_training<bool>
- losses<float>
- optimizer<keras.optimizers>
> __init__(self, game, network)
> initialize_network(self)
> begin_training(self)
> next_training_step(self, encoded_state, encoded_prediction, move)
> accumulate_loss(self, loss)
> end_training_episode(self)

class Network:
- model<keras.Model>
> __init__(self)
> predict(self, input_data)
> backpropagate(self, loss)

class SolitaireTrainingDashboard:
+ num_threads<int>
+ epoch_count<int>
+ epoch_duration<timedelta>
+ best_network<Network>
+ best_network_score<int>
+ total_episodes<int>
> __
