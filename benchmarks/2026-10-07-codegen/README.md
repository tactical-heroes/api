# Wolverine codegen: complete test-suite benchmark

The complete build/codegen/test cycle did **not** show a statistically significant
speedup. Executing already-built tests became faster, but preparing the generated
code offset that saving.

| Metric | Auto without pregenerated types | Codegen + Static | Change in elapsed time | Paired 95% confidence interval |
| --- | ---: | ---: | ---: | ---: |
| Complete cycle, mean | 181.227 s | 183.145 s | +1.06% | -0.33% to +2.56% |
| Test execution only, mean | 155.632 s | 144.718 s | -7.01% | -8.17% to -5.78% |
| Complete cycle, median | 193.745 s | 192.782 s | | |
| Complete cycle, observed range | 129.449–207.290 s | 137.191–211.164 s | | |

The primary metric is the complete cycle. Its confidence interval includes zero;
the experiment does not establish either a speedup or a slowdown of that cycle.
This does not prove that the two approaches have exactly identical performance.
Test execution was faster in all 20 pairs. The useful reason to retain a strict
static test path is checking the pregenerated handlers used by deployment.

## What accounts for the difference

| Phase, mean | Baseline | Pregenerated |
| --- | ---: | ---: |
| Bootstrap build of Host | — | 10.125 s |
| codegen write | — | 0.778 s |
| Solution build, including generated sources when present | 25.595 s | 27.524 s |
| Complete test execution | 155.632 s | 144.718 s |

Preparation costs an additional 12.832 seconds on average, while test execution
saves 10.914 seconds. The mean net difference is +1.918 seconds.
The command itself is inexpensive; obtaining a runnable Host and compiling its
generated sources also count toward the cost.

## Design fixed before collecting results

- Commit: `dbeb484bba0d17b2c18c1fe2bf4160d8e4e504d1`.
- [Successful GitHub Actions run](https://github.com/tactical-heroes/api/actions/runs/37641341275).
- 20 separate GitHub-hosted Ubuntu runner jobs, 4 logical CPUs each, SDK 10.0.401.
  Both variants of a pair ran on the same runner and the same source revision.
- CPU models: AMD EPYC 9V74 (9 jobs), EPYC 7763 (7), EPYC 9V45 (2),
  Intel Xeon 6973P-C (1), Xeon Platinum 8573C (1).
- 10 pairs ran Auto first; 10 ran Static first. Assignment was shuffled with
  seed 20261007. Each runner first warmed both variants once, in reverse order.
- 40 measured complete-suite runs plus 40 complete-suite warmups. Every run
  passed all 1210 tests across the solution's 13 test projects; two projects
  currently contain no tests. No measured runs or outliers were excluded.
- Each run began with generated sources removed and `dotnet clean -c Release`.
  Restore, image downloads and experimental cleanup were outside both timers.
  Source generation, required builds, test container startup/shutdown, fixtures,
  test execution and test-process shutdown were included.
- Both variants used the same disabled observability flag for Test/codegen.
  The existing Development-observability test still exercised normal startup.
- Tests ran sequentially by assembly (`--max-parallel-test-modules 1`), without
  coverage collection. These numbers are not the elapsed time of the existing
  parallel CI matrix, nor a benchmark of deployed containers.
- Baseline: `Auto` with no generated types in the freshly built Host assembly,
  so handlers were compiled on demand. Candidate: `codegen write`, rebuild,
  then strict `Static`, which also uses Wolverine's generated handler registry.
  The result therefore applies to this complete change, not just the command.
- Identity/Compendium WebApplicationFactory hosts explicitly selected the Host
  assembly. Isolated integration-test hosts retained their own configuration.
  Static generation produced 46 source files in every candidate run.

The interval is a paired percentile bootstrap of the relative difference in
arithmetic means: 100,000 resamples of whole runner pairs, seed 20261007.
An additional paired Student-t check gives a complete-cycle difference interval
of -0.871 to +4.707 seconds, reaching the same conclusion.
Mean complete-cycle differences by order were -0.173 seconds (Auto first) and
+4.008 seconds (Static first); balanced ordering matters.

## Validation and reproducibility

A separate local Release preflight passed 1210/1210 tests with strict Static.
A negative control rebuilt without generated files and failed with
`MissingPreBuiltTypesException`, naming 45 absent handler types in
`PANiXiDA.TacticalHeroes.Host`. This verifies that Static did not silently fall
back to runtime generation. The
[Wolverine codegen documentation](https://wolverinefx.io/guide/codegen)
describes this strict loading behavior.

The initial runner attempt failed during NuGet restore, before any warmup or
measurement. After fixing configuration inheritance, all 20 pairs were restarted
at the revision above. The failed setup contributed no samples.

- [All 40 raw measurements](measurements.csv), including phase timings and order.
- [Machine-readable statistics and test counts](summary.json).
- Scripts: `scripts/benchmark-codegen.py` and `scripts/summarize-codegen-benchmark.py`.
- The linked run contains per-pair metadata, all warmups, TRX reports and logs
  as artifacts (30-day retention); its summary artifact has 90-day retention.
  The committed CSV and statistics remain available after artifact expiry.

Dispatch the **Codegen benchmark** workflow to repeat the complete experiment,
or run a single pair locally using Python 3, Docker and the pinned .NET SDK:

```text
python scripts/benchmark-codegen.py --pair 0 --output <new-directory-outside-the-repository>
```
