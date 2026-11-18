#!/usr/bin/env python3
"""Generate the Tax Buddy logo SVGs.

Usage: generate.py <directions_dir> [<adopted_assets_dir>]

The first folder receives the three directions. The second, when given, receives
the adopted identity (direction C) under the file names the prototype and the
site use: prototypes/assets, then copied to src/TaxBuddy.Web/wwwroot/images.
"""
import os
import sys

OUT = sys.argv[1]

FOREST, INK, PAPER = "#245C46", "#1E332C", "#F7F7F2"
SAGE, SAGE_TEXT, DEEP = "#B7D8A4", "#EEF1E7", "#111C18"

# Wordmark "Tax Buddy": original monoline letterforms, centre lines.
# Cap top y=8, x-height y=18, baseline y=40, descender y=50.
# Directions A and B: one path, weight 4.6, width 0..223.
WORD = (
    "M0 8H24M12 8V40"                                   # T
    "M44 29A11 11 0 1 0 22 29A11 11 0 1 0 44 29M44 18V40"  # a
    "M52 18L72 40M72 18L52 40"                          # x
    "M88 8V40M88 8H97A7.5 7.5 0 0 1 97 23H88M97 23H99A8.5 8.5 0 0 1 99 40H88"  # B
    "M116 18V30A10 10 0 0 0 136 30V18M136 30V40"        # u
    "M166 29A11 11 0 1 0 144 29A11 11 0 1 0 166 29M166 6V40"  # d
    "M196 29A11 11 0 1 0 174 29A11 11 0 1 0 196 29M196 6V40"  # d
    "M203 18L213 40M223 18L208.5 50"                    # y
)
# Direction C (the adopted identity): heavier weight 5.4, re-spaced, split into
# two words so each can take its own colour. Width 0..227. Since 2026-11-19 the
# letters are filled outlines, not strokes: each stroke of a letter is written as
# its own closed shape (a capsule, a ring or a half ring), all wound the same way,
# so a non-zero fill paints their union. The shapes overlap; they are not merged.
C_WEIGHT = 5.4
C_TAX = [
    ("line", 0, 8, 24, 8), ("line", 12, 8, 12, 40),                              # T
    ("ring", 33.5, 29, 11), ("line", 44.5, 18, 44.5, 40),                        # a
    ("line", 53.5, 18, 73.5, 40), ("line", 73.5, 18, 53.5, 40),                  # x
]
C_BUDDY = [
    ("line", 88.5, 8, 88.5, 40), ("line", 88.5, 8, 97.5, 8), ("half", 97.5, 15.5, 7.5, "right"),
    ("line", 88.5, 23, 99.5, 23), ("half", 99.5, 31.5, 8.5, "right"), ("line", 88.5, 40, 99.5, 40),  # B
    ("line", 117, 18, 117, 30), ("half", 127, 30, 10, "bottom"), ("line", 137, 18, 137, 40),         # u
    ("ring", 157, 29, 11), ("line", 168, 6, 168, 40),                            # d
    ("ring", 188, 29, 11), ("line", 199, 6, 199, 40),                            # d
    ("line", 207, 18, 217, 40), ("line", 227, 18, 212.5, 50),                    # y
]


def n(v):
    return f"{v:.2f}".rstrip("0").rstrip(".")


def outline(shapes, weight):
    """Outline path for monoline letters. Every shape runs anticlockwise on screen; holes run clockwise."""
    r = weight / 2
    d = []
    for kind, *a in shapes:
        if kind == "line":  # a capsule: the stroke of one straight line with round caps
            x1, y1, x2, y2 = a
            length = ((x2 - x1) ** 2 + (y2 - y1) ** 2) ** 0.5
            nx, ny = -(y2 - y1) / length * r, (x2 - x1) / length * r
            d.append(f"M{n(x1 + nx)} {n(y1 + ny)}L{n(x2 + nx)} {n(y2 + ny)}A{n(r)} {n(r)} 0 0 0 {n(x2 - nx)} {n(y2 - ny)}"
                     f"L{n(x1 - nx)} {n(y1 - ny)}A{n(r)} {n(r)} 0 0 0 {n(x1 + nx)} {n(y1 + ny)}Z")
        elif kind == "ring":  # the stroke of a full circle
            cx, cy, rad = a
            o, i = rad + r, rad - r
            d.append(f"M{n(cx + o)} {n(cy)}A{n(o)} {n(o)} 0 1 0 {n(cx - o)} {n(cy)}A{n(o)} {n(o)} 0 1 0 {n(cx + o)} {n(cy)}Z"
                     f"M{n(cx + i)} {n(cy)}A{n(i)} {n(i)} 0 1 1 {n(cx - i)} {n(cy)}A{n(i)} {n(i)} 0 1 1 {n(cx + i)} {n(cy)}Z")
        else:  # the stroke of a half circle; the lines it joins cover its flat ends
            cx, cy, rad, side = a
            o, i = rad + r, rad - r
            if side == "right":  # from the top, round the right, to the bottom
                d.append(f"M{n(cx)} {n(cy + o)}A{n(o)} {n(o)} 0 0 0 {n(cx)} {n(cy - o)}L{n(cx)} {n(cy - i)}"
                         f"A{n(i)} {n(i)} 0 0 1 {n(cx)} {n(cy + i)}Z")
            else:  # bottom: from the right, under, to the left
                d.append(f"M{n(cx - o)} {n(cy)}A{n(o)} {n(o)} 0 0 0 {n(cx + o)} {n(cy)}L{n(cx + i)} {n(cy)}"
                         f"A{n(i)} {n(i)} 0 0 1 {n(cx - i)} {n(cy)}Z")
    return "".join(d)


