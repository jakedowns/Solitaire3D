from TrainerManager import TrainerManager

# soldash > SolitaireTrainingDashboard
class SolitaireTrainingDashboard:
    def __init__(self, num_threads = 4):
        self.training_manager = TrainerManager(num_threads)
        self.training_manager.start_trainers()
        self.max_epochs = 100
        self.current_epoch = 0

    def start(self):
        

        trainer = SolitaireTrainer(solitaire, self.population_size, self.max_moves)

        while self.current_epoch < self.max_epochs:
            print("\033c")  # clear terminal

            # print(f"Epoch {self.current_epoch + 1}/{self.max_epochs}")
            # print(f"Generation {self.current_generation + 1}/{self.generations}")

            # Train current generation
            trainer.train()

            # Print network stats for current generation
            self.update()

            # Update current best network
            self.current_best_network = trainer.get_best_network()

            # print(f"\nBest Network: {self.current_best_network.id}")
            # self.print_network_stats(self.current_best_network)

            self.current_epoch += 1

            # Reset trainer for next generation
            trainer.new_generation()

            # End training if keyboard interrupt is detected
            try:
                input()
            except KeyboardInterrupt:
                break

    def print_network_stats(self, network):
        loss_rate = "#" * int(network.loss_rate * 10)
        subject, from_spot, to_spot, next_score, final_score, total_moves, is_valid, next_score_dev = \
            network.last_prediction
        print(f"{network.id}: {loss_rate} ({network.loss_rate:.2f})")
        print(f"Lineage: {network.lineage}")
        print(f"Type: {network.type}")
        print(f"Last Prediction: Subject: {subject}, From: {from_spot}, To: {to_spot}, Next Score: {next_score}, "
              f"Final Score: {final_score}, Total Moves: {total_moves} | Valid? {is_valid}, Deviation: {next_score_dev:.2f}")

    def update(self, Done=False):
        os.system('clear')
        header = ['Network ID', 'Loss Rate', 'Lineage', 'Type', 'Subject', 'From', 'To', 'Next Score', 'Final Score', 'Total Moves to Win', 'Valid?', 'Next Score Deviation']
        rows = []
        for network in self.trainer.networks:
            network_id = self.trainer.get_network_id(network)
            loss_rate = self.trainer.get_loss_rate(network)
            lineage = self.trainer.get_network_lineage(network)
            action_type = self.trainer.get_network_type(network)
            predictions = self.trainer.get_network_predictions(network)
            for prediction in predictions:
                subject, from_spot, to_spot, next_score, final_score, total_moves, valid, next_score_dev = prediction
                rows.append([network_id, loss_rate, lineage, action_type, subject, from_spot, to_spot, next_score, final_score, total_moves, valid, next_score_dev])

        rows.sort(key=lambda x: -x[1]) # sort by loss rate, descending
        best_network_id = self.trainer.get_best_network_id()
        table = tabulate(rows[:20], headers=header)
        print(f"Best Network ID: {best_network_id}")
        if done:
            print("Training Complete!")
        print(table)
