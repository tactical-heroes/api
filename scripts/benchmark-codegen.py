"""Run one balanced pair of full-suite codegen measurements in a disposable worktree."""

import argparse
import json
import os
from pathlib import Path
import platform
import random
import shutil
import subprocess
import tempfile
import time
import xml.etree.ElementTree as ET


SOLUTION = "PANiXiDA.TacticalHeroes.slnx"
HOST = Path("src/PANiXiDA.TacticalHeroes.Host")
HOST_PROJECT = HOST / "PANiXiDA.TacticalHeroes.Host.csproj"
SEED = 20261007
NUGET_CONFIG = "nuget.config"
NO_NODE_REUSE = "/nr:false"
# The reserved .invalid address deliberately has no real telemetry collector.
OTLP_TEST_ENDPOINT = "http://otel-benchmark.invalid:4317"  # NOSONAR(S5332)


def command(args, cwd, env, log):
    started = time.perf_counter()
    with log.open("w", encoding="utf-8") as output:
        process = subprocess.run(args, cwd=cwd, env=env, stdout=output,
                                 stderr=subprocess.STDOUT, text=True)
    elapsed = time.perf_counter() - started
    if process.returncode:
        print(log.read_text(encoding="utf-8", errors="replace")[-16000:], flush=True)
        raise RuntimeError(f"{args[0:3]} failed ({process.returncode}); see {log}")
    return elapsed


def counters(reports):
    results = {}
    for report in sorted(reports.rglob("*.trx")):
        document = ET.parse(report)
        counter = document.find(".//{*}Counters")
        if counter is None:
            raise RuntimeError(f"Missing test counters in {report}")
        # One TRX per assembly; nested attachments are not separate reports.
        assembly = report.stem
        if assembly in results:
            raise RuntimeError(f"Duplicate report for {assembly}")
        results[assembly] = {k: int(counter.get(k, "0"))
                             for k in ("total", "executed", "passed", "failed", "notExecuted")}
    return results


