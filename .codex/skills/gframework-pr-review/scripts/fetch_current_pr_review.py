#!/usr/bin/env python3
"""
Fetch the GitHub PR page for the current GFramework branch and extract the
signals needed for local follow-up work without relying on gh CLI.
"""

from __future__ import annotations

import argparse
import html
import json
import re
import subprocess
import sys
import urllib.parse
import urllib.request
from typing import Any

OWNER = "GeWuYou"
REPO = "GFramework"
WINDOWS_GIT = "/mnt/d/Tool/Development Tools/Git/cmd/git.exe"


def run_command(args: list[str]) -> str:
    process = subprocess.run(args, capture_output=True, text=True, check=False)
    if process.returncode != 0:
        stderr = process.stderr.strip()
        raise RuntimeError(f"Command failed: {' '.join(args)}\n{stderr}")
    return process.stdout.strip()


def get_current_branch() -> str:
    return run_command([WINDOWS_GIT, "rev-parse", "--abbrev-ref", "HEAD"])


def fetch_text(url: str) -> str:
    opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))
    with opener.open(url, timeout=30) as response:
        return response.read().decode("utf-8", "replace")


def resolve_pr_number(branch: str) -> int:
    query = urllib.parse.quote(f"is:pr head:{branch} sort:updated-desc")
    url = f"https://github.com/{OWNER}/{REPO}/pulls?q={query}"
    html_text = fetch_text(url)
    match = re.search(rf'/{OWNER}/{REPO}/pull/(\d+)', html_text)
    if match is None:
        raise RuntimeError(f"No public PR matched branch '{branch}'.")
    return int(match.group(1))


def extract_embedded_data(html_text: str) -> dict[str, Any]:
    match = re.search(
        r'<script type="application/json" data-target="react-app\.embeddedData">(.*?)</script>',
        html_text,
        re.S,
    )
    if match is None:
        raise RuntimeError("Failed to locate GitHub embedded PR metadata.")
    return json.loads(match.group(1))


def extract_clipboard_values(html_text: str) -> list[str]:
    return [html.unescape(value) for value in re.findall(r'<clipboard-copy\b[^>]*\bvalue="(.*?)"', html_text, re.S)]


def collapse_whitespace(text: str) -> str:
    return re.sub(r"\s+", " ", text).strip()


def strip_tags(text: str) -> str:
    return collapse_whitespace(re.sub(r"<[^>]+>", " ", text))


def extract_section(text: str, start_marker: str, end_markers: list[str]) -> str | None:
    start = text.find(start_marker)
    if start < 0:
        return None

    end = len(text)
    for marker in end_markers:
        marker_index = text.find(marker, start + len(start_marker))
        if marker_index >= 0:
            end = min(end, marker_index)

    return text[start:end].strip()


def parse_failed_checks(summary_block: str) -> list[dict[str, str]]:
    failed_section = extract_section(
        summary_block,
        "### ❌ Failed checks",
        ["<details>\n<summary>✅ Passed checks", "<sub>", "<!-- pre_merge_checks_walkthrough_end -->"],
    )
    if failed_section is None:
        return []

    rows: list[dict[str, str]] = []
    for line in failed_section.splitlines():
        stripped = line.strip()
        if not stripped.startswith("|") or "Check name" in stripped or stripped.startswith("| :"):
            continue

        parts = [part.strip() for part in stripped.strip("|").split("|")]
        if len(parts) != 4:
            continue

        rows.append(
            {
                "name": parts[0],
                "status": parts[1],
                "explanation": parts[2],
                "resolution": parts[3],
            }
        )

    return rows


