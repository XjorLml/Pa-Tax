@AGENTS.md

# Claude Code instructions

Owner instructions of 2026-11-16. They add to the [agent workflow](AGENTS.md) above.

## Review the documentation before implementing

Before writing or changing any code, read the documentation that governs the work, and only then start:

1. [docs/README.md](docs/README.md): the index, feature table and open conflicts.
2. The [PRD](docs/PRD.md) rows, business rules and open decisions for the requirement IDs involved.
3. The [SDD](docs/SDD.md) sections for the pages, data and rules involved: render modes, navigation, sign-in and authorization.
4. [DESIGN](docs/DESIGN.md) for the screens and states involved.
5. The feature's `specs/<feature>/spec.md`, `plan.md` and `tasks.md`, or the [template](specs/feature-template/spec.md) for a new feature.
6. [STATUS](docs/delivery/STATUS.md) for the gates, open decisions and current state.
7. The matching screens in the prototype (next section).

If the documents disagree with each other, with the prototype or with the code, record it and ask; don't silently pick one.

## The prototype is the origin of the design

[prototypes/index.html](prototypes/index.html) with [taxbuddy.css](prototypes/taxbuddy.css) (the Tax Buddy look, Paper & Forest palette) is the source of every page's design: layout, components, wording, states and order of fields.

- Before building or changing a page, open its prototype screen. Its render function is in `index.html`, for example `signinHTML`, `meHTML`, `editorParts`, `desktopShell` or `phoneShell`. Follow its markup structure, class names, copy and states, in both its phone and desktop frames.
- Port its styles into `src/TaxBuddy.Web/Styles/app.css` through the semantic tokens; don't invent new looks.
- When the prototype can't be followed, keep the recorded decision and write the deviation into DESIGN and the feature spec. Reasons it might not be followed: a page is static and works without script (SDD, C04); a linked feature isn't built yet; or the owner decided otherwise.
- The prototype does not authorize scope; the PRD does.
