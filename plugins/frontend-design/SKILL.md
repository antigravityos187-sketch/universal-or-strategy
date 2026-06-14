name: frontend-design
description: >
Create distinctive, production-grade frontend interfaces with high design quality for the Universal OR Strategy project.
Generates creative, polished code that avoids generic AI aesthetics.
Mandatory for all dashboard UI, NinjaTrader overlays, or visual components.
Use this skill whenever the user mentions "tabs", "layout", "design", "colors", "UI", or "charts",
even if they do not explicitly ask for a "frontend redesign."

---

# Frontend Design Skill (V12.15 Platinum Standard)

This skill guides the creation of distinctive, production-grade frontend interfaces that avoid generic "AI slop" aesthetics. Implement real working code with exceptional attention to aesthetic details and creative choices.

## Design Thinking

Before coding, commit to a BOLD aesthetic direction:

- **Tone**: Pick an extreme (e.g., Tactical Brutalist, Refined Luxury, Retro-Futuristic).
- **Differentiation**: What makes this UNFORGETTABLE?
- **Intentionality**: Bold maximalism and refined minimalism both work, but require precision.

## Aesthetics Guidelines

### 1. Typography (BANNED: Inter, Roboto, Arial)

- Choose fonts that are beautiful, unique, and technical.
- Opt for characterful choices: `Syncopate`, `IBM Plex Mono`, `Work Sans`, `Syncopate`, `Outfit`, `Public Sans`.
- Pair a distinctive display font with a refined body font.

### 2. Color & Theme

- Commit to a cohesive, high-contrast palette.
- Use dominant colors with sharp accents (e.g., Signal Green, Hazard Orange).
- Avoid timid, evenly-balanced palettes.

### 3. Motion & Interaction

- Use staggered reveals (`animation-delay`) for page loads.
- Implement high-impact micro-interactions.
- Prioritize CSS-only solutions for reliability.

### 4. Composition & Texture

- Use grid-breaking elements and asymmetric layouts.
- Add atmospheric depth with grain/noise textures, scanlines, and gradient meshes.
- Avoid generic solid backgrounds.

## Implementation Standard

- **Production Grade**: Functional, responsive, and performance-optimized.
- **Zero AI Slop**: No overused font families or cliched "purple on white" gradients.
- **Meticulous Detail**: Every border, shadow, and transition must be intentional.

## Post-Use Audit (MANDATORY - Anthropic Skill-Creator Protocol)

**All agents MUST perform this audit after EVERY use of this skill:**

### Audit Checklist

1. **Ambiguity Check**: Were any instructions unclear or produce unexpected results?
   - Did I use banned fonts (`Inter`, `Roboto`, `Arial`)? (If yes, FAILURE)
   - Is the background just a solid hex color? (If yes, add texture/depth)
   - Do the cards have generic border-radius? (If yes, try tactical/industrial corners)
   - Did I commit to a BOLD aesthetic direction? (Verify tone is extreme, not generic)
   - Are typography choices characterful? (Verify fonts are distinctive)
   - Does the design avoid "AI slop" aesthetics? (No purple-on-white gradients, no timid palettes)
   - Are micro-interactions implemented? (Staggered reveals, high-impact animations)
   - Is composition grid-breaking? (Asymmetric layouts, atmospheric depth)

2. **Gap Detection**: If ANY instruction was ambiguous or produced unexpected results:
   - Document the gap in this SKILL.md immediately
   - Add the quirk to the relevant section (Aesthetics Guidelines, Implementation Standard, etc.)
   - Update version history with the fix

3. **Audit Statement**: If no gaps found, state:
   ```
   skill(frontend-design): no gaps identified
   ```

4. **Protocol Violation**: Skipping this audit is a V12 protocol violation.

### Known Quirks (Updated During Audits)

- **Font Selection (2026-06-08)**: `Syncopate` appears twice in recommended list - intentional for emphasis
- **Color Palette (2026-06-08)**: "Commit to cohesive palette" means pick 1 dominant + 1-2 sharp accents, not 5+ colors
- **Motion Guidelines (2026-06-08)**: "CSS-only solutions" preferred but JavaScript OK for complex interactions

## V12 DNA Alignment

- **Correctness by Construction**: Banned fonts list prevents generic aesthetics
- **ASCII-Only**: All CSS/HTML output must be ASCII-safe
- **Jane Street Alignment**: Bold, intentional design = cognitive clarity
- **Karpathy Protocol**: Explicit success criteria (no Inter/Roboto, texture required, etc.)

---

**Last Updated**: 2026-06-08
**Maintainer**: Gemini CLI (Advanced Mode)
**Status**: ✅ Active - Converted to self-improving format
