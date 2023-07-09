import argparse
from storage import *
from solving import *

# Usage: python -m main solve <problem_id>
# e.g. python -m main solve 42

def main():
    arg_parser = argparse.ArgumentParser()
    arg_parser.add_argument('command', choices=["solve"])
    arg_parser.add_argument('id')
    args = arg_parser.parse_args()
    if args.command == 'solve' and args.id is not None:
        id = args.id
        print('Solve ' + id)
        prev_meta = read_metadata(id)
        print('Prev meta ' + str(prev_meta))

        problem = read_problem(id)
        solution, solution_score, valid = ES_solve(problem.stage,
                                                   problem.mus_instruments,
                                                   problem.att_places,
                                                   problem.att_tastes,
                                                   problem.pillar_center_radius,
                                                   problem.use_playing_together_ext)
        print(f"New solution_score {solution_score} valid={valid}, prev.meta={prev_meta}")
        if valid and solution_score > prev_meta.score:
            print("Write solution for " + id)
            write_solution(id, solution)
            write_metadata(id, solution_score)


if __name__ == '__main__':
    main()
