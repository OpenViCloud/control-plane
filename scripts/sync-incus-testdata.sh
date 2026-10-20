#!/bin/bash
# Copies the Incus lab recordings the tests read (Titan.Tests/Infrastructure/Incus/IncusLabRecording.cs) from one
# commit of OpenViCloud/incus-worker and records that commit in Titan.Tests/Infrastructure/Incus/testdata/SOURCE.
# CI checks that each copy is exactly the file at that commit.
# Usage: scripts/sync-incus-testdata.sh <incus-worker commit, tag or branch>
set -euo pipefail
ref=${1:?usage: scripts/sync-incus-testdata.sh <incus-worker commit, tag or branch>}
cd "$(dirname "$0")/.."
dir=Titan.Tests/Infrastructure/Incus/testdata
sha=$(gh api "repos/OpenViCloud/incus-worker/commits/$ref" --jq .sha)
for file in lab-tls-shapes-2026-10-07.log lab-untrusted-2026-10-07.log; do
  gh api "repos/OpenViCloud/incus-worker/contents/incus/provider/testdata/$file?ref=$sha" \
    -H "Accept: application/vnd.github.raw" > "$dir/$file"
done
echo "$sha" > "$dir/SOURCE"
echo "$dir synced from OpenViCloud/incus-worker@$sha; mention it in the pull request"
