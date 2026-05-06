#!/usr/bin/env bash
# Copyright (c) 2025-2026 GeWuYou
# SPDX-License-Identifier: Apache-2.0

set -euo pipefail

package_dir="${1:-./packages}"

if [ ! -d "$package_dir" ]; then
  echo "Package directory not found: $package_dir" >&2
  exit 1
fi

expected_packages=(
  "GeWuYou.GFramework"
  "GeWuYou.GFramework.Core"
  "GeWuYou.GFramework.Core.Abstractions"
  "GeWuYou.GFramework.Core.SourceGenerators"
  "GeWuYou.GFramework.Cqrs"
  "GeWuYou.GFramework.Cqrs.Abstractions"
  "GeWuYou.GFramework.Cqrs.SourceGenerators"
  "GeWuYou.GFramework.Ecs.Arch"
  "GeWuYou.GFramework.Ecs.Arch.Abstractions"
  "GeWuYou.GFramework.Game"
  "GeWuYou.GFramework.Game.Abstractions"
  "GeWuYou.GFramework.Game.SourceGenerators"
  "GeWuYou.GFramework.Godot"
  "GeWuYou.GFramework.Godot.SourceGenerators"
)

work_dir="$(mktemp -d)"
trap 'rm -rf "$work_dir"' EXIT

expected_file="$work_dir/expected-packages.txt"
actual_file="$work_dir/actual-packages.txt"

mapfile -t actual_packages < <(
  find "$package_dir" -maxdepth 1 -type f -name '*.nupkg' -exec basename {} \; \
    | sed -E 's/\.[0-9][0-9A-Za-z.-]*\.nupkg$//' \
    | sort -u
)

printf '%s\n' "${expected_packages[@]}" | sort > "$expected_file"
printf '%s\n' "${actual_packages[@]}" > "$actual_file"

echo "Expected packages:"
cat "$expected_file"
echo "Actual packages:"
cat "$actual_file"

diff -u "$expected_file" "$actual_file"
