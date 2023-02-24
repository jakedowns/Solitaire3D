import argparse
import time

from SolitaireTrainingDashboard import SolitaireTrainingDashboard

def main():
    # Define command line arguments
    parser = argparse.ArgumentParser(description='SolDash - Solitaire Training Dashboard')
    parser.add_argument('--threads', type=int, default=4, help='Number of threads to use for training')

    # Parse command line arguments
    args = parser.parse_args()

    # Initialize a new SolitaireTrainingDashboard
    dashboard = SolitaireTrainingDashboard(trainer, threads=args.threads)

    # Start the dashboard
    dashboard.start()

if __name__ == '__main__':
    main()