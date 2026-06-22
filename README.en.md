# RDCore™

[EN] | [FR](./README.md)

## Before we begin.

> 👋 New here? Rubberduck was always an open-source initiative. **RDCore honors it with an Open-Core formula**.  
> <small>See [rubberduckvba.ca](https://rubberduckvba.ca) for more information.</small>

This repository contains different projects **under active development** producing different libraries and executables, under a relatively simple licensing model:

- **The RDCore.SDK library** is licensed under **⚖️MIT**;
- **Everything else** built around it is licensed under **⚖️GPLv3**.

This arrangement protects both the legacy and current contributors while enabling the future: **The RDCore runtime implementation shall remain open-source**.

> 👉 We're building a solid _language core_ foundation here, but please note that at the moment the only deliverable is the [documentation site](https://rdcore-sdk.github.io).

---

# 1.0.1 RDCore

**RDCore**™ is an actively evolving _Language Server_ (LSP) platform that is currently a **work in progress**. Ultimately, the RDCore deliverables are:

- 🎯 **rdc.exe**: a configurable and extensible RD-VBA _environment host_ and LSP client CLI application;
- 🎯 **RDCore.LanguageServer.exe**: the platform's "orchestrator" LSP server application;
- 🎯 **RDCore.Parser.exe**: the platform's parser is a satellite LSP server application owned and coordinated by the main language server;
- 🎯 **RDCore.Diagnostics.exe**: a core platform extension asynchronously issuing _diagnostics_ to the main language server;
- 👉 **RDCore.Runtime.dll**: a library containing an implementation for all the RD-VBA runtime semantics and mechanics, _including an implementation of the VBA Standard Library_;
- 🧩 **RDCore.SDK.dll**: a library exposing the RDCore abstractions and encapsulating the base RD-VBA _language core_ implementation.

# 1.0.2 RD-VBA

The implementation of the platform's _language core_ is a **work in progress**. Ultimately, RD-VBA:

- 🎯 **aims for strict compliance with the MS-VBAL specifications**, ensuring behavioral compatibility with existing VBA semantics;
- 🧩 **elevates VBA into a modern, extensible, _and fully open-sourced_ language platform** separating the language definition from its original 1993 implementation;
- 👀 **makes implicit language behavior explicit**, exposing semantic rules, evaluation steps, call stacks, and error conditions as _observable facts_.


---
 V I V A T 🩷 C U C U M I S ™  
 [Home](https://rubberduck-vba.github.io/rdcore/index.en.md)  ℹ️[Introduction](https://rubberduck-vba.github.io/rdcore/introduction.en.md) | 🧩[Getting Started](https://rubberduck-vba.github.io/rdcore/getting-started.en.md) | 🎯[RD-VBAL](https://rubberduck-vba.github.io/rdcore/specs/rd-vbal.md) | [SDK](/api) | 🌐[rubberduckvba.ca](https://rubberduckvba.ca)

---

<p align="center">
<img alt="Logo™ 9562-7303 Québec inc." src="./assets/vector-ducky.svg" style="width:200px; margin-top:72px;" /><br/>
<small>© Copyright <strong>9562-7303 Québec inc.</strong> (2026)<br/>
<em>"Rubberduck" is used as a reference to the legacy open-source project <strong>the same way it has been used publicly since 2015</strong> and without any links or affiliation with any third-party trademark holders of a similar trademark in any jurdisdiction. "RDCore" and "VIVAT CUCUMIS" are trademarks claimed by 9562-7303 Québec inc. (pending)
</small>
</p>