def parse_actionable_comments(actionable_block: str) -> dict[str, Any]:
    comment_count_match = re.search(r"Actionable comments posted:\s*(\d+)", actionable_block)
    count = int(comment_count_match.group(1)) if comment_count_match else 0

    comments: list[dict[str, str]] = []
    primary_block = actionable_block.split(
        "<details>\n<summary>🤖 Prompt for all review comments with AI agents</summary>",
        1,
    )[0]
    pattern = re.compile(
        r"<summary>"
        r"((?:[^<\n]+/)*[^<\n]+\.(?:cs|md|csproj|yaml|yml|json|txt|props|targets)|AGENTS\.md|CLAUDE\.md|README\.md|\.gitignore)"
        r" \((\d+)\)</summary><blockquote>\s*(.*?)\s*(?:(?:</blockquote></details>)|(?:</blockquote>))",
        re.S,
    )

    for path, _, body in pattern.findall(primary_block):
        finding_match = re.search(r"`([^`]+)`: \*\*(.*?)\*\*", body, re.S)
        prompt_match = re.search(r"<summary>🤖 Prompt for AI Agents</summary>\s*```(.*?)```", body, re.S)
        suggestion_match = re.search(r"<summary>✏️ 建议文案调整</summary>\s*```diff(.*?)```", body, re.S)

        body_without_details = body.split("<details>", 1)[0]
        description = strip_tags(body_without_details)
        if finding_match is not None:
            description = description.replace(f"{finding_match.group(1)}: {finding_match.group(2)}", "").strip()

        comments.append(
            {
                "path": path.strip(),
                "range": finding_match.group(1).strip() if finding_match else "",
                "title": collapse_whitespace(finding_match.group(2)) if finding_match else "",
                "description": description,
                "suggested_diff": suggestion_match.group(1).strip() if suggestion_match else "",
                "ai_prompt": prompt_match.group(1).strip() if prompt_match else "",
            }
        )

    prompt_match = re.search(
        r"<summary>🤖 Prompt for all review comments with AI agents</summary>\s*```(.*?)```",
        actionable_block,
        re.S,
    )

    return {
        "count": count,
        "comments": comments,
        "all_comments_prompt": prompt_match.group(1).strip() if prompt_match else "",
        "raw": actionable_block.strip(),
    }


def parse_test_report(block: str) -> dict[str, Any]:
    report: dict[str, Any] = {
        "raw": block.strip(),
        "stats": {},
        "failed_tests": [],
        "has_failed_tests": False,
    }

    summary_row_match = re.search(
        r"\|\s*\*?\*?(\d+)\*?\*?\s*\|\s*\*?\*?(\d+)\*?\*?\s*\|\s*\*?\*?(\d+)\*?\*?\s*\|"
        r"\s*\*?\*?(\d+)\*?\*?\s*\|\s*\*?\*?(\d+)\*?\*?\s*\|\s*\*?\*?(\d+)\*?\*?\s*\|\s*\*?\*?([^\|]+?)\*?\*?\s*\|",
        block,
    )
    if summary_row_match is not None:
        report["stats"] = {
            "tests": int(summary_row_match.group(1)),
            "passed": int(summary_row_match.group(2)),
            "failed": int(summary_row_match.group(3)),
            "skipped": int(summary_row_match.group(4)),
            "other": int(summary_row_match.group(5)),
            "flaky": int(summary_row_match.group(6)),
            "duration": summary_row_match.group(7).strip(),
        }

    failed_tests_section = extract_section(
        block,
        "### Failed Tests",
        ["### Slowest Tests", "### Insights", "<sub>", "[Github Test Reporter]"],
    )
    if failed_tests_section:
        lines = [line.strip("- ").strip() for line in failed_tests_section.splitlines()[1:] if line.strip()]
        report["failed_tests"] = lines
        report["has_failed_tests"] = True
    elif "No failed tests in this run." in block or "All tests passed!" in block:
        report["failed_tests"] = []
        report["has_failed_tests"] = False

    return report


def select_code_rabbit_summary(values: list[str]) -> str:
    for value in values:
        if "auto-generated comment: summarize by coderabbit.ai" in value:
            return value.strip()
    return ""


def select_actionable_comments(values: list[str]) -> str:
    for value in values:
        if "Actionable comments posted:" in value and "Prompt for all review comments with AI agents" in value:
            return value.strip()
    return ""


def select_test_reports(values: list[str]) -> list[str]:
    return [value.strip() for value in values if "CTRF PR COMMENT TAG:" in value or "### Test Results" in value]


