# RDCore™

[EN](./README.en.md) | [FR]

### Avant de commencer.

> 👋 Nouveau ici? Rubberduck a toujours été une initiative open-source.
> **RDCore l'honore avec une formule Open-Core**. Voir [rubberduckvba.ca](https://rubberduckvba.ca) pour plus de détails.

Ce référentiel contient différents projets **en phase de développement actif** produisant différentes librairies et exécutables sous un modèle de licence relativement simple :

- **La librairie RDCore.SDK** est sous licence **⚖️MIT**;
- **Tout le reste** est construit autour et sous licence **⚖️GPLv3**.

Cet arrangement protège tant les contributeurs historiques qu'actuels, tout en protégeant son avenir : **l'implémentation du _runtime_ de RDCore demeurera open-source**.

> 👉 Nous construisons ici une solide fondation pour le _coeur de langage_, mais veuillez noter qu'en ce moment le seul livrable est le [site de documentation](https://rubberduck-vba.github.io/rdcore).

---

## RDCore

**RDCore**™ est une plateforme de _serveur de langage_ (LSP) dont les travaux d'implémentation sont **présentement en cours**. À la cible, les livrables de RDCore sont :

- 🎯 **rdc.exe**: un _environnement hôte_ RD-VBA configurable et extensible, client LSP (CLI);
- 🎯 **RDCore.LanguageServer.exe**: le serveur d'orchestration LSP de la plateforme;
- 🎯 **RDCore.Parser.exe**: le _parser_ de la plateforme est une application serveur LSP satellite détenue et orchestrée par le serveur de langage principal;
- 🎯 **RDCore.Diagnostics.exe**: une extension _core_ de la plateforme qui envoie les _diagnostics_ au serveur de langage principal de façon asynchrone;
- 👉 **RDCore.Runtime.dll**: une librairie renfermant l'implémentation de toute la sémantique et mécanismes du run-time de RD-VBA, _incluant une implémentation de la librairie VBA standard_;
- 🧩 **RDCore.SDK.dll**: une librairie exposant les abstractions de la plateforme RDCore et encapsulant les implémentations de base du _coeur de langage_ RD-VBA.


### ✨Ce que RDCore rend envisageable
- Analyse sémantique profonde de code VBA
- Exécution de code VBA hors du VBIDE
- Outillage langage via le protocole _Language Server_ (LSP)
- Inspection du comportement à l'exécution, faits sémantiques 
- Extension de la plateforme avec des analyseurs et plug-ins


### 📊Statut du projet
RDCore est présentement en phase active de développement - le seul livrable pour l'instant consiste en sa spécification et sa documentation.
- Architecture: ✅ stable
- SDK langage: ✅ largement défini
- Runtime: 🚧 implémentation en cours
- Librarie standard: 🚧 partiellement définie
- Parser: 🚧 existe (tout juste)
- Hôte CLI (rdc.exe): 🚧 existe (tout juste)
- Contributions publiques: ❌ pas encore ouvertes


## RD-VBA

L'implémentation du _coeur de langage_ de la plateforme est également un **projet en cours de réalisation**. Ultimement, RD-VBA :

- 🎯 **vise une stricte adhésion aux spécifications MS-VBAL**, assurant une compatibilité comportementale avec les sémantiques spécifiées existantes de VBA;
- 🧩 **élève VBA en une plate-forme de langage moderne, extensible, et _entièrement open-source_**, séparant la _définition du langage_ de son _implémentation originale_ de 1993;
- 👀 **rend explicite les comportements implicites du langage** en exposant les règles sémantiques, étapes d'évaluation, piles d'appels, et états d'erreur en tant que _faits observables_.


---
 V I V A T 🩷 C U C U M I S ™  
 [Accueil](https://rubberduck-vba.github.io/rdcore) | ℹ️[Introduction](https://rubberduck-vba.github.io/rdcore/introduction.md) | 🧩[Démarrage](https://rubberduck-vba.github.io/rdcore/getting-started.md) | 🎯[RD-VBAL](https://rubberduck-vba.github.io/rdcore/specs/rd-vbal.md) | [SDK](/api) | 🌐[rubberduckvba.ca](https://rubberduckvba.ca)

---

<p align="center">
<img alt="Logo™ 9562-7303 Québec inc." src="./assets/vector-ducky.svg" style="width:200px; margin-top:72px;" /><br/>
<small>© Copyright <strong>9562-7303 Québec inc.</strong> (2026)<br/>
<em>"Rubberduck" est utilisé pour fins de référence au projet open-source legacy <strong>utilisé publiquement ainsi depuis 2015</strong> et sans lien ni affiliation avec tout tiers détenteur d'une marque semblable dans quelque juridiction que ce soit. "RDCore" et "VIVAT CUCUMIS" sont des marques de commerce revendiquées par 9562-7303 Québec inc. (en attente)
</small>
</p>
