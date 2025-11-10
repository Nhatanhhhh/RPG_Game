#!/usr/bin/env python3
"""
Unity scene duplicate local fileID fixer (safe, group-aware).

What it does:
- Backs up the scene file to <filename>.bak before any changes
- Scans YAML sections (--- !u!<class> &<fileID>)
- Detects duplicated GameObject IDs and treats each later duplicate as a new "group"
- For each duplicated GameObject group:
  - Assigns a fresh unique fileID for that group's GameObject
  - Collects related component/transform sections belonging to that group (by nearest GO occurrence)
  - Assigns fresh unique fileIDs for those related sections if they collide
  - Rewrites internal references within that group's sections to the new IDs
- Leaves the first occurrence of each fileID untouched

Notes:
- This aims to fix scene files broken by merges where identical localIDs got duplicated.
- It focuses on keeping references coherent within each duplicate group to allow Unity to load the scene again.
- It is conservative and will not edit assets outside the scene file.
"""
from __future__ import annotations

import argparse
import os
import re
import shutil
import sys
from typing import Dict, List, Tuple, Set, Optional


SECTION_HEADER_RE = re.compile(r'^---\s*!u!(\d+)\s*&(-?\d+)(?=\D).*?$')
FILEID_FIELD_RE = re.compile(r'(\bfileID:\s*)(-?\d+)')  # captures "fileID: 12345" or negatives
GO_BINDING_RE_TMPL = r'^\s*m_GameObject:\s*\{{\s*fileID:\s*{}\s*\}}'

# Matches problematic tokens such as:
#   fileID: 9223372036854775808G
#   fileID: 9223372036854775809T
#   fileID: 9999999999999
FILEID_TOKEN_SANITIZE_RE = re.compile(r'(\bfileID:\s*)(-?\d+)([A-Za-z]+)?')
INT32_MIN = -2147483648
INT32_MAX = 2147483647
INT64_MAX = 9223372036854775807

class Section:
    def __init__(self, class_id: int, file_id: int, start: int, end: int):
        self.class_id = class_id
        self.file_id = file_id
        self.start = start
        self.end = end  # exclusive

    def __repr__(self) -> str:
        return f"Section(class={self.class_id}, id={self.file_id}, [{self.start}, {self.end}))"


def parse_sections(lines: List[str]) -> List[Section]:
    sections: List[Section] = []
    current_start: Optional[int] = None
    current_class: Optional[int] = None
    current_id: Optional[int] = None

    for i, line in enumerate(lines):
        m = SECTION_HEADER_RE.match(line)
        if m:
            # close previous section
            if current_start is not None:
                sections.append(Section(current_class, current_id, current_start, i))
            current_class = int(m.group(1))
            current_id = int(m.group(2))
            current_start = i
    # close last
    if current_start is not None and current_class is not None and current_id is not None:
        sections.append(Section(current_class, current_id, current_start, len(lines)))
    return sections


def collect_used_ids(sections: List[Section]) -> Set[int]:
    return {s.file_id for s in sections}


def next_free_id(used: Set[int], start_from: int = 2000000000) -> int:
    """
    Generate a new positive id in a safe range, independent of existing huge/overflowing ids.
    We purposely start from a fixed high base and increment until unused.
    """
    cand = start_from
    while cand in used:
        cand += 1
    used.add(cand)
    return cand


def header_replace_fileid(lines: List[str], section: Section, new_id: int) -> None:
    # Replace only in the header line
    header_line = lines[section.start]
    # Header is like: --- !u!<class> &<id><optional trailing text>
    # Replace only the id after '&', keep trailing text intact
    new_header = re.sub(r'(&)(-?\d+)', f"&{new_id}", header_line, count=1)
    lines[section.start] = new_header


def replace_fileid_in_range(lines: List[str], start: int, end: int, old_id: int, new_id: int) -> None:
    # Replace occurrences of "fileID: old_id" to "fileID: new_id" within [start, end)
    for i in range(start, end):
        line = lines[i]
        # quick path
        if str(old_id) not in line:
            continue
        def _sub(m: re.Match) -> str:
            num = int(m.group(2))
            if num == old_id:
                return f"{m.group(1)}{new_id}"
            return m.group(0)
        lines[i] = FILEID_FIELD_RE.sub(_sub, line)

def replace_fileid_globally(lines: List[str], old_id: int, new_id: int) -> int:
    """
    Replace all occurrences of "fileID: old_id" in the entire file.
    Returns number of lines changed (approximation).
    """
    changed = 0
    for i, line in enumerate(lines):
        if str(old_id) not in line:
            continue
        def _sub(m: re.Match) -> str:
            num = int(m.group(2))
            if num == old_id:
                return f"{m.group(1)}{new_id}"
            return m.group(0)
        new_line = FILEID_FIELD_RE.sub(_sub, line)
        if new_line != line:
            lines[i] = new_line
            changed += 1
    return changed