def build_result(pr_number: int, branch: str, html_text: str) -> dict[str, Any]:
    embedded_data = extract_embedded_data(html_text)
    pull_request = embedded_data["payload"]["pullRequestsLayoutRoute"]["pullRequest"]
    clipboard_values = extract_clipboard_values(html_text)

    summary_block = select_code_rabbit_summary(clipboard_values)
    actionable_block = select_actionable_comments(clipboard_values)
    test_blocks = select_test_reports(clipboard_values)

    warnings: list[str] = []
    if not summary_block:
        warnings.append("CodeRabbit summary block was not found.")
    if not actionable_block:
        warnings.append("CodeRabbit actionable comments block was not found.")
    if not test_blocks:
        warnings.append("PR test-report block was not found.")

    return {
        "pull_request": {
            "number": int(pull_request["number"]),
            "title": pull_request["title"],
            "state": pull_request["state"],
            "head_branch": pull_request["headBranch"],
            "base_branch": pull_request["baseBranch"],
            "url": f"https://github.com/{OWNER}/{REPO}/pull/{pr_number}",
            "resolved_from_branch": branch,
        },
        "coderabbit_summary": {
            "failed_checks": parse_failed_checks(summary_block) if summary_block else [],
            "raw": summary_block,
        },
        "coderabbit_comments": parse_actionable_comments(actionable_block) if actionable_block else {},
        "test_reports": [parse_test_report(block) for block in test_blocks],
        "parse_warnings": warnings,
    }


def format_text(result: dict[str, Any]) -> str:
    lines: list[str] = []
    pr = result["pull_request"]
    lines.append(f"PR #{pr['number']}: {pr['title']}")
    lines.append(f"State: {pr['state']}")
    lines.append(f"Branch: {pr['head_branch']} -> {pr['base_branch']}")
    lines.append(f"URL: {pr['url']}")

    failed_checks = result["coderabbit_summary"].get("failed_checks", [])
    lines.append("")
    lines.append(f"Failed checks: {len(failed_checks)}")
    for check in failed_checks:
        lines.append(f"- {check['name']}: {check['status']}")
        lines.append(f"  Explanation: {check['explanation']}")
        lines.append(f"  Resolution: {check['resolution']}")

    comments = result.get("coderabbit_comments", {}).get("comments", [])
    lines.append("")
    lines.append(f"CodeRabbit actionable comments: {len(comments)}")
    for comment in comments:
        lines.append(f"- {comment['path']} {comment['range']}".rstrip())
        if comment["title"]:
            lines.append(f"  Title: {comment['title']}")
        if comment["description"]:
            lines.append(f"  Description: {comment['description']}")

    lines.append("")
    lines.append(f"Test reports: {len(result['test_reports'])}")
    for index, report in enumerate(result["test_reports"], start=1):
        stats = report.get("stats", {})
        if stats:
            lines.append(
                f"- Report {index}: tests={stats.get('tests')} passed={stats.get('passed')} "
                f"failed={stats.get('failed')} skipped={stats.get('skipped')} flaky={stats.get('flaky')} "
                f"duration={stats.get('duration')}"
            )
        else:
            lines.append(f"- Report {index}: no structured test stats parsed")

        if report["has_failed_tests"]:
            for failed_test in report["failed_tests"]:
                lines.append(f"  Failed test: {failed_test}")
        else:
            lines.append("  Failed tests: none reported")

    if result["parse_warnings"]:
        lines.append("")
        lines.append("Warnings:")
        for warning in result["parse_warnings"]:
            lines.append(f"- {warning}")

    return "\n".join(lines)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--branch", help="Override the current branch name.")
    parser.add_argument("--pr", type=int, help="Fetch a specific PR number instead of resolving from branch.")
    parser.add_argument("--format", choices=("text", "json"), default="text")
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    branch = args.branch or get_current_branch()
    pr_number = args.pr or resolve_pr_number(branch)
    url = f"https://github.com/{OWNER}/{REPO}/pull/{pr_number}"
    html_text = fetch_text(url)
    result = build_result(pr_number, branch, html_text)

    if args.format == "json":
        print(json.dumps(result, ensure_ascii=False, indent=2))
        return

    print(format_text(result))


if __name__ == "__main__":
    try:
        main()
    except Exception as error:  # noqa: BLE001
        print(str(error), file=sys.stderr)
        sys.exit(1)
