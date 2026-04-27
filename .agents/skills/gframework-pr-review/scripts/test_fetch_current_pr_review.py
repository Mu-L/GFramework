#!/usr/bin/env python3
"""Regression tests for the GFramework PR review fetch helper."""

from __future__ import annotations

import importlib.util
from pathlib import Path
import unittest


SCRIPT_PATH = Path(__file__).with_name("fetch_current_pr_review.py")
MODULE_SPEC = importlib.util.spec_from_file_location("fetch_current_pr_review", SCRIPT_PATH)
if MODULE_SPEC is None or MODULE_SPEC.loader is None:
    raise RuntimeError(f"Unable to load module from {SCRIPT_PATH}.")

MODULE = importlib.util.module_from_spec(MODULE_SPEC)
MODULE_SPEC.loader.exec_module(MODULE)


class ParseFailedTestDetailsTests(unittest.TestCase):
    """Cover failed-test table parsing edge cases for CTRF comments."""

    def test_parse_failed_test_details_ignores_trailing_columns(self) -> None:
        """Extra columns should not prevent extracting the name and failure message."""
        block = """
### ❌ **Some tests failed!**
<table>
  <tbody>
    <tr>
      <td>❌ RegisterMigration_During_Cache_Rebuild_Should_Not_Leave_Stale_Type_Cache</td>
      <td><pre>Expected: False\nBut was: True</pre></td>
      <td>failed</td>
      <td>35.3s</td>
    </tr>
  </tbody>
</table>
"""

        details = MODULE.parse_failed_test_details(block)

        self.assertEqual(
            details,
            [
                {
                    "name": "RegisterMigration_During_Cache_Rebuild_Should_Not_Leave_Stale_Type_Cache",
                    "failure_message": "Expected: False\nBut was: True",
                }
            ],
        )


if __name__ == "__main__":
    unittest.main()
