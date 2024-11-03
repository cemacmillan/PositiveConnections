import argparse
import re
from collections import defaultdict

# Initialize argument parser
parser = argparse.ArgumentParser(description="Process log files for Positive Connections.")
parser.add_argument("--input", type=str, help="Input log file name", required=True)
parser.add_argument("--justOutput", action="store_true", help="Output only matching log lines")

# Parse arguments
args = parser.parse_args()

# Initialize data structures
interaction_counts = defaultdict(int)
interaction_weights = defaultdict(float)
initiator_counts = defaultdict(int)
recipient_counts = defaultdict(int)
output_lines = []

# Define regex pattern to match relevant log entries
pattern = re.compile(r"<color=#00FF7F>\[Positive Connections\]</color> (\w+) - Weight[:]? ([0-9.]+) - Initiator: ([^,]+), Recipient: (.+)")

# Process the log file
with open(args.input, 'r') as f:
    for line in f:
        match = pattern.search(line)
        if match:
            interaction = match.group(1)
            weight = float(match.group(2))
            initiator = match.group(3)
            recipient = match.group(4)

            # Track counts and sums
            interaction_counts[interaction] += 1
            interaction_weights[interaction] += weight
            initiator_counts[initiator] += 1
            recipient_counts[recipient] += 1

            # Collect lines for --justOutput
            if args.justOutput:
                output_lines.append(f"{interaction} - Weight: {weight} - Initiator: {initiator}, Recipient: {recipient}")

# Output results
if args.justOutput:
    # Print filtered log lines
    for line in output_lines:
        print(line)
else:
    # Print summary report for interactions
    print("Interaction Summary:")
    for interaction, count in interaction_counts.items():
        avg_weight = interaction_weights[interaction] / count if count > 0 else 0
        print(f"{interaction}: {count} avgWeight = {avg_weight:.7f}")
    
    # Ensure interactions with zero count are also shown
    all_possible_interactions = [
        "StorytellingInteraction", "SkillShare", "ComplimentInteraction",
        "GiftInteraction", "SharedPassionInteraction", "DiscussIdeoligionInteraction",
        "MediationInteraction", "GiveComfortInteraction"
    ]
    for interaction in all_possible_interactions:
        if interaction not in interaction_counts:
            print(f"{interaction}: 0 avgWeight = 0")

    # Print initiator and recipient counts
    print("\nInitiator Summary:")
    for initiator, count in initiator_counts.items():
        print(f"Initiator: {initiator} = {count}")

    print("\nRecipient Summary:")
    for recipient, count in recipient_counts.items():
        print(f"Recipient: {recipient} = {count}")