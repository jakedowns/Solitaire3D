import threading
from typing import List
from SolitaireTrainer import SolitaireTrainer

# soldash > SolitaireTrainingDashboard > TrainerManager
class TrainerManager:
    def __init__(self, num_threads: int, dashboard_update_frequency: int, *args, **kwargs):
        self._trainers: List[SolitaireTrainer] = []
        self._threads: List[threading.Thread] = []
        self._locks: List[threading.Lock] = []
        self._dashboard_update_frequency = dashboard_update_frequency
        self.solitaire = None

        # Create a lock for each thread
        for _ in range(num_threads):
            self._locks.append(threading.Lock())

    def init_training_session():
        self.init_trainers()

    def init_trainers():
        # Create trainers and threads
        for i in range(num_threads):
            trainer = SolitaireTrainer(*args, **kwargs)
            self._trainers.append(trainer)
            t = threading.Thread(target=self._run_trainer, args=(trainer, i))
            t.start()
            self._threads.append(t)

    def init_epoch():
        # Initialize Trainers for the Epoch (Generation)
        self.init_trainers();


    def init_episode():
        # Initialize solitaire game
        self.init_solitaire_game()
        # Pass the game to the trainers
        for trainer in self._trainers:
            trainer.init_episode(self.solitaire)

    def init_solitaire_game();
        # init a fresh solitaire game which will be copied to each trainer
        self.solitaire = SolitaireGame()
        self.solitaire.shuffle()
        self.solitaire.deal()

    def _run_trainer(self, trainer: SolitaireTrainer, thread_index: int):
        while trainer.is_training:
            with self._locks[thread_index]:
                trainer.train_next_step()
            
    def update(self):
        # Wait for all threads to be done with the current step
        for lock in self._locks:
            lock.acquire()

        # Call update on each trainer
        for i, trainer in enumerate(self._trainers):
            print(f'Trainer {i} - Epoch: {trainer.current_epoch}, Loss: {trainer.current_loss}')

        # Release the locks so the threads can continue
        for lock in self._locks:
            lock.release()