def find_go_occurrence_indices(sections: List[Section], go_id: int) -> List[int]:
    return [idx for idx, s in enumerate(sections) if s.class_id == 1 and s.file_id == go_id]


def build_go_groups(sections: List[Section]) -> Dict[Tuple[int, int], List[int]]:
    """
    For each GameObject fileID that appears multiple times, map each occurrence (by ordinal index)
    to the list of section indices considered part of its group.
    Group heuristic:
      - A GO occurrence at section index i owns:
          * itself (i)
          * subsequent sections referencing m_GameObject: {fileID: <that GO id>}
            until the next GO section with the same GO id (or EOF)
    """
    # Map GO id -> list of section indices where that GO appears
    go_occurrences: Dict[int, List[int]] = {}
    for idx, s in enumerate(sections):
        if s.class_id == 1:
            go_occurrences.setdefault(s.file_id, []).append(idx)

    groups: Dict[Tuple[int, int], List[int]] = {}
    for go_id, occ_idxs in go_occurrences.items():
        if len(occ_idxs) <= 1:
            continue  # not duplicated; skip grouping
        for j, sec_idx in enumerate(occ_idxs):
            start_idx = sec_idx
            end_idx = occ_idxs[j + 1] if j + 1 < len(occ_idxs) else len(sections)
            # collect sections in (start_idx, end_idx) that bind to this GO
            owned: List[int] = [sec_idx]
            for k in range(sec_idx + 1, end_idx):
                owned.append(k)  # tentatively include; filtering done later with text test
            groups[(go_id, j)] = owned
    return groups


def filter_owned_by_text_binding(lines: List[str], sections: List[Section], go_id: int, owned_indices: List[int]) -> List[int]:
    """
    Keep only sections that either:
      - are the GO itself (class_id == 1, file_id == go_id)
      - contain a binding line 'm_GameObject: { fileID: <go_id> }'
    """
    filtered: List[int] = []
    pat = re.compile(GO_BINDING_RE_TMPL.format(re.escape(str(go_id))))
    for idx in owned_indices:
        s = sections[idx]
        if s.class_id == 1 and s.file_id == go_id:
            filtered.append(idx)
            continue
        # scan section body for a GO binding
        bound = False
        for ln in range(s.start, s.end):
            if pat.search(lines[ln]):
                bound = True
                break
        if bound:
            filtered.append(idx)
    return filtered


def sanitize_invalid_fileids(lines: List[str], used_ids: Set[int]) -> int:
    """
    Scan the entire file for invalid fileID tokens (suffix letters or out-of-range numbers)
    and remap them to safe unique positive IDs. Maintains a mapping so identical bad tokens
    become identical new IDs.
    """
    remap_by_token: Dict[str, int] = {}
    total = 0

    def needs_fix(num_str: str, suffix: Optional[str]) -> bool:
        try:
            val = int(num_str)
        except ValueError:
            return True
        if suffix:  # trailing letters like 'G', 'T'
            return True
        # Extremely large numbers (>= 2^63) overflow, and outside int32 are suspicious in scenes
        if abs(val) > INT64_MAX or val < INT32_MIN or val > INT32_MAX:
            return True
        return False

    for i, line in enumerate(lines):
        if "fileID:" not in line:
            continue

        def _sub(m: re.Match) -> str:
            prefix = m.group(1)
            num_str = m.group(2)
            suffix = m.group(3)
            token_key = f"{num_str}{suffix or ''}"
            if not needs_fix(num_str, suffix):
                return m.group(0)
            # assign new id consistently per bad token
            if token_key not in remap_by_token:
                remap_by_token[token_key] = next_free_id(used_ids)
            new_id = remap_by_token[token_key]
            nonlocal total
            total += 1
            return f"{prefix}{new_id}"

        new_line = FILEID_TOKEN_SANITIZE_RE.sub(_sub, line)
        if new_line is not line:
            lines[i] = new_line

    return total


def sanitize_invalid_section_headers(lines: List[str], sections: List[Section], used_ids: Set[int]) -> Tuple[int, Dict[int, int]]:
    """
    For any section with an invalid/overflowing &fileID in the header, assign a new id
    and rewrite:
      - the header anchor (&old -> &new)
      - all global "fileID: old" references in the file
    Returns (num_changes, remap) where remap is old->new applied.
    """
    num_changes = 0
    remap: Dict[int, int] = {}
    for sec in sections:
        old_id = sec.file_id
        if abs(old_id) > INT64_MAX or old_id < INT32_MIN or old_id > INT32_MAX:
            if old_id in remap:
                new_id = remap[old_id]
            else:
                new_id = next_free_id(used_ids)
                remap[old_id] = new_id
            # header replace
            header_replace_fileid(lines, sec, new_id)
            # global references replace
            replace_fileid_globally(lines, old_id, new_id)
            num_changes += 1
    return num_changes, remap


