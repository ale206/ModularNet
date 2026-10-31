#!/usr/bin/env python3
import sys
import os
import xml.etree.ElementTree as ET
import json

def calculate_coverage(cobertura_path):
    tree = ET.parse(cobertura_path)
    root = tree.getroot()
    
    lines_covered = int(root.attrib.get("lines-covered", 0))
    lines_valid = int(root.attrib.get("lines-valid", 0))
    branches_covered = int(root.attrib.get("branches-covered", 0))
    branches_valid = int(root.attrib.get("branches-valid", 0))
    
    if lines_valid > 0:
        line_pct = round((lines_covered / lines_valid) * 100)
    else:
        line_pct = round(float(root.attrib.get("line-rate", 0)) * 100)
        
    branch_pct = round((branches_covered / branches_valid) * 100) if branches_valid > 0 else 0
    
    return line_pct, branch_pct, lines_covered, lines_valid, branches_covered, branches_valid

def get_color(pct):
    if pct >= 80:
        return "#4c1", "brightgreen"
    elif pct >= 60:
        return "#dfb317", "yellow"
    elif pct >= 40:
        return "#fe7d37", "orange"
    else:
        return "#e05d44", "red"

def generate_svg(label, value, hex_color):
    val_len = len(value)
    val_width = val_len * 7 + 14
    val_box_width = val_width + 8
    total_width = 59 + val_box_width
    label_x = 295
    val_x = (59 + val_box_width / 2) * 10

    return f"""<svg xmlns="http://www.w3.org/2000/svg" width="{total_width}" height="20" role="img" aria-label="{label}: {value}">
  <title>{label}: {value}</title>
  <linearGradient id="s" x2="0" y2="100%">
    <stop offset="0" stop-color="#bbb" stop-opacity=".1"/>
    <stop offset="1" stop-opacity=".1"/>
  </linearGradient>
  <clipPath id="r">
    <rect width="{total_width}" height="20" rx="3" fill="#fff"/>
  </clipPath>
  <g clip-path="url(#r)">
    <rect width="59" height="20" fill="#555"/>
    <rect x="59" width="{val_box_width}" height="20" fill="{hex_color}"/>
    <rect width="{total_width}" height="20" fill="url(#s)"/>
  </g>
  <g fill="#fff" text-anchor="middle" font-family="Verdana,Geneva,DejaVu Sans,sans-serif" text-rendering="geometricPrecision" font-size="110">
    <text aria-hidden="true" x="{label_x}" y="150" fill="#010101" fill-opacity=".3" transform="scale(.1)" textLength="490">{label}</text>
    <text x="{label_x}" y="140" transform="scale(.1)" fill="#fff" textLength="490">{label}</text>
    <text aria-hidden="true" x="{val_x}" y="150" fill="#010101" fill-opacity=".3" transform="scale(.1)">{value}</text>
    <text x="{val_x}" y="140" transform="scale(.1)" fill="#fff">{value}</text>
  </g>
</svg>"""

def main():
    if len(sys.argv) < 3:
        print("Usage: generate-coverage-badge.py <cobertura_xml_path> <output_dir>")
        sys.exit(1)

    cobertura_path = sys.argv[1]
    output_dir = sys.argv[2]
    os.makedirs(output_dir, exist_ok=True)

    line_pct, branch_pct, lines_covered, lines_valid, branches_covered, branches_valid = calculate_coverage(cobertura_path)
    hex_color, color_name = get_color(line_pct)
    badge_value = f"{line_pct}%"

    # 1. Generate SVG badge
    svg_content = generate_svg("coverage", badge_value, hex_color)
    svg_path = os.path.join(output_dir, "coverage.svg")
    with open(svg_path, "w", encoding="utf-8") as f:
        f.write(svg_content)
    print(f"Generated {svg_path} ({badge_value}, {color_name})")

    # 2. Generate Shields.io endpoint JSON
    endpoint_data = {
        "schemaVersion": 1,
        "label": "coverage",
        "message": badge_value,
        "color": color_name
    }
    json_path = os.path.join(output_dir, "coverage.json")
    with open(json_path, "w", encoding="utf-8") as f:
        json.dump(endpoint_data, f, indent=2)
    print(f"Generated {json_path}")

    # 3. Export to GitHub Actions environment if present
    github_env = os.environ.get("GITHUB_ENV")
    if github_env and os.path.exists(github_env):
        with open(github_env, "a", encoding="utf-8") as f:
            f.write(f"COVERAGE_PCT={line_pct}\n")
            f.write(f"COVERAGE_COLOR={color_name}\n")

    # 4. Export to GitHub Step Summary if present
    step_summary = os.environ.get("GITHUB_STEP_SUMMARY")
    if step_summary:
        with open(step_summary, "a", encoding="utf-8") as f:
            f.write("### 🧪 Code Coverage Report\n\n")
            f.write(f"| Metric | Covered | Total | Percentage |\n")
            f.write(f"| :--- | :---: | :---: | :---: |\n")
            f.write(f"| **Lines** | {lines_covered} | {lines_valid} | **{line_pct}%** |\n")
            f.write(f"| **Branches** | {branches_covered} | {branches_valid} | **{branch_pct}%** |\n\n")

if __name__ == "__main__":
    main()
