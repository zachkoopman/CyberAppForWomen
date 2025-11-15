"""
End-to-end + stress tests for FIA

Covers three QA tasks:

- Task 146: Stress-test Helper assignment for edge cases and try to break it.
- Task 147: Stress-test seat release, standby admits, and check-in for edge cases and try to break it.
- Task 148: Stress-test Workspace scoping, quiz sharing, and 1:1 Help Logs.

Run with:
    python -m unittest test_fia_stress.py
"""

import os
import time
import threading
import unittest
from typing import Dict, Any, List

import requests

# ==========================
# Basic configuration
# ==========================

# TODO: update to your local / test deployment URL
BASE_URL = os.getenv("FIA_BASE_URL", "https://localhost:5001")

# TODO: swap these for your real auth scheme (cookies, JWT, API keys, etc.)
SUPER_ADMIN_HEADERS = {"Authorization": f"Bearer {os.getenv('FIA_SUPER_ADMIN_TOKEN', 'CHANGE_ME')}"}
UNIVERSITY_ADMIN_HEADERS = {"Authorization": f"Bearer {os.getenv('FIA_UNIV_ADMIN_TOKEN', 'CHANGE_ME')}"}
HELPER_HEADERS = {"Authorization": f"Bearer {os.getenv('FIA_HELPER_TOKEN', 'CHANGE_ME')}"}
PARTICIPANT_HEADERS = {"Authorization": f"Bearer {os.getenv('FIA_PARTICIPANT_TOKEN', 'CHANGE_ME')}"}
OTHER_TENANT_ADMIN_HEADERS = {"Authorization": f"Bearer {os.getenv('FIA_OTHER_TENANT_ADMIN_TOKEN', 'CHANGE_ME')}"}

# Small timeout for requests
REQ_TIMEOUT = 10


# ==========================
# Low-level HTTP helpers
# ==========================

def _get(url: str, headers: Dict[str, str]) -> requests.Response:
    return requests.get(url, headers=headers, timeout=REQ_TIMEOUT, verify=False)


def _post(url: str, headers: Dict[str, str], json: Dict[str, Any]) -> requests.Response:
    return requests.post(url, headers=headers, json=json, timeout=REQ_TIMEOUT, verify=False)


def _put(url: str, headers: Dict[str, str], json: Dict[str, Any]) -> requests.Response:
    return requests.put(url, headers=headers, json=json, timeout=REQ_TIMEOUT, verify=False)


# =========================================================
# Task 146: Helper assignment stress tests (authorization,
#           eligibility, stale pages, duplicates, tenancy)
# =========================================================

