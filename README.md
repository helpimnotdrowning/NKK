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

Backend: Blazor

Frontend: HTMX, Tailwind, prism.js (syntax highlighting)

## Licensing
preface: IANAL, this could all be completely wrong, etc. etc. but you can tell
what I mean...

Site code and modifications to vendored library code (except the IBM Plex font;
see below) is distrbuted under the AGPLv3 license ([LICENSE.md](LICENSE.md))

External licenses listed at [LICENSE.external.md](LICENSE.external.md):
* prism.js: MIT
* htmx: BSD-0
* IBM Plex font family: OFL 1.1
* Ancient One Dark style: MIT

Blog posts and artwork (excluding those featuring external IPs) are separately
licensed under the
[CC BY-NC-SA 4](https://creativecommons.org/licenses/by-nc-sa/4.0/).
Essentially: for non-commercial purposes, and as long as you give credit
(link my website or my Twitter, mention the CC BY-NC-SA 4), you can
remix, transform, or play with and redistribute any* of the creative works under
the `Content/` directory.

The CC license text is also available at [Content/LICENSE.md](Content/LICENSE.md).

Works may occasionally/frequently feature externally-owned IPs (this will
usually be artwork, but not necessarily the accompanying writing). In these
cases, the work copyright belongs to the IP owner and their use is guided by
their owner and their wishes, acceptable use policy, and derivative works
policy.

These terms are inspired in whole by Jamie Paige's
[generous music reuse policy](https://jamies.page/stems)! check out her music :D
