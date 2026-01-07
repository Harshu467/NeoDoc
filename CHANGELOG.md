# Changelog

All notable changes to this project will be documented in this file.

## Unreleased

### Added
- feat(docx): Attach inline images directly to `DocRun` so images are associated with the exact run position instead of only paragraph-level fallbacks. Adds unit tests verifying run-to-image mapping and updates the streaming reader and HTML renderer.

### Fixed
- fix(docx): Improve image extraction robustness by scanning paragraph XML for `embed` and `r:id` relIds (covers VML/legacy image shapes).

### Tests
- Added `DocxParser_AttachesImageToCorrectRun` to assert correct run-level image association.