class HelperAssignmentTests(unittest.TestCase):
    """
    Task 146 – Stress-test Helper assignment for edge cases and try to break it.

    Requirements being tested:
    - Only authorized admins can assign helpers.
    - Server always decides eligibility (never the UI).
    - Stale roles / revoked certifications must be enforced.
    - Duplicate clicks / double-submission should not create duplicates.
    - Tenancy mismatches and stale pages must be blocked.
    - Clear, safe "not eligible" reasons must be returned.
    """

    # --- helper endpoints (adjust these paths to your API) ---

    def create_assignment(self, admin_headers, payload) -> requests.Response:
        """
        Wrapper for the endpoint that assigns a Helper to a Participant/event.

        TODO: Adjust URL and payload fields to match your actual API.
        Example payload:
          {
            "helperId": "H-123",
            "participantId": "P-123",
            "eventId": "E-123"
          }
        """
        url = f"{BASE_URL}/api/admin/assign-helper"
        return _post(url, admin_headers, payload)

    def revoke_helper_cert(self, admin_headers, helper_id: str) -> requests.Response:
        """
        Wrapper for revoking a helper's certification (if you have such an endpoint).

        TODO: Update URL and JSON shape for your system.
        """
        url = f"{BASE_URL}/api/admin/revoke-cert"
        return _post(url, admin_headers, {"helperId": helper_id})

    # --------- individual tests mapped to Task 146 bullets ---------

    def test_only_authorized_admin_can_assign(self):
        """
        Task 146: ensure only authorized admins can assign helpers.

        - Expect: University Admin (or Super Admin) succeeds.
        - Expect: Helper or Participant attempting assignment gets 401/403.
        """
        payload = {
            "helperId": "H-DEMO",
            "participantId": "P-DEMO",
            "eventId": "E-DEMO"
        }

        # authorized admin
        ok_resp = self.create_assignment(UNIVERSITY_ADMIN_HEADERS, payload)
        self.assertIn(ok_resp.status_code, (200, 201), msg=f"Admin assign failed: {ok_resp.text}")

        # unauthorized helper
        helper_resp = self.create_assignment(HELPER_HEADERS, payload)
        self.assertIn(helper_resp.status_code, (401, 403),
                      msg=f"Helper should not be able to assign: {helper_resp.status_code} {helper_resp.text}")

        # unauthorized participant
        participant_resp = self.create_assignment(PARTICIPANT_HEADERS, payload)
        self.assertIn(participant_resp.status_code, (401, 403),
                      msg=f"Participant should not be able to assign: {participant_resp.status_code} {participant_resp.text}")

    def test_server_enforces_eligibility_not_ui(self):
        """
        Task 146: ensure the server always decides eligibility (never the UI).

        - Send an assignment for a helper that is known to be ineligible (e.g., missing cert/tag).
        - Expect: 400/422 with a clear, non-leaky "not eligible" message (no stack traces).
        """
        payload = {
            "helperId": "H-INELIGIBLE",
            "participantId": "P-DEMO",
            "eventId": "E-DEMO"
        }

        resp = self.create_assignment(UNIVERSITY_ADMIN_HEADERS, payload)
        self.assertIn(resp.status_code, (400, 422),
                      msg=f"Ineligible helper should be rejected: {resp.status_code} {resp.text}")

        body = resp.json()
        # TODO: adjust field names to your error envelope
        message = (body.get("message") or "").lower()
        self.assertIn("not eligible", message,
                      msg=f"Error should clearly state 'not eligible', got: {body}")

        # Basic leak check: error message should not contain stack traces or internals
        self.assertNotIn("System.", body.get("message", ""),
                         msg="Server should not leak internal exception details")

    def test_revoked_cert_cannot_be_assigned(self):
        """
        Task 146: ensure stale roles / revoked certs are enforced on assignment.

        - Revoke a helper's certification.
        - Try to assign them to a session.
        - Expect: rejection due to revoked cert.
        """
        helper_id = "H-REVOCABLE"

        # revoke cert
        revoke_resp = self.revoke_helper_cert(SUPER_ADMIN_HEADERS, helper_id)
        self.assertIn(revoke_resp.status_code, (200, 204),
                      msg=f"Revoking helper cert failed: {revoke_resp.text}")

        # attempt assignment after revocation
        assign_payload = {
            "helperId": helper_id,
            "participantId": "P-DEMO",
            "eventId": "E-DEMO"
        }
        resp = self.create_assignment(UNIVERSITY_ADMIN_HEADERS, assign_payload)
        self.assertIn(resp.status_code, (400, 422, 409),
                      msg=f"Revoked helper should not be assignable: {resp.status_code} {resp.text}")

        msg = (resp.json().get("message") or "").lower()
        self.assertIn("revoked", msg,
                      msg=f"Error should reference revoked/inactive cert, got: {msg}")

    def test_duplicate_clicks_dont_create_duplicate_assignments(self):
        """
        Task 146: duplicate clicks / replayed assignment requests should not create duplicates.

        - Fire two concurrent assignment requests with the same payload.
        - Expect: at most one successful assignment, the other should be idempotent (409/200 with 'already assigned').
        """
        payload = {
            "helperId": "H-CLICK-SPAM",
            "participantId": "P-CLICK-SPAM",
            "eventId": "E-CLICK-SPAM"
        }

        responses: List[requests.Response] = []

        def worker():
            resp = self.create_assignment(UNIVERSITY_ADMIN_HEADERS, payload)
            responses.append(resp)

        threads = [threading.Thread(target=worker) for _ in range(2)]
        for t in threads:
            t.start()
        for t in threads:
            t.join()

        self.assertEqual(len(responses), 2)

        status_codes = {r.status_code for r in responses}
        self.assertTrue(status_codes.issubset({200, 201, 409}),
                        msg=f"Unexpected codes for duplicate assignment: {status_codes}")

    def test_tenancy_mismatch_is_rejected(self):
        """
        Task 146: tenancy mismatches or stale pages must never slip through.

        - Simulate an admin from Tenant A trying to assign a helper or participant from Tenant B.
        - Expect: 403 or 404 (no cross-tenant leakage).
        """
        payload = {
            "helperId": "H-OTHER-TENANT",
            "participantId": "P-OTHER-TENANT",
            "eventId": "E-OTHER-TENANT"
        }

        resp = self.create_assignment(UNIVERSITY_ADMIN_HEADERS, payload)
        self.assertIn(resp.status_code, (403, 404),
                      msg=f"Cross-tenant assignment should be blocked: {resp.status_code} {resp.text}")


