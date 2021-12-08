# Changelog
All notable changes to this project will be documented in this file.  

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

## [1.3.0] - 2021-12-08
### Changed
- Conditional attributes can be applied to Methods and Properties
- Conditional Group

## [1.2.1] - 2021-12-06
### Fixed
- Field assign callback throws null when no callback are assigned

### Changed
- Changed how ReadonlyFieldAttribute works

## [1.2.0] - 2021-12-02
### Fixed
- Inheritance reflection cache

### Added
- OverseerIncludeAttribute, FieldAssignCallbackAttribute

## [1.1.0] - 2021-12-01
### Changed
- Reflection-able properties have their own cache section. Thus reduce initialization time of Inspector.
- Rework on MethodButton interface
- FoldoutGroup now has animated foldout arrow as a little decoration
- ReadOnly Field attribute

## [1.0.1] - 2021-11-29
### Changed
- Remove nesting level number in Foldout Group

## [1.0.0] - 2021-11-29
### Changed
- Overseer Inspector is now stable enough to use. (more update about performance will be developed)

## [0.0.8] - 2021-11-29
### Removed
- Leftover debug message(s).

## [0.0.7] - 2021-11-29
### Work in progress
- Code emitting for faster performance.

### Changed
- Conditional attribute valuation moved to Editors, not in Runtime anymore, and replace with \[ConditionalConnect\] attribute

## [0.0.6] - 2021-11-28
### Added
- Method button

## [0.0.5] - 2021-11-28
### Changed
- ShowIfNull and HideIfNull attributes now also work for property

## [0.0.4] - 2021-11-28
### Fixed
- Nested Group's NestingLevel value

### Changed
- Class now has namespaces

## [0.0.3] - 2021-11-28
### Fixed
- Nested Group system

## [0.0.2] - 2021-11-27
### Fixed
- Fixed the bug where base class serializable private/protected fields are not retrieved.

## [0.0.1] - 2021-11-26
### Changed
- Initial release.  

### Added
- Collection of built-in attributes with the capability of extension