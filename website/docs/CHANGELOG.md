# Changelog

All notable changes to the Price Feed Service will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Market regime detection framework
- Cross-chain price verification module
- Advanced anomaly detection system

## [1.2.0] - 2024-03-20

### Added
- Multi-dimensional Kalman filter state tracking
- Velocity and acceleration estimation
- Enhanced monitoring metrics for state variables
- Matrix operations utility functions
- Improved documentation for multi-state tracking

### Changed
- Refactored Kalman filter implementation for better maintainability
- Updated state transition model to include acceleration
- Enhanced metric recording for multi-state variables

### Fixed
- Matrix multiplication type errors
- State vector initialization issues
- Covariance matrix update logic

## [1.1.0] - 2024-03-19

### Added
- Adaptive noise parameter tuning for Kalman filter
- Innovation-based measurement noise estimation
- Dynamic process noise adjustment
- Extended monitoring metrics for noise adaptation
- Comprehensive documentation for noise parameter tuning

### Changed
- Improved Kalman filter accuracy through adaptive parameters
- Enhanced error handling for parameter adaptation
- Updated configuration interface for noise parameters

### Fixed
- Noise parameter boundary conditions
- Innovation variance calculation
- Metric recording for noise parameters

## [1.0.0] - 2024-03-18

### Added
- Initial release of the Price Feed Service
- Multi-source price aggregation
- Basic Kalman filtering
- Historical accuracy tracking
- Prometheus metrics integration
- Structured logging
- Rate limiting
- Error handling
- Caching system
- Documentation

### Security
- TEE integration
- Secure API key management
- Input validation
- Rate limiting protection

## [0.2.0] - 2024-03-15

### Added
- Price aggregation algorithm
- Source weight calculation
- Statistical analysis
- Outlier detection
- Basic monitoring

### Changed
- Improved error handling
- Enhanced logging format
- Updated configuration structure

## [0.1.0] - 2024-03-10

### Added
- Project initialization
- Basic price fetching
- Simple averaging
- Error handling framework
- Initial documentation

[Unreleased]: https://github.com/neo-project/neo-service-layer/compare/v1.2.0...HEAD
[1.2.0]: https://github.com/neo-project/neo-service-layer/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/neo-project/neo-service-layer/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/neo-project/neo-service-layer/compare/v0.2.0...v1.0.0
[0.2.0]: https://github.com/neo-project/neo-service-layer/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/neo-project/neo-service-layer/releases/tag/v0.1.0 