WORD_TAX = outline(C_TAX, C_WEIGHT)
WORD_BUDDY = outline(C_BUDDY, C_WEIGHT)


def stroke(d, color, tx, ty, s, weight):
    return (f'<path transform="translate({tx} {ty}) scale({s})" d="{d}" fill="none" '
            f'stroke="{color}" stroke-width="{weight}" stroke-linecap="round" stroke-linejoin="round"/>')


def word(colors, tx, ty, s):
    return stroke(WORD, colors[0], tx, ty, s, 4.6)


def word_c(colors, tx, ty, s):
    return "".join(f'<path transform="translate({tx} {ty}) scale({s})" fill="{c}" d="{d}"/>'
                   for c, d in ((colors[0], WORD_TAX), (colors[1], WORD_BUDDY)))


def rr(x, y, w, h, r):
    """Rounded rectangle as a path."""
    return (f"M{x + r} {y}H{x + w - r}A{r} {r} 0 0 1 {x + w} {y + r}V{y + h - r}"
            f"A{r} {r} 0 0 1 {x + w - r} {y + h}H{x + r}A{r} {r} 0 0 1 {x} {y + h - r}"
            f"V{y + r}A{r} {r} 0 0 1 {x + r} {y}Z")


# ---- Symbols on a 48 grid. Each returns SVG markup for (main colour, accent colour).
def sym_a(c, accent):
    body = ("M13 5H28V16H39V33A4 4 0 0 1 35 37H22L13 45V37A4 4 0 0 1 9 33V9A4 4 0 0 1 13 5Z"
            "M16.5 11H22.5A1.5 1.5 0 0 1 22.5 14H16.5A1.5 1.5 0 0 1 16.5 11Z"
            "M16.5 20H31.5A1.5 1.5 0 0 1 31.5 23H16.5A1.5 1.5 0 0 1 16.5 20Z"
            "M16.5 27H31.5A1.5 1.5 0 0 1 31.5 30H16.5A1.5 1.5 0 0 1 16.5 27Z")
    flap = "M30 5L39 14H32A2 2 0 0 1 30 12Z"
    return (f'<path fill="{c}" fill-rule="evenodd" d="{body}"/>'
            f'<path fill="{accent}" d="{flap}"/>')


def sym_b(c, accent):
    left = "M10 11H15C19 11 22 13 22 17V37C20.5 35 18 34 16 34L9 40.5V34A3 3 0 0 1 6 31V15A4 4 0 0 1 10 11Z"
    right = "M38 11.5H33C29.2 11.5 26.5 13.5 26.5 17.2V36.3C28 34.5 30 33.5 32 33.5L38.5 39.8V33.5A3 3 0 0 0 41.5 30.5V15A3.5 3.5 0 0 0 38 11.5Z"
    return (f'<path fill="{c}" stroke="{c}" stroke-width="2" stroke-linejoin="round" d="{left}"/>'
            f'<g fill="none" stroke="{c}" stroke-linecap="round" stroke-linejoin="round">'
            f'<path stroke-width="3" d="{right}"/>'
            f'<path stroke-width="2.6" d="M31.5 19.5H36.5M31.5 25.5H36.5"/></g>')


# Direction C: a circle whose bottom-left corner stays square (the speech tail),
# holding one solid T: the T-account rule and the T of Tax Buddy.
C_DROP = "M24 4A20 20 0 0 1 24 44H6.5A2.5 2.5 0 0 1 4 41.5V24A20 20 0 0 1 24 4Z"
C_T = ("M14.5 14H33.5A1.5 1.5 0 0 1 35 15.5V18.5A1.5 1.5 0 0 1 33.5 20H27V33.5A1.5 1.5 0 0 1 25.5 35"
       "H22.5A1.5 1.5 0 0 1 21 33.5V20H14.5A1.5 1.5 0 0 1 13 18.5V15.5A1.5 1.5 0 0 1 14.5 14Z")


def sym_c(c, accent):
    return f'<path fill="{c}" fill-rule="evenodd" d="{C_DROP}{C_T}"/>'


