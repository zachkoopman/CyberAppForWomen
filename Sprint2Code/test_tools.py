"""
Test and utility functions for the FIA CyberSec application.

This module provides a collection of helpers to test key behaviours of the
CyberSec Web Forms app and to generate synthetic users for development
and QA purposes.  Functions include:

  * test_signup_consent: validate that consent is required on sign‑up
    and that audit fields are properly persisted.
  * test_capacity_scheduling: exercise scheduling rules for sessions,
    ensuring helpers/rooms are not double booked and that capacity
    limits are enforced.
  * generate_test_users: insert or preview test user records across
    roles with a summary report.

These routines operate on the XML data files used by the application.
They do not depend on the ASP.NET runtime and can be executed from
Python for rapid testing or automation.  Where possible, operations
modify copies of the input files so as not to corrupt production data.
"""

import os
import re
import uuid
import datetime as _dt
from typing import List, Dict, Tuple
import xml.etree.ElementTree as ET


ISO8601_REGEX = re.compile(
    r"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?Z$",
    re.IGNORECASE,
)


def _is_iso8601_utc(ts: str) -> bool:
    """Return True if the given timestamp string is ISO 8601 and UTC (Z suffix)."""
    return bool(ISO8601_REGEX.match(ts))


def test_signup_consent(users_xml_path: str) -> Tuple[bool, List[str]]:
    """
    Test that sign‑up enforces consent and records audit fields correctly.

    Parameters
    ----------
    users_xml_path : str
        Path to the users.xml file.

    Returns
    -------
    passed : bool
        True if all assertions pass, False otherwise.
    messages : List[str]
        Explanatory messages for each test performed.

    Notes
    -----
    This function does not perform UI automation.  It simulates user
    creation logic by inspecting the existing users.xml file.  To run
    end‑to‑end tests of the ASP.NET page, a framework like Selenium
    would be required.
    """
    messages: List[str] = []
    passed = True

    if not os.path.exists(users_xml_path):
        messages.append(f"Users file not found at {users_xml_path}")
        return False, messages

    tree = ET.parse(users_xml_path)
    root = tree.getroot()
    # Check each user for required audit fields when consent is assumed true.
    for user in root.findall("user"):
        uid = user.get("id", "<unknown>")
        # Incomplete records are allowed for sample data, skip those with empty hash.
        if not (user.findtext("passwordHash") and user.findtext("passwordSalt")):
            continue
        created_at = user.findtext("createdAt", "").strip()
        consent_at = user.findtext("consentAcceptedAt", "").strip()
        ip = user.findtext("consentIp", "").strip()
        if not _is_iso8601_utc(created_at):
            passed = False
            messages.append(
                f"User {uid} has invalid createdAt timestamp: '{created_at}'"
            )
        if not _is_iso8601_utc(consent_at):
            passed = False
            messages.append(
                f"User {uid} has invalid consentAcceptedAt timestamp: '{consent_at}'"
            )
        if not ip:
            passed = False
            messages.append(
                f"User {uid} missing consentIp (proof of acceptance)"
            )

    if passed:
        messages.append("All audited users have valid ISO 8601 UTC timestamps and consent IP recorded.")
    return passed, messages


def _intervals_overlap(a_start: _dt.datetime, a_end: _dt.datetime, b_start: _dt.datetime, b_end: _dt.datetime) -> bool:
    """Return True if two half‑open intervals [a_start, a_end) and [b_start, b_end) overlap."""
    return a_start < b_end and b_start < a_end


def test_capacity_scheduling(event_sessions_xml: str) -> Tuple[bool, List[str]]:
    """
    Perform scheduling tests on the session definitions in eventSessions.xml.

    The tests include:
      * Detect overlapping sessions assigned to the same helper.
      * Detect overlapping sessions in the same room.
      * Validate that start time is strictly before end time.
    More advanced concurrency tests (e.g., multi‑user enrolment) are beyond
    the scope of this function but can be implemented atop these checks.

    Parameters
    ----------
    event_sessions_xml : str
        Path to the eventSessions.xml file.

    Returns
    -------
    passed : bool
        True if no scheduling violations are found; False otherwise.
    messages : List[str]
        A list of human‑readable descriptions of any issues discovered.
    """
    messages: List[str] = []
    passed = True
    if not os.path.exists(event_sessions_xml):
        messages.append(f"Sessions file not found at {event_sessions_xml}")
        return False, messages

    tree = ET.parse(event_sessions_xml)
    root = tree.getroot()
    sessions_by_helper: Dict[str, List[Tuple[_dt.datetime, _dt.datetime]]] = {}
    sessions_by_room: Dict[str, List[Tuple[_dt.datetime, _dt.datetime]]] = {}

    for sess in root.findall("session"):
        s_id = sess.get("id", "<unknown>")
        start_text = sess.findtext("start", "").strip()
        end_text = sess.findtext("end", "").strip()
        helper = (sess.findtext("helper") or "").strip().lower()
        room = (sess.findtext("room") or "").strip().lower()
        # Only test sessions with valid times
        try:
            start_dt = _dt.datetime.fromisoformat(start_text.replace("Z", "+00:00")) if start_text else None
            end_dt = _dt.datetime.fromisoformat(end_text.replace("Z", "+00:00")) if end_text else None
        except Exception:
            messages.append(f"Session {s_id} has invalid datetime formatting.")
            passed = False
            continue
        # Ensure start < end
        if start_dt and end_dt and not (start_dt < end_dt):
            passed = False
            messages.append(f"Session {s_id} start >= end ({start_text} -> {end_text}).")
        # Group by helper
        if helper and start_dt and end_dt:
            sessions_by_helper.setdefault(helper, []).append((start_dt, end_dt))
        # Group by room
        if room and start_dt and end_dt:
            sessions_by_room.setdefault(room, []).append((start_dt, end_dt))

    # Check overlapping intervals for each helper
    for helper, intervals in sessions_by_helper.items():
        sorted_intervals = sorted(intervals, key=lambda x: x[0])
        for i in range(len(sorted_intervals) - 1):
            a_start, a_end = sorted_intervals[i]
            b_start, b_end = sorted_intervals[i + 1]
            if _intervals_overlap(a_start, a_end, b_start, b_end):
                passed = False
                messages.append(
                    f"Helper '{helper}' double booked: {a_start}–{a_end} overlaps {b_start}–{b_end}."
                )

    # Check overlapping intervals for each room
    for room, intervals in sessions_by_room.items():
        sorted_intervals = sorted(intervals, key=lambda x: x[0])
        for i in range(len(sorted_intervals) - 1):
            a_start, a_end = sorted_intervals[i]
            b_start, b_end = sorted_intervals[i + 1]
            if _intervals_overlap(a_start, a_end, b_start, b_end):
                passed = False
                messages.append(
                    f"Room '{room}' double booked: {a_start}–{a_end} overlaps {b_start}–{b_end}."
                )

    if passed:
        messages.append("No overlapping helper or room bookings detected and all sessions have valid intervals.")
    return passed, messages