def run_variant(root, output, env, variant, label):
    directory = output / label
    directory.mkdir()
    generated = root / HOST / "Internal/Generated"
    if generated.exists():
        # root is the disposable worktree created by this script.
        generated.resolve().relative_to(root.resolve())
        shutil.rmtree(generated)
    command(["dotnet", "clean", SOLUTION, "-c", "Release", "--nologo", "-v", "quiet",
             "-m:2", NO_NODE_REUSE], root, env, directory / "clean.log")

    phase = {}
    total_start = time.perf_counter()
    generated_count = 0
    if variant == "static":
        phase["bootstrap_build"] = command(
            ["dotnet", "build", str(HOST_PROJECT), "-c", "Release", "--no-restore",
             "--nologo", "-v", "quiet", "-m:2", NO_NODE_REUSE],
            root, env, directory / "bootstrap-build.log")
        tooling_env = dict(env, DOTNET_ENVIRONMENT="Production",
                           ASPNETCORE_ENVIRONMENT="Production")
        phase["codegen"] = command(
            ["dotnet", "bin/Release/net10.0/PANiXiDA.TacticalHeroes.Host.dll", "codegen", "write"],
            root / HOST, tooling_env, directory / "codegen.log")
        generated_count = len(list(generated.rglob("*.cs")))
        if generated_count == 0:
            raise RuntimeError("codegen write produced no source files")

    phase["solution_build"] = command(
        ["dotnet", "build", SOLUTION, "-c", "Release", "--no-restore",
         "--nologo", "-v", "quiet", "-m:2", NO_NODE_REUSE],
        root, env, directory / "solution-build.log")
    test_env = dict(env, WOLVERINE_PREGENERATED="1" if variant == "static" else "0")
    reports = directory / "reports"
    phase["tests"] = command(
        ["dotnet", "test", "--solution", SOLUTION, "-c", "Release", "--no-build",
         "--max-parallel-test-modules", "1", "--report-trx",
         "--results-directory", str(reports)],
        root, test_env, directory / "tests.log")
    total = time.perf_counter() - total_start
    test_counts = counters(reports)
    expected = len(list((root / "tests").rglob("*Tests.csproj")))
    if len(test_counts) != expected:
        raise RuntimeError(f"Expected {expected} test reports, got {len(test_counts)}")
    if any(item["failed"] for item in test_counts.values()):
        raise RuntimeError("At least one test failed")
    result = {"variant": variant, "total": total, "phases": phase, "counts": test_counts,
              "generated_files": generated_count}
    (directory / "timing.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(f"{label}: {total:.3f}s; tests {phase['tests']:.3f}s", flush=True)
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--pair", type=int, choices=range(20), required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    repository = Path(__file__).resolve().parents[1]
    commit = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=repository, text=True).strip()
    orders = ["dynamic-first"] * 10 + ["static-first"] * 10
    # A fixed seed makes experimental ordering reproducible; this is not cryptography.
    random.Random(SEED).shuffle(orders)  # NOSONAR(S2245)
    measured_order = (["dynamic", "static"] if orders[args.pair] == "dynamic-first"
                      else ["static", "dynamic"])
    # The test fixtures must create their own isolated containers and databases.
    environment = {key: value for key, value in os.environ.items()
                   if not key.lower().startswith(("connectionstrings__", "otel_exporter_otlp"))}
    environment.update(OTEL_EXPORTER_OTLP_ENDPOINT=OTLP_TEST_ENDPOINT,
                       DOTNET_CLI_TELEMETRY_OPTOUT="1", DOTNET_NOLOGO="1")
    metadata = {
        "pair": args.pair, "order": measured_order, "seed": SEED, "commit": commit,
        "platform": platform.platform(), "cpu_count": os.cpu_count(),
        "processor": platform.processor(),
        "sdk": subprocess.check_output(["dotnet", "--version"], text=True).strip(),
        "runtimes": subprocess.check_output(["dotnet", "--list-runtimes"], text=True).splitlines(),
        "run_id": os.getenv("GITHUB_RUN_ID"), "measured": [], "warmup": [],
    }
    cpuinfo = Path("/proc/cpuinfo")
    if cpuinfo.exists():
        metadata["processor"] = next(
            (line.split(":", 1)[1].strip() for line in cpuinfo.read_text().splitlines()
             if line.startswith("model name")), "")
    (output / "metadata.json").write_text(json.dumps(metadata, indent=2) + "\n", encoding="utf-8")
    with tempfile.TemporaryDirectory(prefix="panixida-codegen-pair-") as temporary:
        workspace = Path(temporary) / "api"
        subprocess.run(["git", "worktree", "add", "--detach", str(workspace), commit],
                       cwd=repository, check=True)
        try:
            config = repository / NUGET_CONFIG
            restore = ["dotnet", "restore", SOLUTION, "--nologo", "-v", "quiet"]
            if config.exists():
                shutil.copyfile(config, workspace / NUGET_CONFIG)
            command(restore, workspace, environment, output / "restore.log")
            # Warm both variants, including test containers, without counting these samples.
            for variant in reversed(measured_order):
                metadata["warmup"].append(run_variant(
                    workspace, output, environment, variant, f"warmup-{variant}"))
            for variant in measured_order:
                metadata["measured"].append(run_variant(
                    workspace, output, environment, variant, f"measured-{variant}"))
            all_runs = metadata["warmup"] + metadata["measured"]
            if any(run["counts"] != all_runs[0]["counts"] for run in all_runs[1:]):
                raise RuntimeError("The variants did not execute exactly the same tests")
            (output / "result.json").write_text(json.dumps(metadata, indent=2) + "\n", encoding="utf-8")
        finally:
            if config.exists():
                (workspace / NUGET_CONFIG).unlink(missing_ok=True)
            subprocess.run(["git", "worktree", "remove", str(workspace)],
                           cwd=repository, check=True)


if __name__ == "__main__":
    main()
