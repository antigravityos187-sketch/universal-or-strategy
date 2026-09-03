---
name: Apex Velocity
source: Stitch by Google — project 8763211050693256698
url: https://stitch.withgoogle.com/projects/8763211050693256698
generated_from: ninjatrader.com
captured: 2026-08-29
---

## Design System Export

colors:
  surface: '#121414'
  surface-dim: '#121414'
  surface-bright: '#37393a'
  surface-container-lowest: '#0c0f0f'
  surface-container-low: '#1a1c1c'
  surface-container: '#1e2020'
  surface-container-high: '#282a2b'
  surface-container-highest: '#333535'
  on-surface: '#e2e2e2'
  on-surface-variant: '#e7bdb2'
  inverse-surface: '#e2e2e2'
  inverse-on-surface: '#2f3131'
  outline: '#ae887f'
  outline-variant: '#5d4038'
  surface-tint: '#ffb5a1'
  primary: '#ffb5a1'
  on-primary: '#611300'
  primary-container: '#ff5628'
  on-primary-container: '#551000'
  inverse-primary: '#b22b00'
  secondary: '#a6c8ff'
  on-secondary: '#00315f'
  secondary-container: '#2992ff'
  on-secondary-container: '#002a53'
  tertiary: '#c8c6c5'
  on-tertiary: '#313030'
  tertiary-container: '#929090'
  on-tertiary-container: '#2a2a2a'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#ffdbd1'
  primary-fixed-dim: '#ffb5a1'
  on-primary-fixed: '#3c0800'
  on-primary-fixed-variant: '#881f00'
  secondary-fixed: '#d5e3ff'
  secondary-fixed-dim: '#a6c8ff'
  on-secondary-fixed: '#001c3b'
  on-secondary-fixed-variant: '#004786'
  tertiary-fixed: '#e5e2e1'
  tertiary-fixed-dim: '#c8c6c5'
  on-tertiary-fixed: '#1c1b1b'
  on-tertiary-fixed-variant: '#474746'
  background: '#121414'
  on-background: '#e2e2e2'
  surface-variant: '#333535'
  background-deep: '#000000'
  surface-elevated: '#121212'
  success-green: '#00C805'
  risk-red: '#FF3B30'
  muted-text: '#8E8E93'

typography:
  display-xl:
    fontFamily: Montserrat
    fontSize: 64px
    fontWeight: '800'
    lineHeight: 72px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Montserrat
    fontSize: 40px
    fontWeight: '700'
    lineHeight: 48px
  headline-lg-mobile:
    fontFamily: Montserrat
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
  headline-md:
    fontFamily: Montserrat
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  body-lg:
    fontFamily: Source Sans 3
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Source Sans 3
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  label-sm:
    fontFamily: IBM Plex Sans
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
    letterSpacing: 0.05em
  data-mono:
    fontFamily: IBM Plex Sans
    fontSize: 14px
    fontWeight: '600'
    lineHeight: 20px

rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px

spacing:
  stack-xs: 4px
  stack-sm: 8px
  stack-md: 16px
  stack-lg: 32px
  section-xl: 80px
  gutter: 24px
  margin-mobile: 16px
  margin-desktop: 64px

---

## Brand & Style

This design system is engineered for high-performance trading, balancing institutional-grade
reliability with the aggressive speed of modern futures markets. The brand personality is
authoritative, precise, and sophisticated, targeting serious traders who demand clarity
under pressure.

The visual style is Corporate / Modern with a lean toward High-Contrast Minimalism. It
leverages deep charcoal surfaces to reduce eye strain during long trading sessions,
punctuated by vibrant functional accents that guide the user's eye toward critical actions
and market shifts. The aesthetic prioritizes information density and data legibility over
decorative elements.

## Colors

The palette is optimized for a Dark Mode first experience to highlight data visualization.

- Primary (Apex Orange): Used exclusively for primary calls to action (CTAs) and
  high-priority alerts. It signifies energy and movement.
- Secondary (Velocity Blue): Reserved for interactive links, informational accents,
  and secondary platform features.
- Surface Palette: A range of deep blacks and charcoals create a hierarchical depth.
  Use #000000 for the primary background and #121212 for cards and navigation bars.
- Functional Colors: Standardized green and red are used strictly for market data
  (bullish/bearish) and risk status.

## Typography

The typography system is built for rapid scanning of financial data.

- Headlines: Montserrat provides a bold, geometric presence for marketing claims
  and section headers.
- Body Copy: Source Sans 3 is used for descriptive text and educational content
  due to its high legibility and neutral character.
- Data & Labels: IBM Plex Sans is utilized for technical labels and numerical data.
  Its structured, systematic feel mimics the precision of a trading terminal.
- Scaling: Use headline-lg-mobile for all screen widths under 768px to ensure the
  hero messaging remains impactful without overwhelming the viewport.

## Layout & Spacing

Fixed Grid model for marketing pages (max-width 1280px).
Fluid Grid for trading interface to maximize screen real estate.

- Grid: 12-column layout with 24px gutters.
- Breakpoints:
  - Mobile: < 768px (4 columns, 16px margins)
  - Tablet: 768px - 1024px (8 columns, 32px margins)
  - Desktop: > 1024px (12 columns, 64px margins)
- Rhythm: Vertical spacing follows a strict 8px base unit. Use section-xl to separate
  major value propositions.

## Elevation & Depth

Depth is conveyed through Tonal Layering rather than traditional shadows.

- Level 0 (Base): #000000 for the page background.
- Level 1 (Surfaces): #121212 for primary cards, feature blocks, and navigation containers.
- Level 2 (Interactive): Hovered/active elements use 1px outline (10% white).
- Glassmorphism: Use sparingly for navigation overlays. 12px backdrop blur with 80% opacity
  on charcoal fill.

## Shapes

Soft (0.25rem) shape language.

- Buttons: 4px radius
- Cards: 8px radius
- Inputs: 4px radius

## Components

### Buttons
- Primary: Apex Orange (#FF4200) background, white text, bold Montserrat. No shadow.
- Secondary: Ghost style. Transparent background with 1px white border.
- Tertiary: Text-only with trailing arrow_forward icon.

### Cards & Feature Blocks
- Level 1 surface (#121212). Padding: 32px desktop / 20px mobile.
- Hover: 1px border stroke Velocity Blue (#008CFF) or subtle lighten.

### Inputs & Selection
- Dark charcoal background (#1A1A1A) with 1px border (#333333).
- Focused: border changes to Velocity Blue.
- Chips: Low-profile charcoal background. Active: Apex Orange underline.

### Data Displays
- Financial figures use data-mono type style for decimal alignment.
- Up/Down indicators use success-green and risk-red.

### Imagery
- High-fidelity, angled monitor mockups showcasing complex charts.
- Subtle gradients (Black to Transparent) over images for text legibility.
