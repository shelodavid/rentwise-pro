# UI Guide

## Overview
RentWise Pro uses a lightweight app-shell layout built on Razor views and Bootstrap 5. The goal is a clean finance-dashboard experience with consistent spacing, typography, and card layouts inspired by modern real-estate analytics tools.

## Layout Components

### App Shell
- **File:** `Views/Shared/_Layout.cshtml`
- **Purpose:** Provides the left navigation, top bar, and content region.
- **Behavior:**
  - Sidebar collapses on mobile and can be toggled via the top bar menu button.
  - Active navigation items highlight automatically based on the current controller/action.

### Page Header (`_PageHeader`)
- **File:** `Views/Shared/_PageHeader.cshtml`
- **Usage:** Set `ViewData["PageTitle"]` and optional `ViewData["PageSubtitle"]` and `ViewData["PageMeta"]` before calling the partial.
- **Example:**
  ```csharp
  @{ 
      ViewData["PageTitle"] = "ROI Forecast";
      ViewData["PageSubtitle"] = "Use the forecast to validate returns.";
      ViewData["PageMeta"] = "ZPID 12345";
  }
  @await Html.PartialAsync("_PageHeader")
  ```

### KPI Ribbon (`_KpiRibbon`)
- **File:** `Views/Shared/_KpiRibbon.cshtml`
- **Model:** `ForecastKpis`
- **Behavior:** Sticky on desktop, stacked on mobile. Displays monthly cash flow, cash-on-cash %, cap rate, and DSCR.

### Stat Card (`_StatCard`)
- **File:** `Views/Shared/_StatCard.cshtml`
- **Usage:** Provide `ViewData` keys `StatLabel`, `StatValue`, and optional `StatHelper` to render a compact KPI card.

### Step Indicator (`_StepIndicator`)
- **File:** `Views/Shared/_StepIndicator.cshtml`
- **Usage:** Pass a list of step names and set `ViewData["ActiveStep"]` to the current index. Designed for wizard-style flows such as the Purchase Sheet.

## Styling
- **File:** `wwwroot/css/site.css`
- **Design tokens:** CSS variables define colors, spacing, border radius, and shadows.
- **Components:** Cards, inputs, buttons, KPI ribbon, and stepper styles are defined here for reuse.

## UI Interactions
- **File:** `wwwroot/js/site.js`
- **Features:**
  - Sidebar toggle on mobile.
  - Stepper navigation for wizard flows.
  - Simple collapse/expand panels.

## Page Patterns
- **Home / Index:** Card grid of saved properties with quick actions.
- **Purchase Sheet:** Wizard-like sections with stepper and review panel.
- **ROI Forecast:** Inputs on the left, charts and summary on the right using Chart.js.
