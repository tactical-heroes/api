"""Summarize the predeclared 20 independent runner pairs without excluding outliers."""

import argparse
import csv
import json
from pathlib import Path
import random
import statistics


def interval(values):
    ordered = sorted(values)
    return [ordered[int(0.025 * len(ordered))], ordered[int(0.975 * len(ordered))]]


def summarize(pairs, phase):
    before = [pair["dynamic"]["total"] if phase == "total"
              else pair["dynamic"]["phases"][phase] for pair in pairs]
    after = [pair["static"]["total"] if phase == "total"
             else pair["static"]["phases"][phase] for pair in pairs]
    rng = random.Random(20261007)  # NOSONAR: Reproducible statistical resampling; no security-sensitive randomness.
    changes = []
    differences = []
    for _ in range(100000):
        indices = rng.choices(range(len(pairs)), k=len(pairs))
        a = statistics.fmean(before[i] for i in indices)
        b = statistics.fmean(after[i] for i in indices)
        changes.append(100 * (b / a - 1))
        differences.append(b - a)
    a, b = statistics.fmean(before), statistics.fmean(after)
    return {
        "dynamic_mean": a, "static_mean": b, "difference": b-a,
        "change_percent": 100*(b/a-1),
        "difference_ci95": interval(differences), "change_ci95": interval(changes),
        "dynamic_median": statistics.median(before), "static_median": statistics.median(after),
        "dynamic_range": [min(before), max(before)], "static_range": [min(after), max(after)],
        "dynamic_stdev": statistics.stdev(before), "static_stdev": statistics.stdev(after),
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("directory", type=Path)
    args = parser.parse_args()
    results = [json.loads(path.read_text(encoding="utf-8"))
               for path in sorted(args.directory.rglob("result.json"))]
    if len(results) != 20 or {result["pair"] for result in results} != set(range(20)):
        raise RuntimeError("All 20 complete, unique pairs are required; do not discard failed pairs")
    if len({result["commit"] for result in results}) != 1:
        raise RuntimeError("Pairs used different commits")
    pairs = []
    for result in sorted(results, key=lambda item: item["pair"]):
        pair = {run["variant"]: run for run in result["measured"]}
        if pair["dynamic"]["counts"] != pair["static"]["counts"]:
            raise RuntimeError("Test counts differ")
        pairs.append(pair)
    if any(pair["dynamic"]["counts"] != pairs[0]["dynamic"]["counts"] for pair in pairs):
        raise RuntimeError("Test counts differ across runners")
    summary = {
        "commit": results[0]["commit"], "pairs": 20,
        "method": "Paired percentile bootstrap, 100000 resamples, seed 20261007; no outlier exclusions",
        "total": summarize(pairs, "total"), "tests": summarize(pairs, "tests"),
        "test_counts": pairs[0]["dynamic"]["counts"],
    }
    (args.directory / "summary.json").write_text(json.dumps(summary, indent=2) + "\n", encoding="utf-8")
    with (args.directory / "measurements.csv").open("w", newline="", encoding="utf-8") as file:
        writer = csv.writer(file)
        writer.writerow(["pair", "order", "variant", "total_seconds", "bootstrap_build_seconds",
                         "codegen_seconds", "solution_build_seconds", "tests_seconds"])
        for result in sorted(results, key=lambda item: item["pair"]):
            for run in result["measured"]:
                phase = run["phases"]
                writer.writerow([result["pair"], "/".join(result["order"]), run["variant"],
                                 run["total"], phase.get("bootstrap_build", 0),
                                 phase.get("codegen", 0), phase["solution_build"], phase["tests"]])
    lines = ["# Codegen benchmark", "",
             f"Commit: `{summary['commit']}`. 20 independent runner pairs.",
             "Both variants warmed once on each runner. No measured samples excluded.", "",
             "| Metric | Dynamic mean, s | Static mean, s | Change | Paired 95% CI |",
             "|---|---:|---:|---:|---:|"]
    for name in ("total", "tests"):
        value = summary[name]
        low, high = value["change_ci95"]
        lines.append(f"| {name} | {value['dynamic_mean']:.3f} | {value['static_mean']:.3f} | "
                     f"{value['change_percent']:+.2f}% | [{low:+.2f}%, {high:+.2f}%] |")
    lines.extend(["", summary["method"] + ".", "",
                  "Total includes clean-output Release builds, codegen write, recompilation, "
                  "and all test projects including their container startup/shutdown. "
                  "Restore, image downloads, experimental cleanup and warmups are excluded for both variants.",
                  "Tests run sequentially by assembly, without coverage. This is not the wall time of the "
                  "existing parallel CI matrix. Strict Static applies to the API WebApplicationFactory hosts; "
                  "isolated integration-test hosts retain their own configuration.", ""])
    (args.directory / "summary.md").write_text("\n".join(lines), encoding="utf-8")
    print("\n".join(lines))


if __name__ == "__main__":
    main()
