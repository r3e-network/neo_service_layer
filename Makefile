.PHONY: build test clean lint

# Go parameters
GOCMD=go
GOBUILD=$(GOCMD) build
GOCLEAN=$(GOCMD) clean
GOTEST=$(GOCMD) test
GOGET=$(GOCMD) get
GOMOD=$(GOCMD) mod
BINARY_NAME=neo-service-layer
CLI_BINARY=cli

# Build parameters
BUILD_DIR=bin
MAIN_PKG=./cmd/server
CLI_PKG=./cmd/cli

all: test build

build: 
	mkdir -p $(BUILD_DIR)
	$(GOBUILD) -o $(BUILD_DIR)/$(BINARY_NAME) $(MAIN_PKG)
	$(GOBUILD) -o $(BUILD_DIR)/$(CLI_BINARY) $(CLI_PKG)

test: 
	$(GOTEST) -v ./...

clean: 
	$(GOCLEAN)
	rm -rf $(BUILD_DIR)

run:
	$(GOBUILD) -o $(BUILD_DIR)/$(BINARY_NAME) $(MAIN_PKG)
	./$(BUILD_DIR)/$(BINARY_NAME)

deps:
	$(GOMOD) download

lint:
	golangci-lint run

# Docker targets
docker-build:
	docker build -t neo-service-layer .

docker-run:
	docker run -p 8080:8080 neo-service-layer

# Development targets
dev: build
	./$(BUILD_DIR)/$(BINARY_NAME) --config config.yaml

dev-cli: build
	./$(BUILD_DIR)/$(CLI_BINARY) --config config.yaml

# Generate targets
generate:
	go generate ./...

# Database targets
migrate:
	go run cmd/migrate/main.go

# Test targets
test-coverage:
	$(GOTEST) -coverprofile=coverage.out ./...
	go tool cover -html=coverage.out

# Benchmark targets
bench:
	$(GOTEST) -bench=. ./...