# ---- Favicons on a 16 grid, simplified. A and B: forest tile, white glyph.
# C: the symbol itself fills the square.
FAV = {
    "a": (f'<rect width="16" height="16" rx="3.5" fill="{FOREST}"/>'
          '<path fill="#FFF" fill-rule="evenodd" d="M5 3H9.5L12 5.5V10A1 1 0 0 1 11 11H7.5L5 13.5V11A1 1 0 0 1 4 10V4A1 1 0 0 1 5 3Z'
          'M5.5 6H10.5V7H5.5ZM5.5 8H10.5V9H5.5Z"/>'),
    "b": (f'<rect width="16" height="16" rx="3.5" fill="{FOREST}"/>'
          '<path fill="#FFF" d="M4 4.5H6C7 4.5 7.5 5 7.5 6V12C7 11.4 6.5 11 6 11L4 13V11A1 1 0 0 1 3 10V5.5A1 1 0 0 1 4 4.5Z'
          'M12 4.5H10C9 4.5 8.5 5 8.5 6V12C9 11.4 9.5 11 10 11L12 13V11A1 1 0 0 0 13 10V5.5A1 1 0 0 0 12 4.5Z"/>'),
    "c": (f'<path fill="{FOREST}" d="M8 0A8 8 0 0 1 8 16H1A1 1 0 0 1 0 15V8A8 8 0 0 1 8 0Z"/>'
          '<path fill="#FFF" d="M4 4H12V6H9V12H7V6H4Z"/>'),
}


def icon_default(sym):
    return (f'<rect width="48" height="48" rx="11" fill="{FOREST}"/>'
            f'<g transform="translate(6 6) scale(.75)">{sym(PAPER, SAGE)}</g>')


def icon_c(sym):
    return (f'<rect width="48" height="48" rx="11" fill="{FOREST}"/>'
            f'<g transform="translate(4.8 4.8) scale(.8)">{sym(SAGE, SAGE)}</g>')


# key: (folder, symbol, description, wordmark, wordmark width, app icon)
DIRS = {
    "a": ("a-speaking-page", sym_a, "ledger page with a folded corner and a speech tail", word, 223, icon_default),
    "b": ("b-open-spread", sym_b, "two facing speech bubbles forming an open ledger", word, 223, icon_default),
    "c": ("c-t-account", sym_c, "speech bubble holding a T", word_c, 227, icon_c),
}


def svg(vb, title, inner):
    return (f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {vb[0]} {vb[1]}" role="img" '
            f'aria-labelledby="title"><title id="title">{title}</title>{inner}</svg>\n')


def write(folder, name, text):
    path = os.path.join(OUT, folder, name)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)


# (suffix, symbol colour, accent colour, wordmark colours: first word, second word)
# Only direction C uses the second wordmark colour.
FULL = [("", FOREST, SAGE, (INK, FOREST)), ("-reversed", SAGE, SAGE_TEXT, (SAGE_TEXT, SAGE))]
MONO = [("-forest", FOREST, FOREST, (FOREST, FOREST)), ("-black", "#000000", "#000000", ("#000000", "#000000")),
        ("-white", "#FFFFFF", "#FFFFFF", ("#FFFFFF", "#FFFFFF"))]

for key, (folder, sym, desc, wordmark, width, icon) in DIRS.items():
    hw = round(62 + width * 0.85 + 3)
    for suf, c, acc, w in FULL + MONO:
        write(folder, f"symbol{suf}.svg", svg((48, 48), f"Tax Buddy symbol: {desc}", sym(c, acc)))
        write(folder, f"logo-horizontal{suf}.svg", svg((hw, 56), "Tax Buddy",
              f'<g transform="translate(0 4)">{sym(c, acc)}</g>' + wordmark(w, 62, 9, 0.85)))
        write(folder, f"logo-stacked{suf}.svg", svg((200, 104), "Tax Buddy",
              f'<g transform="translate(76 2)">{sym(c, acc)}</g>'
              + wordmark(w, round((200 - width * 0.8) / 2, 1), 58, 0.8)))
    write(folder, "app-icon.svg", svg((48, 48), "Tax Buddy app icon", icon(sym)))
    write(folder, "favicon.svg", svg((16, 16), "Tax Buddy", FAV[key]))

# ---- Adopted identity: direction C, in the files the prototype and the site load.
# The lockup is 258 x 48 so it shows at 161 x 30 CSS px, the height the site bar was built for.
if len(sys.argv) > 2:
    def adopted(name, text):
        os.makedirs(sys.argv[2], exist_ok=True)
        with open(os.path.join(sys.argv[2], name), "w", encoding="utf-8", newline="\n") as f:
            f.write(text)

    for name, c, w in (("light", FOREST, (INK, FOREST)), ("dark", SAGE, (SAGE_TEXT, SAGE))):
        adopted(f"taxbuddy-logo-{name}.svg", svg((258, 48), "Tax Buddy",
                f'<g transform="translate(0 -2)">{sym_c(c, c)}</g>' + word_c(w, 62, 3, 0.85)))
    adopted("taxbuddy-icon.svg", svg((48, 48), "Tax Buddy symbol: speech bubble holding a T", sym_c(FOREST, FOREST)))
    adopted("taxbuddy-icon-dark.svg", svg((48, 48), "Tax Buddy symbol: speech bubble holding a T", sym_c(SAGE, SAGE)))
    adopted("favicon.svg", svg((16, 16), "Tax Buddy", FAV["c"]))
print("ok")
