# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Fixed

- Replaced removed gregCore `GregMenuBinding` API with `GregMenuRegistry` opener/closer (+ open-state reporting) — builds against current gregCore again.


### Added

- Potato mode (`F3`): lowest texture quality (mip limit 3, streaming off, aniso off), AA/shadows/pixel-lights/LOD/soft-particles/probes/skin-weights levers, VSync off + 60 FPS cap.
- HDRP kills mirroring gregCore's verified paths: SSAO, contact shadows, GI, SSR, volumetric fog, decals off, 16 max shadow requests.
- Scene levers: post-process volumes off, opt-in full particle kill.
- Snapshot/restore of every touched setting (toggle-off, re-apply on scene load, restore on unload).
- GregCore wiring behind soft probe: F1 menu + key HUD + toggle binding + toasts; standalone fallback.
- Docs: `docs/POTATO_SPEC.md`, `docs/GREGCORE_GAPS.md`.
