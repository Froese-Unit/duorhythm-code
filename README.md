# DuoRhythm — code

Companion code for the **DuoRhythm** EEG hyperscanning dataset (cooperative joint action in a
video-game environment), collected at the Embodied Cognitive Science Unit (ECSU), OIST.

This repository holds the **code** that supports the dataset's *Scientific Data* Data Descriptor
(BIDS conversion, preprocessing, and technical validation). The **data itself** lives on OpenNeuro
(BIDS, with a DOI) — not here.

> ⚠️ **Code only — no data, no personal information.** Never commit raw recordings, participant
> identifiers, questionnaire responses, `.xdf`/`.eeg`/`.vhdr` files, or anything traceable to a
> participant. The dataset is released through OpenNeuro under the project's ethics approval
> (OIST HSR-2024-019); this repo must stay firewall-clean.

## Layout

| Folder | Contents |
|---|---|
| `bids-conversion/` | Scripts converting the raw `.xdf` recordings → BIDS (`.eeg`/`.vhdr`/`.vmrk`), `participants.tsv`, phenotype `.tsv`/`.json`. |
| `preprocessing/` | A reusable EEG preprocessing pipeline (e.g. MNE / HyPyP) — filtering, artifact handling, montage. |
| `technical-validation/` | The temporal-synchronization validation (Arduino/TTL pulse test, ~47 ms RDA offset) and the EEG data-quality / PSD checks reported in the descriptor. |
| `task/` | The Unity video-game task (or a pointer to its release). |

## Populating this repo

- **Eric (Chen Lam Loh):** the `.xdf`→BIDS conversion, the Unity task, and the temporal-synchronization
  validation code (the TTL/Arduino test and the RDA-latency analysis).
- **Saisha:** the preprocessing pipeline and the PSD / data-quality scripts used for technical validation.

Please add a short header comment to each script (what it does, inputs/outputs) and keep paths relative
so others can reproduce without editing hard-coded locations.

## Citation & archival

On acceptance, this repository will be archived on **Zenodo** to mint a DOI, which the descriptor's
**Code Availability** statement will cite. The dataset is cited separately via its OpenNeuro DOI.

## License

To be set by the PI before public release (a permissive license such as **MIT** is standard for
research code; the dataset carries its own OpenNeuro terms).