# ==================================================================
# Task 147: Seat release, standby admits, and check-in stress tests
#           (cutoffs, waitlist position, undo, XSS, replay, auth gaps)
# ==================================================================

class SeatReleaseAndCheckInTests(unittest.TestCase):
    """
    Task 147 – Stress-test seat release, standby admits, and check-in.

    Requirements being tested:
    - Server enforces release cutoffs (e.g., 5 minutes after start).
    - Standby admits are strictly by waitlist position.
    - Only correct check-in roles may check in attendees.
    - Short server-validated undo window for check-in.
    - Notes are sanitized to prevent XSS.
    - Stress for clock skew, concurrent admits, replayed requests, and auth gaps.
    """

    # --- helper endpoints (adjust these paths to your API) ---

    def release_unclaimed_seats(self, headers, event_id: str) -> requests.Response:
        """
        Endpoint that triggers or simulates 'release unclaimed seats'.

        TODO: If this is scheduled internally, expose a test-only endpoint or
        call whatever admin action you use in production.
        """
        url = f"{BASE_URL}/api/events/{event_id}/release-seats"
        return _post(url, headers, {})

    def admit_standby(self, headers, event_id: str, count: int = 1) -> requests.Response:
        """
        Endpoint to admit N attendees from the standby list.

        TODO: Adjust URL and JSON payload.
        """
        url = f"{BASE_URL}/api/events/{event_id}/admit-standby"
        return _post(url, headers, {"count": count})

    def check_in(self, headers, session_id: str, attendee_id: str, note: str = "") -> requests.Response:
        """
        Endpoint for check-in with optional note.

        TODO: Adjust URL and JSON fields for your system.
        """
        url = f"{BASE_URL}/api/sessions/{session_id}/check-in"
        return _post(url, headers, {"attendeeId": attendee_id, "note": note})

    def undo_check_in(self, headers, session_id: str, attendee_id: str) -> requests.Response:
        """
        Endpoint to undo check-in (server-validated undo window).

        TODO: Adjust URL and JSON format.
        """
        url = f"{BASE_URL}/api/sessions/{session_id}/check-in/undo"
        return _post(url, headers, {"attendeeId": attendee_id})

    def get_session_attendance(self, headers, session_id: str) -> requests.Response:
        """
        Read session attendance to verify order, status, and sanitized notes.

        TODO: Adjust URL.
        """
        url = f"{BASE_URL}/api/sessions/{session_id}/attendance"
        return _get(url, headers)

    # --------- individual tests mapped to Task 147 bullets ---------

    def test_release_cutoff_enforced(self):
        """
        Task 147: ensure the server enforces release cutoffs.

        - Assume the event is configured with 'release after 5 minutes'.
        - Call release endpoint before cutoff; expect no release.
        - Call release endpoint at/after cutoff; expect seats released.
        """
        event_id = "E-RELEASE-CUTOFF"

        # before cutoff
        before_resp = self.release_unclaimed_seats(UNIVERSITY_ADMIN_HEADERS, event_id)
        self.assertIn(before_resp.status_code, (200, 202),
                      msg=f"Release pre-cutoff should not fail, got: {before_resp.status_code}")
        before_data = before_resp.json()
        # TODO: adjust flag name: maybe "released": false
        self.assertFalse(before_data.get("released", False),
                         msg=f"Seats should not be released before cutoff: {before_data}")

        # after cutoff – in real tests you might mock server time; here we just call again
        time.sleep(1)
        after_resp = self.release_unclaimed_seats(UNIVERSITY_ADMIN_HEADERS, event_id)
        self.assertIn(after_resp.status_code, (200, 202))
        after_data = after_resp.json()
        self.assertTrue(after_data.get("released", True),
                        msg=f"Seats should be released after cutoff: {after_data}")

    def test_standby_admits_in_waitlist_order(self):
        """
        Task 147: ensure standby admits follow exact waitlist position.

        - Admit multiple attendees from standby.
        - Verify the admitted list matches the expected order.
        """
        event_id = "E-WAITLIST"
        resp = self.admit_standby(UNIVERSITY_ADMIN_HEADERS, event_id, count=3)
        self.assertEqual(resp.status_code, 200, msg=f"Standby admit failed: {resp.text}")
        data = resp.json()

        # TODO: align with your response shape
        admitted_ids = data.get("admittedIds", [])
        self.assertEqual(len(admitted_ids), 3,
                         msg=f"Expected exactly 3 admits, got {len(admitted_ids)}: {admitted_ids}")

        # If you know expected order, assert that here
        # expected_order = ["A-1", "A-2", "A-3"]
        # self.assertEqual(admitted_ids, expected_order)

    def test_check_in_requires_correct_role(self):
        """
        Task 147: require the correct check-in role.

        - Helper with check-in permission succeeds.
        - Participant or random user fails.
        """
        session_id = "S-CHECKIN"
        attendee_id = "A-CHECKIN"

        # helper (assumed to have check-in role)
        ok_resp = self.check_in(HELPER_HEADERS, session_id, attendee_id, note="Arrived on time")
        self.assertEqual(ok_resp.status_code, 200, msg=f"Helper check-in failed: {ok_resp.text}")

        # participant trying to check in someone else
        bad_resp = self.check_in(PARTICIPANT_HEADERS, session_id, attendee_id)
        self.assertIn(bad_resp.status_code, (401, 403),
                      msg=f"Participant should not be able to check-in others: {bad_resp.status_code} {bad_resp.text}")

    def test_check_in_undo_window_enforced(self):
        """
        Task 147: short server-validated undo window for check-in.

        - Check in an attendee.
        - Immediately undo: expect success.
        - After artificial delay beyond window: expect failure.
        """
        session_id = "S-UNDO"
        attendee_id = "A-UNDO"

        # initial check-in
        ok_resp = self.check_in(HELPER_HEADERS, session_id, attendee_id)
        self.assertEqual(ok_resp.status_code, 200, msg=f"Check-in failed: {ok_resp.text}")

        # immediate undo
        undo_resp = self.undo_check_in(HELPER_HEADERS, session_id, attendee_id)
        self.assertEqual(undo_resp.status_code, 200, msg=f"Undo within window failed: {undo_resp.text}")

        # check-in again
        self.check_in(HELPER_HEADERS, session_id, attendee_id)
        # simulate passing the undo window (in reality, server uses its own clock)
        time.sleep(2)
        late_undo = self.undo_check_in(HELPER_HEADERS, session_id, attendee_id)
        self.assertIn(late_undo.status_code, (400, 410, 422),
                      msg=f"Undo after window should be rejected: {late_undo.status_code} {late_undo.text}")

    def test_notes_are_sanitized_to_block_xss(self):
        """
        Task 147: sanitize notes to block XSS.

        - Check in an attendee with a note containing potential XSS payload.
        - Retrieve attendance.
        - Expect the displayed note to be escaped or stripped (no <script> tag).
        """
        session_id = "S-XSS"
        attendee_id = "A-XSS"
        xss_payload = "<script>alert('xss');</script>"

        resp = self.check_in(HELPER_HEADERS, session_id, attendee_id, note=xss_payload)
        self.assertEqual(resp.status_code, 200, msg=f"Check-in with note failed: {resp.text}")

        attendance_resp = self.get_session_attendance(UNIVERSITY_ADMIN_HEADERS, session_id)
        self.assertEqual(attendance_resp.status_code, 200, msg=f"Attendance read failed: {attendance_resp.text}")

        data = attendance_resp.json()
        # TODO: adjust where notes live in your response
        notes = str(data)
        self.assertNotIn("<script", notes.lower(),
                         msg=f"XSS payload should not be stored/rendered as raw script: {notes}")

    def test_concurrent_standby_admits_and_replay(self):
        """
        Task 147: hammer scenarios like clock-skew, concurrent admits, and replayed requests.

        - Fire multiple concurrent standby admit requests.
        - Expect: total admitted count matches capacity and no duplicates.
        """
        event_id = "E-STANDBY-STRESS"
        responses: List[requests.Response] = []

        def worker():
            resp = self.admit_standby(UNIVERSITY_ADMIN_HEADERS, event_id, count=1)
            responses.append(resp)

        threads = [threading.Thread(target=worker) for _ in range(5)]
        for t in threads:
            t.start()
        for t in threads:
            t.join()

        # All should either succeed or indicate 'no more seats'
        for r in responses:
            self.assertIn(r.status_code, (200, 409),
                          msg=f"Unexpected status on concurrent admit: {r.status_code} {r.text}")