def sanitize_duplicate_section_headers(lines: List[str], sections: List[Section], used_ids: Set[int]) -> int:
    """
    Ensure all section header &fileIDs are unique, even if they are "valid" numbers.
    For any repeated id, keep the first occurrence, and for each subsequent section:
      - assign a fresh id
      - update the section header
      - update 'fileID: old' tokens within that section body only (to keep local coherence)
    """
    seen: Set[int] = set()
    changed = 0
    for sec in sections:
        old_id = sec.file_id
        if old_id not in seen:
            seen.add(old_id)
            continue
        # duplicate; assign new
        new_id = next_free_id(used_ids)
        header_replace_fileid(lines, sec, new_id)
        replace_fileid_in_range(lines, sec.start, sec.end, old_id, new_id)
        changed += 1
    return changed


def main() -> int:
    parser = argparse.ArgumentParser(description="Fix duplicate local fileIDs in a Unity .unity scene file.")
    parser.add_argument("--scene", required=True, help="Path to the .unity scene file (e.g., Assets/Scenes/RPG.unity)")
    parser.add_argument("--dry-run", action="store_true", help="Analyze and report changes without writing")
    args = parser.parse_args()

    scene_path = args.scene
    if not os.path.isfile(scene_path):
        print(f"Error: Scene file not found: {scene_path}", file=sys.stderr)
        return 2

    with open(scene_path, "r", encoding="utf-8") as f:
        lines = f.readlines()

    sections = parse_sections(lines)
    if not sections:
        print("No sections found. Is this a valid Unity YAML scene?")
        return 3

    used_ids = collect_used_ids(sections)

    # First pass: sanitize obviously bad fileID tokens across the whole file
    sanitized_changes = sanitize_invalid_fileids(lines, used_ids)
    # Re-parse sections if header boundaries may have shifted minimally
    sections = parse_sections(lines)

    # Second pass: sanitize invalid/overflowing section header anchors (&id)
    header_changes, header_remap = sanitize_invalid_section_headers(lines, sections, used_ids)
    sanitized_changes += header_changes

    # Third pass: ensure all section header ids are unique (deduplicate)
    # Re-parse to ensure sections reflect any header edits
    sections = parse_sections(lines)
    dedup_changes = sanitize_duplicate_section_headers(lines, sections, used_ids)
    sanitized_changes += dedup_changes

    # Build GO duplicate groups
    groups = build_go_groups(sections)
    if not groups:
        if sanitized_changes == 0:
            print("No duplicated GameObject IDs detected. Nothing to fix.")
            return 0

    total_changes = 0
    # Process each duplicated GO occurrence except the first occurrence per GO id
    for (go_id, occ_ordinal), owned_indices in groups.items():
        if occ_ordinal == 0:
            # first occurrence is canonical; skip
            continue

        # Filter owned sections to only those bound to this GO by text
        owned_indices = filter_owned_by_text_binding(lines, sections, go_id, owned_indices)
        if not owned_indices:
            continue

        # Map old -> new ids for this group (start with GO id remap)
        remap: Dict[int, int] = {}
        new_go_id = next_free_id(used_ids)
        remap[go_id] = new_go_id

        # Assign new IDs for each section if it collides with an existing id that would cause duplicates
        # even if not strictly necessary, we keep section header &ids unique within file
        for idx in owned_indices:
            sec = sections[idx]
            old_id = sec.file_id
            if old_id in remap:
                # already assigned (e.g., GO)
                continue
            if old_id in used_ids:
                # If this id appears elsewhere (which it will by definition here for duplicates),
                # ensure a fresh unique id
                new_id = next_free_id(used_ids)
                remap[old_id] = new_id

        # Apply header id changes
        for idx in owned_indices:
            sec = sections[idx]
            if sec.file_id in remap:
                header_replace_fileid(lines, sec, remap[sec.file_id])
                total_changes += 1

        # Update references within the owned sections to keep them coherent
        for idx in owned_indices:
            sec = sections[idx]
            for old_id, new_id in remap.items():
                replace_fileid_in_range(lines, sec.start, sec.end, old_id, new_id)

    total_changes += sanitized_changes

    if total_changes == 0:
        print("Detected duplicates or invalid tokens but no changes were necessary.")
        return 0

    if args.dry_run:
        print(f"[Dry run] Would apply {total_changes} section header updates, token sanitizations, and internal reference rewrites.")
        return 0

    # Backup
    backup_path = scene_path + ".bak"
    if not os.path.exists(backup_path):
        shutil.copyfile(scene_path, backup_path)
        print(f"Backup created: {backup_path}")
    else:
        print(f"Backup already exists: {backup_path}")

    with open(scene_path, "w", encoding="utf-8", newline="") as f:
        f.writelines(lines)

    print(f"Applied fixes. Updated {total_changes} sections/tokens. Try opening the scene in Unity.")
    return 0


if __name__ == "__main__":
    sys.exit(main())