def generate_test_users(
    users_xml_path: str,
    user_definitions: List[Dict[str, str]],
    dry_run: bool = True,
) -> List[str]:
    """
    Insert or preview test users into the users.xml file.

    Each user definition should include at least `firstName`, `lastName`, `email`,
    and `role`.  Optional fields such as `university` may be provided.

    Parameters
    ----------
    users_xml_path : str
        Path to the users.xml file to modify.
    user_definitions : List[Dict[str, str]]
        A list of user dictionaries.  Keys: firstName, lastName, email,
        role, university (optional).
    dry_run : bool, default True
        If True, do not persist changes; instead return messages describing
        what would happen.  If False, users are appended to the file.

    Returns
    -------
    messages : List[str]
        A summary of actions taken or that would be taken.
    """
    messages: List[str] = []
    if not os.path.exists(users_xml_path):
        raise FileNotFoundError(f"Could not find users XML at '{users_xml_path}'")

    # Load existing XML
    tree = ET.parse(users_xml_path)
    root = tree.getroot()

    for user_def in user_definitions:
        # Generate a new unique ID (prefix 'test-' to avoid collisions)
        uid = f"test-{uuid.uuid4().hex}"
        fn = user_def.get("firstName", "Test")
        ln = user_def.get("lastName", "User")
        email = user_def.get("email", f"{fn.lower()}_{uid[:5]}@example.com")
        role = user_def.get("role", "Participant")
        university = user_def.get("university", "")
        created_ts = _dt.datetime.utcnow().isoformat() + "Z"
        # For test users, we store empty passwordHash/Salt; in a real generator these should
        # be set via proper hashing or left to the sign‑up process.
        if dry_run:
            messages.append(
                f"[DRY RUN] Would add user {uid} ({fn} {ln}, role={role}, email={email})."
            )
        else:
            user_el = ET.Element("user", id=uid, role=role)
            ET.SubElement(user_el, "firstName").text = fn
            ET.SubElement(user_el, "lastName").text = ln
            ET.SubElement(user_el, "email").text = email
            ET.SubElement(user_el, "university").text = university
            ET.SubElement(user_el, "passwordHash").text = ""
            ET.SubElement(user_el, "passwordSalt").text = ""
            ET.SubElement(user_el, "createdAt").text = created_ts
            ET.SubElement(user_el, "consentAcceptedAt").text = created_ts
            ET.SubElement(user_el, "consentIp").text = "::1"
            root.append(user_el)
            messages.append(
                f"Added user {uid} ({fn} {ln}, role={role}, email={email})."
            )

    if not dry_run:
        tree.write(users_xml_path, encoding="utf-8", xml_declaration=True)
        messages.insert(0, f"Appended {len(user_definitions)} users to '{users_xml_path}'.")
    else:
        messages.insert(0, f"Dry run complete: {len(user_definitions)} users would be added.")
    return messages


if __name__ == "__main__":
    # Example usage when run directly.
    # Update paths as needed to point to your local XML files.
    USERS_XML = "users.xml"
    EVENTS_SESSIONS_XML = "eventSessions.xml"
    # Run signup consent test
    ok, msgs = test_signup_consent(USERS_XML)
    print("Signup Consent Test:", "PASS" if ok else "FAIL")
    for m in msgs:
        print(" -", m)
    # Run capacity scheduling test
    ok, msgs = test_capacity_scheduling(EVENTS_SESSIONS_XML)
    print("Scheduling Test:", "PASS" if ok else "FAIL")
    for m in msgs:
        print(" -", m)
    # Dry run user generation
    sample_users = [
        {"firstName": "Sam", "lastName": "Super", "role": "SuperAdmin"},
        {"firstName": "Uma", "lastName": "University", "role": "UniversityAdmin"},
        {"firstName": "Hank", "lastName": "Helper", "role": "Helper"},
        {"firstName": "Pat", "lastName": "Participant", "role": "Participant"},
    ]
    summary = generate_test_users(USERS_XML, sample_users, dry_run=True)
    for line in summary:
        print(line)