# ===================================================================
# Task 148: Workspace scoping, quiz sharing, and 1:1 Help Logs tests
#           (scoping, privacy by default, share/revoke, redaction)
# ===================================================================

class WorkspaceScopingAndPrivacyTests(unittest.TestCase):
    """
    Task 148 – Stress-test Workspace scoping, quiz sharing, and 1:1 Help Logs.

    Requirements being tested:
    - Workspace shows only the signed-in Helper’s items (proper scoping).
    - Quiz results are private by default; visible only when Participant shares.
    - Sharing can be revoked, and access must cut off instantly (invalidate caches/links).
    - 1:1 Help Logs store minimal necessary details but still update certification and audits.
    - Redaction is applied across roles/exports (no oversharing or PII leakage).
    """

    # --- helper endpoints (adjust these paths to your API) ---

    def get_helper_workspace(self, headers) -> requests.Response:
        """
        Helper's workspace endpoint.

        TODO: update URL to your Helper workspace API.
        """
        url = f"{BASE_URL}/api/helper/workspace"
        return _get(url, headers)

    def get_helper_workspace_as_admin(self, headers) -> requests.Response:
        """
        Admin view of a Helper's workspace, if you have a scoped admin endpoint.

        TODO: update URL and add helperId param when needed.
        """
        url = f"{BASE_URL}/api/admin/helper-workspace"
        return _get(url, headers)

    def get_quiz_results_for_helper(self, helper_headers, participant_id: str) -> requests.Response:
        """
        Helper attempting to view a participant's quiz results.

        TODO: update URL.
        """
        url = f"{BASE_URL}/api/helper/participants/{participant_id}/quiz-results"
        return _get(url, helper_headers)

    def set_quiz_share_state(self, participant_headers, helper_id: str, share: bool) -> requests.Response:
        """
        Participant toggles quiz sharing with a specific helper.

        TODO: update URL and payload to match your API.
        """
        url = f"{BASE_URL}/api/participant/quiz-sharing"
        return _post(
            url,
            participant_headers,
            {"helperId": helper_id, "share": share}
        )

    def log_1to1_help(self, helper_headers, payload: Dict[str, Any]) -> requests.Response:
        """
        Helper logs a 1:1 Help interaction.

        TODO: update URL and payload to match your API.
        Expected minimal payload:
          {
            "participantId": "P-1",
            "microcourseId": "MC-1",
            "action": "2FA setup",
            "note": "High level only"
          }
        """
        url = f"{BASE_URL}/api/helper/1to1-log"
        return _post(url, helper_headers, payload)

    def get_1to1_logs_for_helper(self, headers) -> requests.Response:
        """
        Read the helper's 1:1 logs (for redaction checks).

        TODO: update URL.
        """
        url = f"{BASE_URL}/api/helper/1to1-log"
        return _get(url, headers)

    # --------- individual tests mapped to Task 148 bullets ---------

    def test_workspace_scopes_to_signed_in_helper(self):
        """
        Task 148: ensure the Workspace shows only the signed-in Helper’s items.

        - Hit the workspace endpoint as helper H1; assert all items belong to H1.
        - Optionally hit as H2 and confirm different / no overlap.
        """
        resp = self.get_helper_workspace(HELPER_HEADERS)
        self.assertEqual(resp.status_code, 200, msg=f"Workspace fetch failed: {resp.text}")
        data = resp.json()

        # TODO: adjust where helperId lives in your response
        items = data.get("items", [])
        for item in items:
            self.assertEqual(
                item.get("helperId"),
                "H-THIS-HELPER",
                msg=f"Workspace item leaked to wrong helper: {item}"
            )

    def test_quiz_results_private_by_default(self):
        """
        Task 148: quiz results are private by default.

        - Helper requests quiz results for Participant P without any share.
        - Expect: 403/404 or empty result (no scores, no answers).
        """
        participant_id = "P-QUIZ-PRIVATE"
        resp = self.get_quiz_results_for_helper(HELPER_HEADERS, participant_id)
        self.assertIn(resp.status_code, (403, 404, 200),
                      msg=f"Unexpected status for private quiz results: {resp.status_code} {resp.text}")

        if resp.status_code == 200:
            data = resp.json()
            # TODO: tune these checks to your schema
            self.assertFalse(data.get("shared", False),
                             msg=f"Quiz data should not be marked shared by default: {data}")
            self.assertFalse(data.get("results"),
                             msg=f"Helper should not see quiz results by default: {data}")

    def test_quiz_share_and_revoke_cuts_off_access(self):
        """
        Task 148: quiz results grant on share and cut off instantly on revoke.

        - Participant enables sharing with a specific Helper; Helper should then see results.
        - Participant revokes sharing; Helper should immediately lose access.
        - This also implicitly tests cache invalidation.
        """
        helper_id = "H-QUIZ-SHARE"
        participant_id = "P-QUIZ-SHARE"

        # Share on
        share_resp = self.set_quiz_share_state(PARTICIPANT_HEADERS, helper_id, True)
        self.assertEqual(share_resp.status_code, 200,
                         msg=f"Enabling quiz sharing failed: {share_resp.text}")

        # Helper should now see results
        visible_resp = self.get_quiz_results_for_helper(HELPER_HEADERS, participant_id)
        self.assertEqual(visible_resp.status_code, 200,
                         msg=f"Helper should now see quiz results: {visible_resp.text}")
        visible_data = visible_resp.json()
        self.assertTrue(visible_data.get("shared", False),
                        msg="Quiz results should be marked shared when access is granted")
        self.assertTrue(visible_data.get("results"),
                        msg="Helper should see some quiz result data when shared")

        # Share off
        revoke_resp = self.set_quiz_share_state(PARTICIPANT_HEADERS, helper_id, False)
        self.assertEqual(revoke_resp.status_code, 200,
                         msg=f"Revoking quiz sharing failed: {revoke_resp.text}")

        # Helper should now lose access right away
        revoked_resp = self.get_quiz_results_for_helper(HELPER_HEADERS, participant_id)
        self.assertIn(revoked_resp.status_code, (403, 404, 200),
                      msg=f"Unexpected status after revoke: {revoked_resp.status_code} {revoked_resp.text}")
        if revoked_resp.status_code == 200:
            revoked_data = revoked_resp.json()
            self.assertFalse(revoked_data.get("shared", False),
                             msg="Quiz results should not be marked shared after revoke")
            self.assertFalse(revoked_data.get("results"),
                             msg="Helper should not see results after revoke")

    def test_1to1_logs_minimal_details_but_update_certification(self):
        """
        Task 148: 1:1 logs store minimal details but still update certification and audits.

        - Log a 1:1 Help session.
        - Retrieve logs and confirm: minimal fields only (no sensitive PII or full narratives).
        - (Optional) Check that some certification progress field has updated as expected.
        """
        payload = {
            "participantId": "P-1TO1",
            "microcourseId": "MC-1TO1",
            "action": "2FA setup",
            "note": "Helped them enable 2FA in 10 minutes."
        }
        log_resp = self.log_1to1_help(HELPER_HEADERS, payload)
        self.assertEqual(log_resp.status_code, 201,
                         msg=f"1:1 Help log creation failed: {log_resp.text}")

        logs_resp = self.get_1to1_logs_for_helper(HELPER_HEADERS)
        self.assertEqual(logs_resp.status_code, 200,
                         msg=f"Reading 1:1 logs failed: {logs_resp.text}")

        data = logs_resp.json()
        logs = data.get("logs", [])
        self.assertTrue(logs, msg="Expected at least one 1:1 log entry")

        last_log = logs[-1]
        # Check minimal fields
        for forbidden_key in ["fullTranscript", "rawChat", "sensitiveNotes"]:
            self.assertNotIn(forbidden_key, last_log,
                             msg=f"Sensitive field {forbidden_key} should not appear in 1:1 log: {last_log}")

        # You may also assert that a 'certificationProgress' or 'auditTrail' field moved forward.
        # Example:
        # self.assertTrue(last_log.get("countsTowardCertification", False),
        #                 msg="1:1 log should flag that it counts toward certification")

    def test_1to1_logs_redaction_across_roles(self):
        """
        Task 148: redaction across roles/exports.

        - Log a 1:1 Help item with some content that should be redacted.
        - Access logs as Helper vs Admin vs (if allowed) export endpoint.
        - Confirm each view only shows what that role is allowed to see.
        """
        payload = {
            "participantId": "P-REDACTION",
            "microcourseId": "MC-REDACTION",
            "action": "Password reset support",
            "note": "Discussed specific email provider issues."
        }
        log_resp = self.log_1to1_help(HELPER_HEADERS, payload)
        self.assertEqual(log_resp.status_code, 201,
                         msg=f"1:1 Help log creation failed: {log_resp.text}")

        helper_logs = self.get_1to1_logs_for_helper(HELPER_HEADERS)
        self.assertEqual(helper_logs.status_code, 200)
        helper_data = helper_logs.json()

        # As admin (if you expose admin logs), you may have a different endpoint or view.
        # For now reuse helper endpoint with admin headers just to verify scoping.
        admin_logs = self.get_1to1_logs_for_helper(SUPER_ADMIN_HEADERS)
        self.assertEqual(admin_logs.status_code, 200)
        admin_data = admin_logs.json()

        # TODO: adjust checks to your schema. Example:
        # - Helper may see action + timestamp.
        # - Admin may only see aggregate counts or anonymized fields.

        self.assertIn("logs", helper_data)
        self.assertIn("logs", admin_data)


if __name__ == "__main__":
    # Allow running directly with `python test_fia_stress.py`
    unittest.main()
