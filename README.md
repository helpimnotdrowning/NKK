Personal homepage at [www.helpimnotdrowning.net](https://www.helpimnotdrowning.net/)

Exposes port 8081

Build container with
```pwsh
sudo docker build -t helpimnotdrowning/nkk .
```

Run with the following compose definition:
```yml
services:
    # ...
    helpimnotdrowning_net:
        image: helpimnotdrowning/nkk
        container_name: helpimnotdrowning_net
        restart: unless-stopped
        ports:
            - '8081:8081'
```

Dependencies are purposefully distributed unminified in support of the
[#ViewSource affordance](https://htmx.org/essays/right-click-view-source/)
(in-transit compression is great anyways!)

Fonts at `public/font` generated with my `auto-font-splitter` script
([Forgejo](https://git.helpimnotdrowning.net/helpimnotdrowning/auto-font-splitter/),
[GitHub](https://github.com/helpimnotdrowning/auto-font-splitter/)), using my
fork of `font-splitter`
([Forgejo](https://git.helpimnotdrowning.net/helpimnotdrowning/font-splitter/),
[GitHub](https://github.com/helpimnotdrowning/font-splitter))

Backend: PowerShell, [Pode](https://github.com/Badgerati/Pode), Mizumiya
([Forgejo](https://git.helpimnotdrowning.net/helpimnotdrowning/Mizumiya),
[GitHub](https://github.com/helpimnotdrowning/Mizumiya))

Frontend: HTMX, Tailwind, prism.js (syntax highlighting)

## Licensing
Site code and modifications to vendored library code (except the IBM Plex font;
see below) is distrbuted under the AGPLv3 license ([LICENSE.md](LICENSE.md))

External licenses listed at [LICENSE.external.md](LICENSE.external.md):

The [prism.js](https://prismjs.com/) library
([upstream](https://github.com/PrismJS/prism), local [library](public/prism.js),
[syntax files](public/prism/grammars/)) is vendored under the MIT license.

The [htmx](https://htmx.org/) library
([upstream](https://github.com/bigskysoftware/htmx), local
[library](public/htmx.js)) is vendored under the BSD-0 license.

Members of the [IBM Plex font](https://www.ibm.com/plex/)
([upstream](https://github.com/IBM/plex), local [font](public/font/)) family
(Mono and Sans JP) are modified (split) and distributed under the OFL 1.1.

The code theme used in the prism.js style is based off the
[Ancient One Dark](https://github.com/sigvt/ancient-one-dark) (local
[theme](tailwind/01.prismjs.tw.css)) "Violet" theme, which is licenced under the
MIT license.
