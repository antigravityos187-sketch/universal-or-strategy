# PTT Trade Copier — Website

Static product website for PTT Trade Copier (NinjaTrader 8 Add-On).

## Pages

| File | URL | Description |
|------|-----|-------------|
| `index.html` | `/` | Landing page — hero, features, pricing preview |
| `pricing.html` | `/pricing` | Tier cards with LemonSqueezy checkout buttons |
| `install.html` | `/install` | Step-by-step NT8 import guide |
| `changelog.html` | `/changelog` | B-block history as public release notes |
| `support.html` | `/support` | FAQ + Formspree contact form |
| `terms.html` | `/terms` | Terms of Service (required by LemonSqueezy) |
| `refunds.html` | `/refunds` | Refund Policy (required by LemonSqueezy) |

## Before Going Live

1. **Replace LemonSqueezy checkout URLs** in `pricing.html`:
   - Search for `LIFETIME_VARIANT_ID` and `MONTHLY_VARIANT_ID` — replace with your real product variant IDs from the LemonSqueezy dashboard.
   - The WSGTA link is `?discount_code=wsgta` appended to the lifetime URL — that's already wired.

2. **Replace Formspree form ID** in `support.html`:
   - Search for `YOUR_FORM_ID` — replace with your free Formspree form ID from formspree.io.

3. **Update email address** — currently `support@proptradertools.com`. Update in all footers if different.

4. **Update copyright year** in all footers.

5. **Custom domain** — add `CNAME` file with your domain for GitHub Pages, or connect in Vercel dashboard.

## Deployment

### GitHub Pages
```bash
git add website/
git commit -m "chore: add product website"
git push origin main
# Then enable Pages in repo Settings → Pages → Source: main, /website folder
```

### Vercel Drop-Deploy
Drag the `website/` folder to vercel.com/new — zero config needed.

## Design System

All pages use the spec CSS variables from `specs/002-trade-copier-spec.html`:

```css
--bg:      #05070d
--surface: #0b0e18
--raised:  #111520
--border:  #1c2133
--accent:  #2563eb   /* primary blue */
--green:   #22c55e
--amber:   #f59e0b
--purple:  #a855f7
--dim:     #4b5563
--mid:     #9ca3af
--bright:  #f1f5f9
--mono:    "IBM Plex Mono"
--sans:    "IBM Plex Sans"
```

Fonts load from Google Fonts CDN — no build step required.
All CSS is inlined per-page — no bundler, no build step.
