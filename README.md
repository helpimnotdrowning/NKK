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
* prism.js: MIT
* htmx: BSD-0
* IBM Plex font family: OFL 1.1
* Ancient One Dark style: MIT

Blog posts and artwork are separately licensed under the
[CC BY-NC-SA 4](https://creativecommons.org/licenses/by-nc-sa/4.0/).
(essentially,) For non-commercial purposes, and as long as  you give credit (link my website or my
Twitter, mention the CC BY-NC-SA 4), you can remix/transform any of the creative
works under the `museum/` and `sayings/` directories. Characters/IPs that are not my own may be
occasionally featured; honestly I'm not sure how the licensing works in that
case but I ASSUME that you follow the combination of my CC terns and whatever
derivative use policy the IP has (ie the COVER Corp.
[Derivative Works Guidelines](https://hololivepro.com/en/terms/)).

The license text mentioned above is also available at
[sayings/LICENSE.md](sayings/LICENSE.md) and
[museum/LICENSE.md](museum/LICENSE.md)

These terms are inspired in whole by Jamie Paige's
[generous music reuse policy](https://jamies.page/stems)! check out her music :D

External licenses listed at [LICENSE.external.md](LICENSE.external.md):
* prism.js: MIT
* htmx: BSD-0[LICENSE.md](museum/LICENSE.md)
* IBM Plex font family: OFL 1.1
* Ancient One Dark style: MIT
